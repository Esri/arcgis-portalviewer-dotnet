using Windows.UI.Xaml;
using Esri.ArcGISRuntime.Portal;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.Text.RegularExpressions;

namespace ArcGISPortalViewer.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        private int _totalHits = 0;

        public RelayCommand<object> ItemClickCommand { get; set; }
        public RelayCommand<object> SelectItemCommand { get; set; }
        public RelayCommand<object> SelectSortFieldCommand { get; set; }
        public RelayCommand<object> SelectSearchDomainCommand { get; set; }
        
        public enum SortField
        {
            None = 0,
            HighestRated = 1,
            MostPopular = 2,
            MostComments = 3,
            Recent = 4,
            Title = 5,
            Owner = 6
        }

        public static PortalQuery GetPortalQuery(SortField sortField)
        {
            switch (sortField)
            {
                case SortField.None:
                    return PortalQuery.Default; // no sort field
                case SortField.HighestRated:
                    return PortalQuery.HighestRated; //"avgrating";
                case SortField.MostPopular:
                    return PortalQuery.MostPopular; //"numviews";
                case SortField.MostComments:
                    return PortalQuery.MostComments; //"numcomments";
                case SortField.Recent:
                    return PortalQuery.Recent; //"uploaded";
                case SortField.Title:
                    return PortalQuery.Title; //"title";
                case SortField.Owner:
                    return PortalQuery.Owner; //"owner";
                default:
                    return PortalQuery.Default;
            }
        }

        public void SortResults(SortField sortField)
        {
            // set portal query based on sort field
            SetQuery(GetPortalQuery(sortField));

            // refresh search results            
            UpdateResults();
        }

        private SearchViewModel.SortField GetCurrentSortField(string sortByField)
        {
            if (string.IsNullOrEmpty(sortByField))
                return SortField.None; // Sort by relevance
            if (sortByField == (string) Application.Current.Resources["SortByTitle"])
                return SortField.Title;
            if (sortByField == (string) Application.Current.Resources["SortByOwner"])
                return SortField.Owner;
            if (sortByField == (string) Application.Current.Resources["SortByHighestRating"])
                return SortField.HighestRated;
            if (sortByField == (string) Application.Current.Resources["SortByMostRecent"])
                return SortField.Recent;
            if (sortByField == (string) Application.Current.Resources["SortByMostViews"])
                return SortField.MostPopular;

            return SortField.None; // Sort by relevance
        }
        
        /// <summary>
        /// The <see cref="CurrentSortField" /> property's name.
        /// </summary>
        public const string CurrentSortFieldPropertyName = "CurrentSortField";

        private SortField _currentSortField = SortField.HighestRated;

        /// <summary>
        /// Sets and gets the CurrentSortField property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SortField CurrentSortField
        {
            get
            {
                return _currentSortField;
            }
            set
            {
                Set(CurrentSortFieldPropertyName, ref _currentSortField, value);
            }
        }

        public ArcGISPortalItem SelectedPortalItem { get; set; }         

        /// <summary>
        /// The <see cref="SearchResults" /> property's name.
        /// </summary>
        public const string SearchResultsPropertyName = "SearchResults";

        private IncremetalLoadingCollection _searchResults;

        /// <summary>
        /// Sets and gets the SearchResults property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IncremetalLoadingCollection SearchResults
        {
            get
            {
                return _searchResults;
            }
            set
            {
                Set(SearchResultsPropertyName, ref _searchResults, value);
            }
        }
        
        public bool IsSearchDomainOptionsVisible
        {
            get
            {
                return (PortalService.CurrentPortalService.Portal != null && PortalService.CurrentPortalService.Portal.ArcGISPortalInfo.CanSearchPublic);
            }
        }

        public string SearchResultsTerm
        {
            get { return string.Format("Results for \"{0}\"", SearchQuery); }
        }

        public string SearchResultsNumber
        {
            get
            {
                return _totalHits == 0 ? "" : string.Format(_totalHits == 1 ? "{0} map" : "{0} maps", Convert.ToString(_totalHits));
            }
        }

        public const string SearchQueryPropertyName = "SearchQuery";
        private string _searchQuery = "";
        public string SearchQuery
        {
            get
            {
                return _searchQuery;
            }

            set
            {
                if (_searchQuery != value)
                {
                    RaisePropertyChanging(SearchQueryPropertyName);
                    _searchQuery = value;
                    UpdateResults();
                    RaisePropertyChanged(SearchQueryPropertyName);
                }
            }
        }

        public const string IsLoadingDataPropertyName = "IsLoadingData";
        private bool _isLoadingData = false;
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

        public const string NoResultsPropertyName = "NoResults";
        private bool _noResults = false;
        public bool NoResults
        {
            get
            {
                return _noResults;                
            }

            set
            {
                if (_noResults != value)
                {                    
                    _noResults = value;
                    RaisePropertyChanged(NoResultsPropertyName);
                }
            }
        }

        private PortalQuery _portalQuery = PortalQuery.Default;

        public SearchViewModel(INavigationService navigationService)
        {
            // initialize ItemClick RelayCommand
            ItemClickCommand = new RelayCommand<object>((e) =>
            {
                // handle the type of EventArgs passed
                ArcGISPortalItem portalItem = null;
                if (e == null)
                    portalItem = null;
                // the GridView sends ItemClickEventArgs
                else if (e is ItemClickEventArgs)
                    portalItem = ((ItemClickEventArgs)e).ClickedItem as ArcGISPortalItem;                

                // send clicked item via a message to other ViewModels who
                // are registered with ChangeItemSelectedMessage
                Messenger.Default.Send<ChangeItemSelectedMessage>(new ChangeItemSelectedMessage() { Item = portalItem });

                //Use the navigation service to navigate to the page showing the item details
                navigationService.Navigate(App.ItemPageName, portalItem);
            });

            // initialize SelectItemCommand RelayCommand
            SelectItemCommand = new RelayCommand<object>((e) =>
            {
                // handle the type of EventArgs passed
                ArcGISPortalItem portalItem = null;
                if (e == null)
                    portalItem = null;

                // the GridView sends SelectionChangedEventArgs
                else if (e.GetType() == typeof(SelectionChangedEventArgs))
                    portalItem = ((SelectionChangedEventArgs)e).AddedItems.FirstOrDefault() as ArcGISPortalItem;

                SelectedPortalItem = portalItem;
                AppViewModel.CurrentAppViewModel.SelectedPortalItem = SelectedPortalItem;
            });

            // initialise SelectSortFieldCommand RelayCommand
            SelectSortFieldCommand = new RelayCommand<object>((obj) =>
            {
                if (obj != null && obj is ComboBoxItem)
                {
                    var content = ((ComboBoxItem)obj).Content;
                    if (content != null)
                        CurrentSortField = GetCurrentSortField(content.ToString());
                    SortResults(CurrentSortField);
                }
            });

            // initialise SelectSearchDomainCommand RelayCommand
            SelectSearchDomainCommand = new RelayCommand<object>((obj) =>
            {
                if (obj is ComboBoxItem)
                {
                    var content = ((ComboBoxItem)obj).Content;
                    if (content == null) return;
                    var searchDomain = content.ToString();
                    if (string.IsNullOrEmpty(searchDomain)) return;
                    PortalService.CurrentPortalService.OrganizationResultsOnly = searchDomain == (string) Application.Current.Resources["SearchOrganization"];
                    if (SearchResults != null && SearchResults.IsEmpty) 
                        SearchResults= null;
                    // refresh search results            
                    UpdateResults();
                }
            });
        }        

        private async Task<IEnumerable<object>> SearchQueryAsync(uint count)
        {
            SearchParameters sp = SearchService.CreateSearchParameters(SearchQuery, _portalQuery, SearchResults.Count + 1, Math.Max((int)count, 100));           
            var result = await GetPortalItemsAsync(sp);
            RaisePropertyChanged(() => SearchResultsTerm);
            RaisePropertyChanged(() => SearchResultsNumber);
            return result;
        }

        private async Task<IEnumerable<object>> GetPortalItemsAsync(SearchParameters sp)
        {
            NoResults = false;

            // since GetPortalItemsAsync is called subsequently by the incremental loading collection,
            // always make sure the search text is valid. 
            if (!IsSearchQueryValid(SearchQuery))
            {
                NoResults = true;
                _totalHits = 0;
                return null;
            }

            var ps = PortalService.CurrentPortalService;
            if (PortalService.CurrentPortalService == null)
                return null;

            IsLoadingData = true;
            SearchResultInfo<ArcGISPortalItem> r = await ps.GetSearchResults(sp);
            IsLoadingData = false;

            // set NoResults each time to avoid false intermediate setting of
            // this depedency property
            if (r == null || r.TotalCount == 0 )
            {
                NoResults = true;
                _totalHits = 0;
                return null;
            }
            
            _totalHits = r.TotalCount;           
            return r.Results;
        }

        private void UpdateResults()
        {
            if (!IsSearchQueryValid(SearchQuery))
            {
                if (SearchResults != null) 
                    SearchResults.Clear();
                NoResults = true;
                _totalHits = 0;
                return;
            }

            if (SearchResults != null)
            {
                _totalHits = 0;
                RaisePropertyChanged("SearchResultsNumber");
                // clear the search results and reload the incremental collection
                SearchResults.Reset();
            }
            else
            {
                IncremetalLoadingCollection.getMore getMoreItemsAsync = SearchQueryAsync;
                SearchResults = new IncremetalLoadingCollection(getMoreItemsAsync);
            }
        }

        private void SetQuery(PortalQuery pQuery)
        {
            _portalQuery = pQuery;
            RaisePortalQueryPropertyAndSendMessage();
        }  

        private void RaisePortalQueryPropertyAndSendMessage()
        {
            Messenger.Default.Send(new ChangePortalQueryMessage(_portalQuery));
        }

        private bool IsSearchQueryValid(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return false;

            var trimmmedSearchText = searchText.Trim();
            if (string.IsNullOrEmpty(trimmmedSearchText))
                return false;

            // flag some chars as invalid search queries
            var r = new Regex("[+-^*~!@%$#&()|`]$");
            if (r.IsMatch(trimmmedSearchText))
                return false;

            return true;
        }
    }
}
