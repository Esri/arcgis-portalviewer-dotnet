// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System.Collections.Generic;
using Windows.System;
using Windows.UI.ViewManagement;
using ArcGISPortalViewer.Common;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using ArcGISPortalViewer.View;
using Esri.ArcGISRuntime.Portal;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Activation;

namespace ArcGISPortalViewer.ViewModel
{
    public class AppViewModel : ViewModelBase
    {
        #region Private Members

        private Frame _rootFrame;
        private LaunchActivatedEventArgs _args;                

        #endregion Private Members

        #region Constructors

        public AppViewModel()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            _currentAppViewModel = this;
        }

        #endregion Constructors               

        #region Public Properties

        #region CurrentAppViewModel
        private static AppViewModel _currentAppViewModel;
        public static AppViewModel CurrentAppViewModel
        {
            get { return _currentAppViewModel ?? (_currentAppViewModel = new AppViewModel()); }
        }
        
        #endregion CurrentAppViewModel

        #region IsNetworkConnectionAvaliable

        public static bool IsNetworkConnectionAvaliable { get; set; }

        #endregion IsNetworkConnectionAvaliable        

        #region SelectedPortalItem

        private const string SelectedPortalItemPropertyName = "SelectedPortalItem";
        private ArcGISPortalItem _selectedPortalItem;
        /// <summary>
        /// Sets and gets the SelectedPortalItem property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ArcGISPortalItem SelectedPortalItem
        {
            get
            {
                return _selectedPortalItem;
            }
            set
            {
                Set(SelectedPortalItemPropertyName, ref _selectedPortalItem, value);
                RaisePropertyChanged(() => IsSelectedItemInFavorites);
            }
        }        

        #endregion SelectedPortalItem
              
        #region IsSelectedItemInFavorites

        public bool IsSelectedItemInFavorites
        {
            get
            {                
                if (SelectedPortalItem == null || FavoritesService.CurrentFavoritesService.Favorites == null)
                    return false;

                return FavoritesService.CurrentFavoritesService.Favorites.Any(pivm => pivm.Id == SelectedPortalItem.Id);
            }
        }

        #endregion IsSelectedItemInFavorites

        #region IsPinningTile

        private const string IsPinningTilePropertyName = "IsPinningTile";
        private bool _isPinningTile;
        /// <summary>
        /// Sets and gets the IsPinningTile property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPinningTile
        {
            get
            {
                return _isPinningTile;
            }
            set
            {
                Set(IsPinningTilePropertyName, ref _isPinningTile, value);
            }
        }        

        #endregion IsPinningTile        

        #region LinearUnitTypeSource

        private IEnumerable<LinearUnitType> _linearUnitTypeSource;
        /// <summary>
        /// Gets an enumeration of supported linear unit types.
        /// </summary>
        public IEnumerable<LinearUnitType> LinearUnitTypeSource
        {
            get
            {
                return _linearUnitTypeSource ?? (_linearUnitTypeSource = new[]
                {
                    LinearUnitType.Metric,
                    LinearUnitType.ImperialUS
                });
            }
        }        

        #endregion LinearUnitTypeSource

        #region CoordinateFormatSource

        private IEnumerable<CoordinateFormat> _coordinateFormatSource;
        /// <summary>
        /// Gets an enumeration of supported coordinate format source
        /// </summary>
        public IEnumerable<CoordinateFormat> CoordinateFormatSource
        {
            get
            {
                return _coordinateFormatSource ?? (_coordinateFormatSource = new[]
                {
                    CoordinateFormat.DecimalDegrees,
                    CoordinateFormat.Dms,
                    CoordinateFormat.DegreesDecimalMinutes,
                    CoordinateFormat.Mgrs
                });
            }
        }        

        #endregion CoordinateFormatSource

        #region LinearUnitType

