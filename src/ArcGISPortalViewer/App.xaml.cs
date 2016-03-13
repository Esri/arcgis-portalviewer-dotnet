using ArcGISPortalViewer.Common;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using ArcGISPortalViewer.View;
using ArcGISPortalViewer.ViewModel;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ArcGISPortalViewer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public const string SharingRest = "{0}/sharing/rest";
        private static SignInViewModel _signInViewModel { get; set; }
        public static SignInViewModel SignInVM
        {
            get
            {
                if (_signInViewModel == null)
                    _signInViewModel = new SignInViewModel();
                return _signInViewModel;
            }
        }

        private static FavoritesService _favoriteService { get; set; }
        public static FavoritesService CurrentFavoritesService
        {
            get
            {
                if (_favoriteService == null)
                {
                    _favoriteService = new FavoritesService();
                    var _ = _favoriteService.SetFavoritesCollection();
                }
                return _favoriteService;
            }
        }

        // Set view/page names - used for navigation purposes.
        // Different projects using same ViewModels will only need to change these names
        public static string NetworkConnectivityPageName = "ArcGISPortalViewer.View.NetworkConnectivityPage";
        public static string BlankPageName = "ArcGISPortalViewer.View.ArcGISBlankPage";
        public static string MainPageName = "ArcGISPortalViewer.View.MainPage";
        public static string MapPageName = "ArcGISPortalViewer.View.MapPage";
        public static string CollectionPageName = "ArcGISPortalViewer.View.PortalCollectionPage";
        public static string GroupPageName = "ArcGISPortalViewer.View.PortalGroupPage";
        public static string ItemPageName = "ArcGISPortalViewer.View.PortalItemPage";
        public static string SearchPageName = "ArcGISPortalViewer.View.SearchPage";

        // Define global properties 
        public static string OrganizationUrl { get; set; }
        public static UriBuilder PortalUri { get; set; }
        public static string AppServerId { get; set; }
        public static string AppRedirectUri { get; set; }
        public static bool IsOrgOAuth2 { get; set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(Microsoft.ApplicationInsights.WindowsCollectors.Metadata | Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            // Initializes application's properties and navigates to the appropriate page after checking internet connection.
            await AppViewModel.CurrentAppViewModel.AppInit(rootFrame, e);

            // Ensure the current window is active
            Window.Current.Activate();

            DispatcherHelper.Initialize();

            // handle lauching the app by secondary tiles:
            // in this case the args passed should start with "arcgis".
            if (e.Arguments.StartsWith("arcgis:"))
            {
                LaunchMapByID(new Uri(e.Arguments, UriKind.RelativeOrAbsolute));
            }
        }

        public static async System.Threading.Tasks.Task ShowExceptionDialog(Exception ex, string messageTitle = "Exception", [System.Runtime.CompilerServices.CallerMemberName]string memberName = "")
        {
            #if DEBUG
                messageTitle = string.Format("{0} in: {1}.", messageTitle, memberName);
            #endif

            // make sure to call MessageDialog.ShowAsync on the ui thread.
            // force this by rescheduling the call using dispatcher.RunAync.
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            if (dispatcher == null) return;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var dialog = new Windows.UI.Popups.MessageDialog(ex.Message, messageTitle);
                if (dialog.Commands == null) return;
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Cancel", a => { }));
                var _ = dialog.ShowAsync();
            });
        }

        public static bool IsItemInFavorites(Esri.ArcGISRuntime.Portal.ArcGISPortalItem portalItem)
        {
            if (portalItem == null || CurrentFavoritesService.Favorites == null)
                return false;

            return CurrentFavoritesService.Favorites.Any(pivm => pivm.Id == portalItem.Id);
        }

        private void LaunchMapByID(Uri uri)
        {
            //arcgis://www.arcgis.com/sharing/rest/content/items/6929c43e567647fb85e261beee7f5774/data
            string uriLP = uri.LocalPath.ToLower();
            if (!string.IsNullOrEmpty(uriLP) && uriLP.IndexOf(OrganizationUrl.ToLower()) != -1)
            {
                string[] strArray = uriLP.Split(new string[] { "/", "\\" }, StringSplitOptions.None);
                if (strArray != null && strArray.Count() > 5)
                {
                    // get the item id located before the last string i.e. "/data" 
                    string itemID = strArray[strArray.Count() - 2];
                    Action loadMap = async () =>
                    {
                        var portal = PortalService.CurrentPortalService.Portal;
                        if (portal == null)
                            return;

                        var result = await portal.SearchItemsAsync(new Esri.ArcGISRuntime.Portal.SearchParameters() { QueryString = "id: " + itemID });
                        if (result.Results != null && result.Results.Any())
                        {
                            AppViewModel.CurrentAppViewModel.SelectedPortalItem = result.Results.FirstOrDefault();
                            (new NavigationService()).Navigate(typeof(MapPage));
                        }
                    };
                    loadMap();
                }
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {   
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
