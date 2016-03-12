// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Esri.ArcGISRuntime.Portal;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ArcGISPortalViewer.Controls;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ArcGISPortalViewer.ViewModel
{
    public class CollectionAndTitle : ViewModelBase
    {
        private ObservableCollection<object> _collection;
        public ObservableCollection<object> Collection
        {
            get { return _collection; }
            set
            {
                if (value != null)
                {
                    _collection = value; // value.Take(3);
                    RaisePropertyChanged(() => Collection);
                }
            }
        }
        public string Title { get; set; }
        public CollectionAndTitle(ObservableCollection<object> collection, string title)
        {
            Collection = collection;
            Title = title;
        }
    }


    /// <summary>
    /// This class contains properties that the main View can databind to.
    /// <para>
    /// it implements MVVM pattern by adopting MVVM light - see http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        #region Private Properties        
        private INavigationService _navigationService;
        private ArcGISPortalItem _selectedPortalItem;
        private PortalGroupViewModel _selectedPortalGroup;
        private IncremetalLoadingCollection _selectedPortalItemCollection;
        private bool _isLoadingData = false;
        #endregion

        #region Public Properties         
        public IncremetalLoadingCollection SelectedPortalItemCollection
        {
            get { return _selectedPortalItemCollection; }
            set
            {
                if (_selectedPortalItemCollection == value) return;
                _selectedPortalItemCollection = value;
                RaisePropertyChanged(() => SelectedPortalItemCollection);
            }
        }

        /// <summary>
        /// The <see cref="PortalService" /> property's name.
        /// </summary>
        public const string PortalServicePropertyName = "PortalService";

        private PortalService _portalService = null;

        /// <summary>
        /// Sets and gets the PortalService property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PortalService PortalService
        {
            get
            {
                return _portalService;
            }
            set
            {
                // RaisePropertyChanging(PortalServicePropertyName); // See https://mvvmlight.codeplex.com/workitem/7662
                _portalService = value;
                RaisePropertyChanged(PortalServicePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedItem" /> property's name.
        /// </summary>
        public const string SelectedPortalItemPropertyName = "SelectedPortalItem";
        /// <summary>
        /// The <see cref="SelectedGroup" /> property's name.
        /// </summary>
        public const string SelectedPortalGroupPropertyName = "SelectedPortalGroup";
        /// <summary>
        /// The <see cref="IsLoadingData" /> property's name.
        /// </summary>
        public const string IsLoadingDataPropertyName = "IsLoadingData";

        /// <summary>
        /// The <see cref="MyMaps" /> property's name.
        /// </summary>
        public const string MyMapsPropertyName = "MyMaps";

        private PortalItemCollection _myMaps = null;

        /// <summary>
        /// Sets and gets the MyMaps property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PortalItemCollection MyMaps
        {
            get { return _myMaps; }
            set { Set(MyMapsPropertyName, ref _myMaps, value); }
        }

        /// <summary>
        /// The <see cref="FeaturedItems" /> property's name.
        /// </summary>
        public const string FeaturedItemsPropertyName = "FeaturedItems";

        private IncremetalLoadingCollection _featuredItems = null;

        /// <summary>
        /// Sets and gets the FeaturedItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IncremetalLoadingCollection FeaturedItems
        {
            get
            {
                return _featuredItems;
            }
            set
            {
                Set(FeaturedItemsPropertyName, ref _featuredItems, value);
            }
        }

        /// <summary>
        /// The <see cref="RecentItems" /> property's name.
        /// </summary>
        public const string RecentItemsPropertyName = "RecentItems";

        private IncremetalLoadingCollection _recentItems = null;

        /// <summary>
        /// Sets and gets the RecentItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IncremetalLoadingCollection RecentItems
        {
            get
            {
                return _recentItems;
            }
            set
            {
                Set(RecentItemsPropertyName, ref _recentItems, value);
            }
        }

        /// <summary>
        /// The <see cref="MostPopularItems" /> property's name.
        /// </summary>
        public const string MostPopularItemsPropertyName = "MostPopularItems";

        private IncremetalLoadingCollection _mostPopularItems = null;

        /// <summary>
        /// Sets and gets the MostPopularItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IncremetalLoadingCollection MostPopularItems
        {
            get
            {
                return _mostPopularItems;
            }
            set
            {
                Set(MostPopularItemsPropertyName, ref _mostPopularItems, value);
            }
        }

        /// <summary>
        /// The <see cref="FavoritesItems" /> property's name.
        /// </summary>
        public const string FavoriteItemsPropertyName = "FavoriteItems";

        private FavoritesViewModel _favoriteItems = null;

        /// <summary>
        /// Sets and gets the FavoritesItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public FavoritesViewModel FavoriteItems
        {
            get
            {
                return _favoriteItems;
            }
            set
            {
                Set(FavoriteItemsPropertyName, ref _favoriteItems, value);
            }
        }

        /// <summary>
        /// The <see cref="PortalGroups" /> property's name.
        /// </summary>
        public const string PortalGroupsPropertyName = "PortalGroups";

        private PortalGroupCollection _portalGroups = null;

        /// <summary>
        /// Sets and gets the PortalGroups property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PortalGroupCollection PortalGroups
        {
            get
            {
                return _portalGroups;
            }
            set
            {
                Set(PortalGroupsPropertyName, ref _portalGroups, value);
            }
        }

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
                if (Set(SelectedPortalItemPropertyName, ref _selectedPortalItem, value)
                    && value != null)
                {
                    _selectedPortalItem = value;

                    try
                    {
                        Messenger.Default.Send<ChangeItemSelectedMessage>(new ChangeItemSelectedMessage() { Item = _selectedPortalItem });
                        _navigationService.Navigate(App.ItemPageName, _selectedPortalItem);
                    }
                    catch (Exception ex)
                    {
                        var _ = App.ShowExceptionDialog(ex.InnerException);
                    }
                }
            }
        }
        /// <summary>
        /// Sets and gets the SelectedPortalGroup property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PortalGroupViewModel SelectedPortalGroup
        {
            get
            {
                return _selectedPortalGroup;
            }
            set
            {
                if (Set(SelectedPortalGroupPropertyName, ref _selectedPortalGroup, value)
                    && value != null)
                {
                    _selectedPortalGroup = value;
                    Messenger.Default.Send<ChangeGroupSelectedMessage>(new ChangeGroupSelectedMessage() { Group = _selectedPortalGroup.PortalGroup });
                    _navigationService.Navigate(App.GroupPageName);
                }
            }
        }
        /// <summary>
        /// Sets and gets the IsLoadingData property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLoadingData
        {
            get
            {
                return _isLoadingData;
            }
            set
            {
                Set(IsLoadingDataPropertyName, ref _isLoadingData, value);
            }
        }
        #endregion

        #region Commands

        private RelayCommand _signInCommand;
        public RelayCommand SignInCommand { get { return _signInCommand ?? (_signInCommand = new RelayCommand(ExecuteSignInCommand)); } }
        private RelayCommand _signOutCommand;
        public RelayCommand SignOutCommand { get { return _signOutCommand ?? (_signOutCommand = new RelayCommand(ExecuteSignOutCommand)); } }

        public RelayCommand<object> ItemClickCommand { get; set; }
        public RelayCommand<object> MoreClickCommand { get; set; }

        #endregion

        #region Constructor
        public MainViewModel(INavigationService navigationService)
        {
            if (navigationService == null)
                throw new ArgumentNullException("navigationService");
            else
                _navigationService = navigationService;

            try
            {
                Task t = Initialize();
            }
            catch { }

#if DEBUG
            CreateDesignTimeData();
#endif
        }

        private async Task Initialize()
        {
            try
            {
                _portalService = PortalService.CurrentPortalService as PortalService;

                InitializeCommandAndMessages();

                if (_portalService.Portal != null)
                {
                    var _ = await PopulateDataAsync();
                }
            }
            catch (Exception ex)
            {
                var _ = App.ShowExceptionDialog(ex);
            }
        }

        private async Task<bool> PopulateDataAsync()
        {
            try
            {
                // fill featured maps collection
                IncremetalLoadingCollection.getMore getMoreItemsAsync0 = FeaturedQueryAsync;
                FeaturedItems = new IncremetalLoadingCollection(getMoreItemsAsync0);
                FeaturedItems.Title = "Featured";

                // fill most recent maps collection
                IncremetalLoadingCollection.getMore getMoreItemsAsync1 = RecentQueryAsync;
                RecentItems = new IncremetalLoadingCollection(getMoreItemsAsync1);
                RecentItems.Title = "Most Recent";

                // fill most popular maps collection  
                IncremetalLoadingCollection.getMore getMoreItemsAsync2 = HighestRatedQueryAsync;
                MostPopularItems = new IncremetalLoadingCollection(getMoreItemsAsync2);
                MostPopularItems.Title = "Most Popular";

                // fill favorite maps collection  
                FavoriteItems = new FavoritesViewModel();
                FavoriteItems.Title = "Favorites";

                // fill the groups collection
                var portalGroupItems = await GetPortalGroupsAsync();
                if (portalGroupItems != null)
                    PortalGroups = new PortalGroupCollection(portalGroupItems, "Groups");
                else
                    PortalGroups = null;

                // fill the user's maps collection
                var myMaps = await GetMyMapsAsync();
                if (myMaps != null)
                    MyMaps = new PortalItemCollection(myMaps, "My Maps");
                else
                    MyMaps = null;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void InitializeCommandAndMessages()
        {
            // initialize ItemClick RelayCommand
            ItemClickCommand = new RelayCommand<object>((e) =>
            {
                // handle the type of EventArgs passed
                if (e == null)
                    return;

                ArcGISPortalItem portalItem = null;
                // the GridView sends ItemClickEventArgs
                if (e.GetType() == typeof(ItemClickEventArgs))
                    portalItem = ((ItemClickEventArgs)e).ClickedItem as ArcGISPortalItem;
                // our GalleryPreviewControl sends TileClickEventArgs
                else if (e.GetType() == typeof(TileClickEventArgs))
                    portalItem = ((TileClickEventArgs)e).ClickedTile as ArcGISPortalItem;

                if (portalItem != null)
                {
                    // send clicked item via a message to other ViewModels who
                    // are registered with ChangeItemSelectedMessage
                    Messenger.Default.Send<ChangeItemSelectedMessage>(new ChangeItemSelectedMessage() { Item = portalItem });
                    //Use the navigation service to navigate to the page showing the item details                    
                    _navigationService.Navigate(App.ItemPageName, portalItem);
                }
                else // check if it is a PortalGroup
                {
                    ArcGISPortalGroup portalGroup = null;
                    // the GridView sends ItemClickEventArgs
                    if (e.GetType() == typeof(ItemClickEventArgs))
                        portalGroup = ((ItemClickEventArgs)e).ClickedItem as ArcGISPortalGroup;
                    // our GalleryPreviewControl sends TileClickEventArgs
                    else if (e.GetType() == typeof(TileClickEventArgs))
                        portalGroup = ((TileClickEventArgs)e).ClickedTile as ArcGISPortalGroup;

                    if (portalGroup != null)
                    {
                        // send clicked item via a message to other ViewModels who
                        // are registered with ChangeGroupSelectedMessage
                        Messenger.Default.Send<ChangeGroupSelectedMessage>(new ChangeGroupSelectedMessage() { Group = portalGroup });
                        //Use the navigation service to navigate to the page showing the item details
                        _navigationService.Navigate(App.GroupPageName);
                    }
                }
            });

            // initialize MoreClickCommand RelayCommand
            MoreClickCommand = new RelayCommand<object>((objectCollection) =>
            {
                if (objectCollection == null)
                    return;

                //Use the navigation service to navigate to the page showing the specific collection of portal items                
                _navigationService.Navigate(App.CollectionPageName, objectCollection);
            });

            // Register with  PopulateDataMessage 
            Messenger.Default.Register<PopulateDataMessage>(this, msg => { var _ = PopulateDataAsync(); });

            // Register with ChangedPortalServiceMessage 
            Messenger.Default.Register<ChangedPortalServiceMessage>(this, msg => { var _ = PopulateDataAsync(); });
        }
        #endregion

        #region Private Methods

        private async Task<IEnumerable<ArcGISPortalItem>> GetMyMapsAsync()
        {
            var ps = PortalService.CurrentPortalService;
            if (PortalService.CurrentPortalService.CurrentUser == null)
                return null;

            // modify query string to get the maps owned by the current user
            SearchParameters searchParam = SearchService.CreateSearchParameters("", PortalQuery.MyMaps, 1, 100);
            searchParam.QueryString = string.Format(" ({0}) AND (owner: {1}) ", searchParam.QueryString, ps.CurrentUser.UserName);

            IsLoadingData = true;
            SearchResultInfo<ArcGISPortalItem> r = await ps.GetSearchResults(searchParam);
            IsLoadingData = false;
            if (r == null) return null;
            return r.Results;
        }

        private Task<IEnumerable<object>> FeaturedQueryAsync(uint count)
        {
            SearchParameters sp = SearchService.CreateSearchParameters("", PortalQuery.Default, FeaturedItems.Count + 1, Math.Min((int)count, 100));
            return GetFeaturedItemsAsync(sp);
        }

        private async Task<IEnumerable<object>> GetFeaturedItemsAsync(SearchParameters searchParameters)
        {
            IsLoadingData = true;
            var r = await PortalService.CurrentPortalService.GetFeaturedItems(searchParameters);
            IsLoadingData = false;
            return r;
        }

        private Task<IEnumerable<object>> PortalQueryAsync(uint count)
        {
            SearchParameters sp = SearchService.CreateSearchParameters("", PortalQuery.Default, FeaturedItems.Count + 1, Math.Min((int)count, 100));
            return GetPortalItemsAsync(sp);
        }

        private async Task<IEnumerable<object>> RecentQueryAsync(uint count)
        {
            SearchParameters sp = SearchService.CreateSearchParameters("", PortalQuery.Recent, RecentItems.Count + 1, Math.Min((int)count, 100));
            return await GetPortalItemsAsync(sp);
        }

        private async Task<IEnumerable<object>> HighestRatedQueryAsync(uint count)
        {
            SearchParameters sp = SearchService.CreateSearchParameters("", PortalQuery.HighestRated, MostPopularItems.Count + 1, Math.Min((int)count, 100));
            return await GetPortalItemsAsync(sp);
        }

        private async Task<IEnumerable<object>> GetPortalItemsAsync(SearchParameters sp)
        {
            IsLoadingData = true;
            var ps = PortalService.CurrentPortalService;
            SearchResultInfo<ArcGISPortalItem> r = await ps.GetSearchResults(sp);
            IsLoadingData = false;
            if (r == null) return null;
            return r.Results;
        }

        private async Task<IList<ArcGISPortalGroup>> GetPortalGroupsAsync()
        {
            IsLoadingData = true;
            var r = await PortalService.CurrentPortalService.GetGroups();
            IsLoadingData = false;
            return r;
        }

        private async void PopulatePortalItemCollection(ObservableCollection<ArcGISPortalItem> portalCollection, PortalQuery portalQuery)
        {
            if (portalCollection == null || portalQuery == PortalQuery.MyGroups)
                return;

            FavoritesService currentFavoritesService = new FavoritesService();
            await currentFavoritesService.SetFavoritesCollection();

            SearchParameters sp = null;
            if (portalQuery == PortalQuery.Favorites)
                sp = SearchService.CreateSearchParameters("", portalQuery, 0, 20, currentFavoritesService.GetFavoritesIds());
            else
                sp = SearchService.CreateSearchParameters("", portalQuery);

            IsLoadingData = true;
            IList<ArcGISPortalItem> portalItems = await PortalService.CurrentPortalService.GetPortalItems(sp);

            if (portalItems != null)
            {
                portalCollection.Clear();

                foreach (ArcGISPortalItem pi in portalItems)
                {
                    portalCollection.Add(pi);
                }
            }
            IsLoadingData = false;
        }

        private void ExecuteSignInCommand()
        {
            if (!App.SignInVM.IsCredentialsPersisted)
                Messenger.Default.Send<ChangeSignInMessage>(new ChangeSignInMessage() { });
        }

        private async void ExecuteSignOutCommand()
        {
            Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(string.Format("Do you want to sign out from {0}?", string.IsNullOrEmpty(PortalService.CurrentPortalService.OrganizationName) ? "ArcGIS.com" : PortalService.CurrentPortalService.OrganizationName));
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Sign out", (a) =>
            {
                // clear all observable collections
                ClearAllCollections();

                // signal changes on PortalService properties
                RaisePropertyChanged(PortalServicePropertyName);

                // sign out the current portal connection 
                Messenger.Default.Send<ChangeSignOutMessage>(new ChangeSignOutMessage() { });
            }));
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Cancel", (a) => { }));
            await dialog.ShowAsync();
        }

        private void ClearAllCollections()
        {
            try
            {
                if (MyMaps != null) MyMaps.Clear();
                if (FeaturedItems != null) FeaturedItems.Clear();
                if (RecentItems != null) RecentItems.Clear();
                if (MostPopularItems != null) MostPopularItems.Clear();
                if (FavoriteItems != null && (FavoriteItems.Items != null && FavoriteItems.Items.Count > 0))
                    FavoriteItems.Items.Clear();
                if (PortalGroups != null) PortalGroups.Clear();
            }
            catch (Exception ex)
            {
                var _ = App.ShowExceptionDialog(ex);
            }
        }
        #endregion


#if DEBUG
        private void CreateDesignTimeData()
        {
            if (IsInDesignMode)
            {
                Uri tn = new Uri("http://www.arcgis.com/sharing/rest/content/items/8b3b470883a744aeb60e5fff0a319ce7/info/thumbnail/templight_gray_canvas_with_labels__ne_usa.png");
                var image = new BitmapImage(tn);
                PortalItem pi = new PortalItem("DebugModelTile", image);
                SelectedPortalItem = pi.ArcGISPortalItem;
            }
        }
#endif
    }
}