        private LinearUnitType _linearUnitType = LinearUnitType.Metric;
        /// <summary>
        /// Gets or sets <see cref="LinearUnitType"/>
        /// </summary>
        public LinearUnitType LinearUnitType
        {
            get { return _linearUnitType; }
            set
            {
                if (_linearUnitType != value)
                {
                    _linearUnitType = value;
                    base.RaisePropertyChanged("LinearUnitType");
                }
            }
        }        

        #endregion LinearUnitType

        #region CoordinateFormat

        private CoordinateFormat _coordinateFormat = CoordinateFormat.DecimalDegrees;
        /// <summary>
        /// Gets or sets <see cref="CoordinateFormat"/>
        /// </summary>  
        public CoordinateFormat CoordinateFormat
        {
            get { return _coordinateFormat; }
            set
            {
                if (_coordinateFormat != value)
                {
                    _coordinateFormat = value;
                    base.RaisePropertyChanged("CoordinateFormat");
                }
            }
        }        

        #endregion CoordinateFormat

        #endregion Public Properties

        #region Public Commands

        #region TryAgainCommand

        private RelayCommand _tryAgainCommand;
        /// <summary>
        /// Gets the TryAgainCommand.
        /// </summary>
        public RelayCommand TryAgainCommand
        {
            get
            {
                return _tryAgainCommand ?? (_tryAgainCommand = new RelayCommand(() =>
                {
                    if (!CheckNetworkAvailability())
                        Window.Current.Activate();
                    else
                    {
                        var _ = AppInit(_rootFrame, _args);
                    }
                }));
            }
        }        

        #endregion TryAgainCommand        

        #region PinToStartCommand

        private RelayCommand<object> _pinToStartCommand;
        /// <summary>
        /// Gets the PinToStartCommand.
        /// </summary>
        public RelayCommand<object> PinToStartCommand
        {
            get
            {
                return _pinToStartCommand ?? (_pinToStartCommand = new RelayCommand<object>(ExecutePinToStartCommand));
            }
        }        

        private void ExecutePinToStartCommand(object sender)
        {
            var element = sender as FrameworkElement;
            if (element != null && SelectedPortalItem != null)
            {
                GeneralTransform buttonTransform = element.TransformToVisual(null);
                Point point = buttonTransform.TransformPoint(new Point());
                var promptRect = new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
                CreatePinTile(SelectedPortalItem, promptRect);
            }
        }

        #endregion PinToStartCommand

        #region AddToFavoritesCommand

        private RelayCommand _addToFavoritesCommand;
        /// <summary>
        /// Gets the AddToFavoritesCommand.
        /// </summary>
        public RelayCommand AddToFavoritesCommand
        {
            get
            {
                return _addToFavoritesCommand ?? (_addToFavoritesCommand = new RelayCommand(ExecuteAddToFavoritesCommand));
            }
        }        

        private void ExecuteAddToFavoritesCommand()
        {
            // send a message to other FavoritesViewModel who is registered with AddItemToFavoritesMessage
            Messenger.Default.Send(new AddItemToFavoritesMessage { Item = SelectedPortalItem });
            RaisePropertyChanged(() => IsSelectedItemInFavorites);
        } 

        #endregion AddToFavoritesCommand

        #region RemoveFromFavoritesCommand

        private RelayCommand _removeFromFavoritesCommand;
        public RelayCommand RemoveFromFavoritesCommand
        {
            get
            {
                return _removeFromFavoritesCommand ?? (_removeFromFavoritesCommand = new RelayCommand(() =>
                {
                    // send a message to other FavoritesViewModel who is registered with RemoveItemFromFavoritesMessage
                    Messenger.Default.Send(new RemoveItemFromFavoritesMessage { Item = SelectedPortalItem });
                    RaisePropertyChanged(() => IsSelectedItemInFavorites);
                }));
            }
        }        

        #endregion RemoveFromFavoritesCommand

        #region WebMapsQuerySubmittedCommand

        private RelayCommand<object> _webMapsQuerySubmittedCommand;
        /// <summary>
        /// Gets the WebMapsQuerySubmittedCommand.
        /// </summary>
        public RelayCommand<object> WebMapsQuerySubmittedCommand
        {
            get
            {
                return _webMapsQuerySubmittedCommand ?? (_webMapsQuerySubmittedCommand = new RelayCommand<object>(
                    ExecuteWebMapsQuerySubmittedCommand,
                    CanExecuteWebMapsQuerySubmittedCommand));
            }
        }        

        private void ExecuteWebMapsQuerySubmittedCommand(object obj)
        {
            if (!(obj is SearchBoxQuerySubmittedEventArgs))
                return;

            var queryText = ((SearchBoxQuerySubmittedEventArgs)obj).QueryText;
            (new NavigationService()).Navigate(App.SearchPageName, queryText);
        }

        private bool CanExecuteWebMapsQuerySubmittedCommand(object obj)
        {
            return true;
        } 

        #endregion WebMapsQuerySubmittedCommand

        #region OpenMapCommand

        private RelayCommand<ArcGISPortalItem> _openMapCommand;
        /// <summary>
        /// Gets the OpenMapCommand.
        /// </summary>
        public RelayCommand<ArcGISPortalItem> OpenMapCommand
        {
            get
            {
                return _openMapCommand ?? (_openMapCommand = new RelayCommand<ArcGISPortalItem>(
                    ExecuteOpenMapCommand,
                    CanExecuteOpenMapCommand));
            }
        }
        
        private void ExecuteOpenMapCommand(ArcGISPortalItem portalItem)
        {
            if (portalItem == null)
            {
                throw new Exception("ArcGIS Portal Item is null or not selected");
            }

            (new NavigationService()).Navigate(App.MapPageName, portalItem);
        }

        private bool CanExecuteOpenMapCommand(ArcGISPortalItem portalItem)
        {
            if (portalItem != null && portalItem.Type == ItemType.WebMap)
                return true;
            return false;
        }        

        #endregion OpenManCommand

        #region OnHyperlinkNavigationCommand

        private RelayCommand<object> _hyperlinkNavigationCommand;
        public RelayCommand<object> HyperlinkNavigationCommand
        {
            get
            {
                return _hyperlinkNavigationCommand ?? (_hyperlinkNavigationCommand = new RelayCommand<object>(ExecuteHyperlinkNavigation));
            }
        }        

        private async void ExecuteHyperlinkNavigation(object obj)
        {
            if (obj is NotifyEventArgs)
            {
                var e = ((NotifyEventArgs)obj);
                var isUri = Uri.IsWellFormedUriString(e.Value, UriKind.RelativeOrAbsolute);
                if (isUri)
                {
                    // Search Tag
                    if (e.Value.Contains("arcgis://search/"))
                    {
                        var search = e.Value.Split(new[] { "arcgis://search/" }, StringSplitOptions.None)[1];
                        search = Uri.UnescapeDataString(search);
                        (new NavigationService()).Navigate(typeof(SearchPage), search);
                    }
                    else
                    {

                        // Launch Default Browser
                        var success = await Launcher.LaunchUriAsync(
                            new Uri(e.Value, UriKind.RelativeOrAbsolute),
                            new LauncherOptions { DesiredRemainingView = ViewSizePreference.UseHalf });
                    }
                }
            }
        }        

        #endregion OnHyperlinkNavigationCommand

        #endregion Public Commands
    
        #region Public Methods

        public async Task AppInit(Frame rootFrame, LaunchActivatedEventArgs args)
        {
            if (args == null) return;

            _args = args;
            _rootFrame = rootFrame;

            // if network connection is not available, navigate to NetworkConnectivityPage 
            await Task.FromResult(CheckNetworkAvailability());
            if (!IsNetworkConnectionAvaliable)
            {
                if (!rootFrame.Navigate(typeof(NetworkConnectivityPage), args.Arguments))
                {
                    throw new Exception("Failed to navigate to network connectivity page");
                }
            }

            if (rootFrame.Content == null || IsNetworkConnectionAvaliable)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation parameter

                // initialize static properties
                App.OrganizationUrl = GetKeyValueFromResources("OrganizationUrl").ToLower();
                App.PortalUri = new UriBuilder(string.Format(App.SharingRest, App.OrganizationUrl));
                App.AppServerId = GetKeyValueFromResources("AppServerId");
                App.AppRedirectUri = GetKeyValueFromResources("AppRedirectUri");
                App.IsOrgOAuth2 = !string.IsNullOrEmpty(App.AppServerId) && !string.IsNullOrEmpty(App.AppRedirectUri);

                // if credentials were persisted or anonymous access is enabled navigate to main page.                 
                if (App.SignInVM.IsCredentialsPersisted) // || SignInVM.IsAnonymousAccessEnabledAsync())
                {
                    if (!(rootFrame.Content is MainPage))
                    {
                        if (!rootFrame.Navigate(typeof (MainPage), args.Arguments))
                        {
                            throw new Exception("Failed to create initial page");
                        }
                    }
                    // try signing in 
                    await App.SignInVM.TrySigningInAsync();
                }
                else
                {
                    // if Anonymous access is enabled also navigate to main page
                    bool b = await App.SignInVM.GetAnonymousAccessStatusAsync();
                    if (b)
                    {
                        if (!(rootFrame.Content is MainPage))
                        {
                            if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                            {
                                throw new Exception("Failed to create initial page");
                            }
                        }
                        // try signing in anonymously
                        await App.SignInVM.SignInAnonymouslyAsync();
                    }
                    // else challenge the user by navigating to the signing in page 
                    else
                    {
                        if (!rootFrame.Navigate(typeof(ArcGISBlankPage), args.Arguments))
                        {
                            throw new Exception("Failed to create login page");
                        }
                    }
                }
            }
        }

        public void ResetProperties()
        {
            SelectedPortalItem = null;
            IsPinningTile = false;
        }

        public async void CreatePinTile(ArcGISPortalItem portalItem, Rect tileProptRect)
        {
            //string nav = string.Format(@"arcgis://www.arcgis.com/sharing/rest/content/items/{0}/data", portalItem.Id);
            string nav = string.Format(@"arcgis:{0}/sharing/rest/content/items/{1}/data", App.OrganizationUrl, portalItem.Id);
            IsPinningTile = true;
            await TileService.CreateSecondaryTileFromWebImage(portalItem.Title, portalItem.Id,
                    portalItem.ThumbnailUri, tileProptRect, nav);
            IsPinningTile = false;
        }

        public static async Task ShowDialogAsync(string message, string title)
        {
            // make sure to call MessageDialog.ShowAsync on the ui thread.
            // force this by rescheduling the call using dispatcher.RunAync.
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var dialog = new Windows.UI.Popups.MessageDialog(message, title);
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Cancel", a => { }));
                await dialog.ShowAsync();
            });
        }

        #endregion Public Methods

        #region Private Methods

        private string GetKeyValueFromResources(string keyName)
        {
            var reskeys = Application.Current.Resources.Keys;
            if (string.IsNullOrEmpty(keyName) || reskeys == null || reskeys.Count == 0)
                return "";

            for (var i = 0; i < reskeys.Count; ++i)
            {
                if (reskeys.ElementAt(i).ToString() != keyName) continue;
                return Application.Current.Resources.Values.ElementAt(i).ToString();
            }

            return "";
        }                      

        private static bool CheckNetworkAvailability(NetworkConnectivityLevel minimumLevelRequired = NetworkConnectivityLevel.InternetAccess)
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();
            IsNetworkConnectionAvaliable = (profile != null && profile.GetNetworkConnectivityLevel() >= minimumLevelRequired);
            return IsNetworkConnectionAvaliable;
        }

        #endregion Private Methods

        #region Private Events

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            if (!CheckNetworkAvailability())
            {
                var _ = ShowDialogAsync("Network connectinvity is below required level. Please make sure internet access is available.", "Internet Connection Warning!");
            }
        }

        #endregion Private Events               
    }    
}
