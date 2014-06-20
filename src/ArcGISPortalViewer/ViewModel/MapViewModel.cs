using ArcGISPortalViewer.Controls;
using ArcGISPortalViewer.Helpers;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Tasks.Query;
using Esri.ArcGISRuntime.WebMap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;

namespace ArcGISPortalViewer.ViewModel
{
    public sealed class Location
    {
        internal Location(int index, LocatorFindResult result)
        {
            Index = index;
            Result = result;
        }
        public LocatorFindResult Result { get; private set; }
        public int Index { get; private set; }
        public string Title { get { return string.Format("{0}. {1}", Index, GetAttributes(Result)); } }

        private string GetAttributes(LocatorFindResult result)
        {
            if (result == null || !result.Feature.Attributes.Any())
                return "";

            var v = result.Feature.Attributes;
            // use the result "name" if the PlaceName attribute is empty.
            // Reason: when searching for exact addresses and street intersections the attribute fields might
            // be empty; in particular we don't want to show an empty PlaceName.
            var placeName = (string)v["PlaceName"];
            if (string.IsNullOrEmpty(placeName))
                return Result.Name;

            // use the Placename, Type, City, and Country
            var s = string.Format("{0}{1}{2}{3}",
                    placeName,
                    string.IsNullOrEmpty((string)v["Type"]) ? "" : ", " + ((string)v["Type"]),
                    string.IsNullOrEmpty((string)v["City"]) ? "" : ", " + ((string)v["City"]),
                    string.IsNullOrEmpty((string)v["Country"]) ? "" : ", " + ((string)v["Country"]));
            return s;
        }
    }

    public class MapViewModel : ViewModelBase
    {
        private const double SearchResultPinXOffset = 22 / 2;
        private const double SearchResultPinYOffset = 51 / 2;

        private CancellationTokenSource _searchCancellationTokenSource;
        private string _currentSearchString;
        private GraphicsLayer _searchResultLayer;

        #region Identify

        private static SimpleLineSymbol _polylineSelectionSymbol = new SimpleLineSymbol { Color = Colors.Cyan, Width = 3, Style = SimpleLineStyle.Solid };
        private static SimpleFillSymbol _polygonSelectionSymbol = new SimpleFillSymbol { Color = Colors.Transparent, Style = SimpleFillStyle.Solid, Outline = _polylineSelectionSymbol };
        private static SimpleMarkerSymbol _pointSelectionSymbol = new SimpleMarkerSymbol { Color = Colors.Transparent, Size = 40, Style = SimpleMarkerStyle.Square, Outline = _polylineSelectionSymbol };

        #endregion Identify

        private Envelope _currentExtent;
        public Envelope CurrentExtent
        {
            get { return _currentExtent; }
            set
            {
                if (_currentExtent != value)
                {
                    _currentExtent = value;
                    base.RaisePropertyChanged("CurrentExtent");
                }
            }
        }

        private ObservableCollection<Location> _locations;
        public ObservableCollection<Location> Locations
        {
            get { return _locations; }
            private set
            {
                if (value != null)
                {
                    _locations = value;
                    RaisePropertyChanged(() => Locations);
                }
            }
        }

        public MapViewController Controller { get; private set; }

        public MapViewModel()
        {
            // the map controller is used to avoid using/passing the map control as a property to the MapViewModel
            Controller = new MapViewController();
			Editor = new MeasureEditor();
            PortalItem = ArcGISPortalViewer.ViewModel.AppViewModel.CurrentAppViewModel.SelectedPortalItem;
            IsSidePaneOpen = false;
            LocationDisplay = new LocationDisplay();
            if (IsInDesignMode)
            {
                IsLoadingWebMap = false;
                IsSidePaneOpen = true;
                IsAppBarOpen = true;
            }

            OnBasemapPickedCommand = new RelayCommand<object>(OnBasemapPicked);
            OnQuerySubmittedCommand = new RelayCommand<object>(OnQuerySubmitted);
            OnCollapsibleTabOpenedCommand = new RelayCommand<object>(OnCollapsibleTabOpened);
            OnMapTappedCommand = new RelayCommand<object>(OnMapTapped);

            #region Identify

            OnPopupTappedCommand = new RelayCommand<object>(OnPopupTapped);
            OnSetViewCommand = new RelayCommand<object>(OnSetView);
            OnSelectedItemCommand = new RelayCommand<object>(OnSelectedItem);
            OnBackClickCommand = new RelayCommand<object>(OnBackClick);

            #endregion Identify
        }

        /// <summary>
        /// The <see cref="PortalItem" /> property's name.
        /// </summary>
        public const string PortalItemPropertyName = "PortalItem";

        private ArcGISPortalItem _portalItem = null;

        /// <summary>
        /// Sets and gets the PortalItem property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ArcGISPortalItem PortalItem
        {
            get { return _portalItem; }
            set
            {
                RaisePropertyChanging(PortalItemPropertyName);
                _portalItem = value;
                IsLoadingWebMap = true;
                UpdateWebMap().ContinueWith(t =>
                {
                    IsLoadingWebMap = false;
                    if (!t.IsFaulted)
                        IsAppBarOpen = false;
                    SetupLayers();
                }, TaskScheduler.FromCurrentSynchronizationContext());
                RaisePropertyChanged(PortalItemPropertyName);
            }
        }

        private bool m_isLoadingWebMap;

        public bool IsLoadingWebMap
        {
            get { return m_isLoadingWebMap; }
            private set
            {
                m_isLoadingWebMap = value;
                base.RaisePropertyChanged("IsLoadingWebMap");
            }
        }

        private bool _isLoadingSearchResults = false;

        public bool IsLoadingSearchResults
        {
            get { return _isLoadingSearchResults; }
            private set
            {
                _isLoadingSearchResults = value;
                base.RaisePropertyChanged("IsLoadingSearchResults");
            }
        }

        private string _searchResultStatus = "";
        public string SearchResultStatus
        {
            get { return _searchResultStatus; }
            private set
            {
                _searchResultStatus = value;
                base.RaisePropertyChanged("SearchResultStatus");
            }
        }

        private Location _searchResultSelectedItem;
        public Location SearchResultSelectedItem
        {
            get { return _searchResultSelectedItem; }
            set
            {
                if (_searchResultSelectedItem != value)
                {
                    _searchResultSelectedItem = value;
                    // clear selected graphics
                    _searchResultLayer.ClearSelection();
                    if (_searchResultSelectedItem != null)
                    {
                        // get the related graphic and highlight it
                        Graphic searchItem = (from g in _searchResultLayer.Graphics
                                              where (int)g.Attributes["ID"] == _searchResultSelectedItem.Index
                                                    && g.Symbol is PictureMarkerSymbol
                                              select g).FirstOrDefault();
                        if (searchItem != null)
                        {
                            // select the new graphic                   
                            searchItem.IsSelected = true;
                            // set the view on the first item in the selection
                            var _ = SetViewAsync(_searchResultSelectedItem.Result.Extent);
                        }
                    }
                    base.RaisePropertyChanged("SearchResultSelectedItem");
                }
            }
        }

        private bool m_isSidePaneOpen;
        public bool IsSidePaneOpen
        {
            get { return m_isSidePaneOpen; }
            set
            {
                if (m_isSidePaneOpen != value)
                {
                    m_isSidePaneOpen = value;
                    base.RaisePropertyChanged("IsSidePaneOpen");
                }
            }
        }

        private bool m_isAppBarOpen = true;
        public bool IsAppBarOpen
        {
            get { return m_isAppBarOpen; }
            set
            {
                if (m_isAppBarOpen != value)
                {
                    m_isAppBarOpen = value;
                    base.RaisePropertyChanged("IsAppBarOpen");
                }
            }
        }

        /// <summary>
        /// Gets the location display used to display your location on the map
        /// </summary>
        public Esri.ArcGISRuntime.Location.LocationDisplay LocationDisplay { get; private set; }

		/// <summary>
		/// Gets the editor used to draw on the map
		/// </summary>
		public Esri.ArcGISRuntime.Controls.Editor Editor { get; private set; }

        private RelayCommand<object> _checkAutoPanMode;
        public ICommand CheckAutoPanMode
        {
            get
            {
                if (_checkAutoPanMode == null)
                    _checkAutoPanMode = new RelayCommand<object>(OnCheckAutoPanMode);
                return _checkAutoPanMode;
            }
        }

        private void OnCheckAutoPanMode(object commandParameter)
        {
            if (Controller == null) return;
            if (commandParameter is AutoPanModeChangedEventArgs)
            {
                var e = commandParameter as AutoPanModeChangedEventArgs;
                if (e.OldMode == AutoPanMode.CompassNavigation && e.OldMode != e.NewMode)
                {
                    Controller.ResetMapRotation();
                }
            }
        }

        /// <summary>
        /// The <see cref="WebMap" /> property's name.
        /// </summary>
        public const string WebMapPropertyName = "WebMap";

        private WebMap _webMap = null;

        /// <summary>
        /// Sets and gets the WebMap property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public WebMap WebMap
        {
            get
            {
                return _webMap;
            }

            set
            {
                if (_webMap == value)
                {
                    return;
                }

                RaisePropertyChanging(WebMapPropertyName);
                _webMap = value;
                RaisePropertyChanged(WebMapPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="WebMap" /> property's name.
        /// </summary>
        public const string WebMapVMPropertyName = "WebMapVM";

        private WebMapViewModel _webMapViewModel = null;

        /// <summary>
        /// Sets and gets the WebMap property.
        /// Changes to that property's value raise the PropertyChanged event.         
        /// </summary>
        public WebMapViewModel WebMapVM
        {
            get
            {
                return _webMapViewModel;
            }

            set
            {
                if (_webMapViewModel == value)
                {
                    return;
                }

                RaisePropertyChanging(WebMapVMPropertyName);
                _webMapViewModel = value;
                RaisePropertyChanged(WebMapVMPropertyName);
                RaisePropertyChanged("OperationalLayers");
                RaisePropertyChanged("HasOperationalLayers");
                RaisePropertyChanged("Bookmarks");
                RaisePropertyChanged("HasBookmarks");
            }
        }

        /// <summary>
        /// Gets the set of layers from the map that are operational layers in the webmap.
        /// Used by legend and layer list, so they only show for these and exclude
        /// basemap layers and temporary layers (like search results)
        /// </summary>
        public Esri.ArcGISRuntime.Layers.LayerCollection OperationalLayers
        {
            get
            {
                var coll = new Esri.ArcGISRuntime.Layers.LayerCollection();
                foreach (var layer in GetOperationalLayers())
                    coll.Add(layer);
                return coll;
            }
        }

        private IEnumerable<Esri.ArcGISRuntime.Layers.Layer> GetOperationalLayers()
        {
            if (WebMapVM != null)
            {
                foreach (var layer in WebMapVM.OperationalLayers)
                {
                    var id = !string.IsNullOrEmpty(layer.Id) ? layer.Id : layer.ItemId;
                    var opLayer = WebMapVM.Map.Layers.FirstOrDefault(l => l.ID == id);
                    if (opLayer != null)
                        yield return opLayer;
                }
            }
        }

        /// <summary>
        /// Get a value indication whether this webmap has any operational layers.
        /// If so, legend, layer list and basemap switcher displays.
        /// </summary>
        public bool HasOperationalLayers
        {
            get
            {
                if (base.IsInDesignMode)
                    return true;
                return GetOperationalLayers().Any();
            }
        }

        private IEnumerable<Bookmark> GetBookmarks()
        {
            if (WebMapVM != null && WebMapVM.WebMap != null && WebMapVM.WebMap.Bookmarks != null)
            {
                foreach (var bookmark in WebMapVM.WebMap.Bookmarks)
                {
                    yield return bookmark;
                }
            }
        }

        public IEnumerable<Bookmark> Bookmarks
        {
            get
            {
                if (_bookmarks == null && HasBookmarks)
                    _bookmarks = GetBookmarks();
                return _bookmarks;
            }
        }
        private IEnumerable<Bookmark> _bookmarks;

        public bool HasBookmarks
        {
            get
            {
                if (base.IsInDesignMode)
                    return true;
                return GetBookmarks().Any();
            }
        }

        private async Task<WebMap> UpdateWebMap()
        {
            try
            {
                WebMap = await WebMap.FromPortalItemAsync(PortalItem);
                if (WebMap == null)
                    return null;

                var currentPortal = this.PortalItem.ArcGISPortal;
                if (currentPortal.ArcGISPortalInfo.BingKey == null)
                    WebMapVM = await WebMapViewModel.LoadAsync(WebMap, currentPortal);
                else
                    WebMapVM = await WebMapViewModel.LoadAsync(WebMap, currentPortal, currentPortal.ArcGISPortalInfo.BingKey);

                var errors = WebMapVM.LoadErrors.ToArray();
                if (errors != null && errors.Any())
                {
                    string title = null;
                    string message = null;
                    if (errors.Count() == 1)
                    {
                        WebMapLayer webMapLayer = errors.First().Key;
                        var layerName = webMapLayer.Title ?? webMapLayer.Id ?? webMapLayer.Type;
                        title = string.Format("Unable to add the layer '{0}' in the map.", layerName);
                        message = errors.First().Value.Message;
                    }
                    else
                    {
                        title = string.Format("Unable to add {0} layers in the map.", errors.Count());
                        foreach (KeyValuePair<WebMapLayer, Exception> error in errors)
                        {
                            WebMapLayer webMapLayer = error.Key;
                            var layerName = webMapLayer.Title ?? webMapLayer.Id ?? webMapLayer.Type;
                            message += layerName + ":  " + error.Value.Message + Environment.NewLine;
                        }
                    }
                    var ex = new Exception(message);
                    var _ = App.ShowExceptionDialog(ex, title);
                }
                
                // <start workaround>
                // This is work around for WebMapViewModel because OutFields is not being set 
                // on the GeodatabaseFeatureServiceTable in the API this will be fixed after Beta.
                foreach(var featureLayer in OperationalLayers.OfType<FeatureLayer>())
                {
                    var webMapLayer = WebMap.OperationalLayers.FirstOrDefault(wml => wml.Id == featureLayer.ID);
                    if(webMapLayer != null && webMapLayer.PopupInfo != null && webMapLayer.PopupInfo.FieldInfos != null && webMapLayer.PopupInfo.FieldInfos.Any())
                    {                                         
                        var geodatabaseFeatureServiceTable = featureLayer.FeatureTable as GeodatabaseFeatureServiceTable;
                        if(geodatabaseFeatureServiceTable != null && geodatabaseFeatureServiceTable.OutFields == null)
                        {
                            geodatabaseFeatureServiceTable.OutFields = new OutFields(webMapLayer.PopupInfo.FieldInfos.Where(f => f != null).Select(f => f.FieldName));
                            geodatabaseFeatureServiceTable.RefreshFeatures(false);
                        }
                    }
                }
                // <end workaround>
            }
            catch (Exception ex)
            {
                var _ = App.ShowExceptionDialog(ex, "An exception was caught while trying to open the map.");
            }

            return WebMap;
        }

        /// <summary>
        /// Takes a webmap portal item as parameter and replaces the basemap of the loaded
        /// webmap with the basemap from the picked portal item. Used by the basemap picker
        /// </summary>
        public ICommand OnBasemapPickedCommand { get; private set; }

        private async void OnBasemapPicked(object parameter)
        {
            if (parameter is ArcGISPortalViewer.Controls.BasemapPicker.ArcGISPortalItemClickEventArgs && WebMapVM != null)
            {
                WebMap baseWebmap = null;
                try
                {
                    var e = (ArcGISPortalViewer.Controls.BasemapPicker.ArcGISPortalItemClickEventArgs)parameter;
                    baseWebmap = await WebMap.FromPortalItemAsync(e.ArcGISPortalItem);
                }
                catch { }
                if (baseWebmap != null)
                    WebMapVM.BaseMap = baseWebmap.BaseMap;
            }
        }

        /// <summary>
        /// Called after the webmap viewmodel has loaded
        /// </summary>
        private void SetupLayers()
        {
            CreateSearchResultLayer();
            CreateMeasureResultsLayer();
            CreateIdentifyResultsLayer();
        }

        /// <summary>
        /// Creates the layer used for displaying search results
        /// </summary>
        private void CreateSearchResultLayer()
        {
            AddGraphicsLayer(_searchResultLayer = new GraphicsLayer() { ID = "SearchLayer" });
        }

        /// <summary>
        /// Gets the on map tapped command. Used for binding a command to detect maclick on search result items.
        /// </summary>
        public ICommand OnMapTappedCommand { get; private set; }

        private async void OnMapTapped(object obj)
        {
            if (obj == null || IsMeasureOpened)
                return;

            if (obj is MapViewInputEventArgs)
            {
                var e = (MapViewInputEventArgs)obj;
                var graphics = await Controller.GraphicsLayerHitTestAsync(_searchResultLayer, e.Position, 10);
                // get the first grapghic hit whose symbol is of type PictureMarkerSymbol
                var hit = (from g in graphics where g.Symbol is PictureMarkerSymbol select g).FirstOrDefault();
                if (hit != null)
                {
                    var id = (int)hit.Attributes["ID"];
                    if (id - 1 < Locations.Count)
                        SearchResultSelectedItem = Locations[id - 1];

                }
                else // Identify
                {
                    try
                    {
                        await Identify(e.Position, e.Location, OperationalLayers);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(ex.Message);
#endif                        
                    }
                }
            }
        }

        private double _collapsibleTabWidth;
        /// <summary>
        /// Gets the on collapsible tab opened command. Used for binding a command to detect CollapsibleTab properties. 
        /// </summary>
        public ICommand OnCollapsibleTabOpenedCommand { get; private set; }

        private void OnCollapsibleTabOpened(object obj)
        {
            if (obj is double)
                _collapsibleTabWidth = (double)obj;
        }


        /// <summary>
        /// Sets the view on the extent of passed geometry while considering if side panel is opened.
        /// </summary>
        /// <param name="boundingGeometry"></param>
        private Task SetViewAsync(Esri.ArcGISRuntime.Geometry.Geometry boundingGeometry, double margin = 20)
        {
            if (Controller == null || boundingGeometry == null)
                return Task.FromResult(false);
            var extent = boundingGeometry.Extent;
            // set the padding around the extent - if the side pane is open add its width to the left margin
            Thickness padding = new Thickness((IsSidePaneOpen ? _collapsibleTabWidth : 0) + margin, margin, margin, margin);
            return Controller.SetViewAsync(extent, padding);
        }

        public Task FlyToAsync(MapPoint to, double scale, double margin = 20)
        {
            if (Controller == null || to == null)
                return Task.FromResult(false);

            // set the padding around the extent - if the side pane is open add its width to the left margin
            var padding = new Thickness((IsSidePaneOpen ? _collapsibleTabWidth : 0) + margin, margin, margin, margin);
            return Controller.FlyToAsync(to, scale, padding);
        }

        public Task FlyToAsync(Geometry flyTo, double margin = 20)
        {
            if (Controller == null || flyTo == null)
                return Task.FromResult(false);
            var extent = flyTo.Extent;
            // set the padding around the extent - if the side pane is open add its width to the left margin
            var padding = new Thickness((IsSidePaneOpen ? _collapsibleTabWidth : 0) + margin, margin, margin, margin);
            return Controller.FlyToAsync(extent, padding);
        }

        private PopupInfo GetPopupInfo(Layer layer, int subLayerId = -1)
        {
            if (WebMap == null || WebMap.OperationalLayers == null)
                return null;

            var webMapLayer = WebMap.OperationalLayers.FirstOrDefault(wml => wml.Id == layer.ID);
            if (webMapLayer != null)
            {
                if (subLayerId == -1)
                {
                    if (webMapLayer.FeatureCollection != null && webMapLayer.FeatureCollection.SubLayers != null)
                    {
                        var webMapSubLayer = webMapLayer.FeatureCollection.SubLayers.FirstOrDefault();
                        return webMapSubLayer != null ? webMapSubLayer.PopupInfo : null;
                    }
                    return webMapLayer.PopupInfo;
                }
                if (webMapLayer.SubLayers != null)
                {
                    var webMapSubLayer = webMapLayer.SubLayers.FirstOrDefault(l => l.Id == subLayerId);
                    return webMapSubLayer != null ? webMapSubLayer.PopupInfo : null;
                }
            }
            return null;
        }

        private void AddGraphicsLayer(GraphicsLayer layer)
        {
            if (WebMapVM != null && layer != null)
                WebMapVM.LayersAboveReferenceLayers.Add(layer);
        }

        private void RemoveGraphicsLayer(GraphicsLayer layer)
        {
            if (WebMapVM != null && layer != null)
                WebMapVM.LayersAboveReferenceLayers.Remove(layer);
        }

        private RelayCommand<object> m_ClearGraphics;
        /// <summary>
        ///  Gets the command that clears graphics from search, measure, and identify.
        ///  </summary>
        public ICommand ClearGraphics
        {
            get
            {
                if (m_ClearGraphics == null)
                    m_ClearGraphics = new RelayCommand<object>(OnClearGraphics);
                return m_ClearGraphics;
            }
        }

        private void OnClearGraphics(object commandParameter)
        {
            if (_searchResultLayer != null)
                _searchResultLayer.Graphics.Clear();
            if (m_MeasureLayer != null)
                m_MeasureLayer.Graphics.Clear();
            if (Controller != null && Editor != null && Editor.Cancel.CanExecute(null))
                Editor.Cancel.Execute(null);
            if (Locations != null)
                Locations.Clear();
            SearchResultStatus = null;
            ResetIdentify();
            measureHasItems = false;
            base.RaisePropertyChanged("IsClearGraphicsVisible");
        }

        public bool IsClearGraphicsVisible
        {
            get
            {
                return measureHasItems || HasIdentifyItems ||
                    (Locations != null && Locations.Any()) ||
                    (_searchResultLayer != null && _searchResultLayer.Graphics != null && _searchResultLayer.Graphics.Count > 0) ||
                    (m_MeasureLayer != null && m_MeasureLayer.Graphics != null && m_MeasureLayer.Graphics.Count > 0);
            }
        }

        /// <summary>
        /// Takes a string and passes it as a search query to perform the search. 
        /// </summary>
        public ICommand OnQuerySubmittedCommand { get; private set; }

        private async void OnQuerySubmitted(object obj)
        {
            if (obj == null || !(obj is SearchBoxQuerySubmittedEventArgs))
                return;

            string queryText = ((SearchBoxQuerySubmittedEventArgs)obj).QueryText;

            if (string.IsNullOrWhiteSpace(queryText))
                return;
            try
            {
                await Query(queryText);
            }
            catch (TaskCanceledException)
            {
                // Indicates that the task was successfully canceled - in this case we do nothing.               
            }
            catch (Exception ex)
            {
                SearchResultStatus = ex.Message;
            }
        }

        public async Task Query(string text)
        {
            try
            {
                if (Controller == null)
                    return;

                IsLoadingSearchResults = true;
                text = text.Trim();

                if (_searchCancellationTokenSource != null)
                {
                    if (_currentSearchString != null && _currentSearchString == text)
                        return;
                    _searchCancellationTokenSource.Cancel();
                }
                _searchCancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _searchCancellationTokenSource.Token;
                Envelope boundingBox = Controller.Extent;
                if (string.IsNullOrWhiteSpace(text)) return;
                if (_currentSearchString != null && _currentSearchString != text)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        _searchCancellationTokenSource.Cancel();
                }
                _searchResultLayer.Graphics.Clear();
                var geo = new OnlineLocatorTask(new Uri("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer", UriKind.Absolute), "");

                boundingBox = boundingBox.Expand(1.2);
               
                _currentSearchString = text;
                SearchResultStatus = string.Format("Searching for '{0}'...", text.Trim());
                var result = await geo.FindAsync(new OnlineLocatorFindParameters(text)
                {
                    MaxLocations = 25,                    
                    OutSpatialReference = WebMapVM.SpatialReference,
                    SearchExtent = boundingBox,
                    Location = (MapPoint)GeometryEngine.NormalizeCentralMeridianOfGeometry(boundingBox.GetCenter()),
                    Distance = GetDistance(boundingBox),
                    OutFields = new List<string>() { "PlaceName", "Type", "City", "Country" }                    
                }, cancellationToken);

                // if no results, try again with larger and larger extent
                var retries = 3;
                while (result.Count == 0 && --retries > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                    boundingBox = boundingBox.Expand(2);
                    result = await geo.FindAsync(new OnlineLocatorFindParameters(text)
                    {
                        MaxLocations = 25,
                        OutSpatialReference = WebMapVM.SpatialReference,
                        SearchExtent = boundingBox,
                        Location = (MapPoint)GeometryEngine.NormalizeCentralMeridianOfGeometry(boundingBox.GetCenter()),
                        Distance = GetDistance(boundingBox),
                        OutFields = new List<string>() { "PlaceName", "Type", "City", "Country"}
                    }, cancellationToken);
                }
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (result.Count == 0) 
                {
                    // atfer trying to expand the bounding box several times and finding no results, 
                    // let us try finding results without the spatial bound.
                    result = await geo.FindAsync(new OnlineLocatorFindParameters(text)
                    {
                        MaxLocations = 25,
                        OutSpatialReference = WebMapVM.SpatialReference,
                        OutFields = new List<string>() { "PlaceName", "Type", "City", "Country"}
                    }, cancellationToken);

                    if (result.Any())
                    {
                        // since the results are not bound by any spatial filter, let us show well known administrative 
                        // places e.g. countries and cities, and filter out other results e.g. restaurents and business names.
                        var typesToInclude = new List<string>()
                        { "", "city", "community", "continent", "country", "county", "district", "locality", "municipality", "national capital", 
                          "neighborhood", "other populated place", "state capital", "state or province", "territory", "village"};
                        for (var i = result.Count - 1; i >= 0; --i)
                        {
                            // get the result type
                            var resultType = ((string)result[i].Feature.Attributes["Type"]).Trim().ToLower();
                            // if the result type exists in the inclusion list above, keep it in the list of results
                            if (typesToInclude.Contains(resultType))
                                continue;
                            // otherwise, remove it from the list of results
                            result.RemoveAt(i);
                        }
                    }
                }

                if (result.Count == 0)
                {
                    SearchResultStatus = string.Format("No results for '{0}' found", text);
                    if (Locations != null)
                        Locations.Clear();
                    //await new Windows.UI.Popups.MessageDialog(string.Format("No results for '{0}' found", text)).ShowAsync();
                }
                else
                {
                    SearchResultStatus = string.Format("Found {0} results for '{1}'", result.Count.ToString(), text);
                    Envelope extent = null;
                    var color = (App.Current.Resources["AppAccentBrush"] as SolidColorBrush).Color;
                    var color2 = (App.Current.Resources["AppAccentForegroundBrush"] as SolidColorBrush).Color;
                    SimpleMarkerSymbol symbol = new SimpleMarkerSymbol()
                    {
                        Color = Colors.Black,
                        Outline = new SimpleLineSymbol() { Color = Colors.Black, Width = 2 },
                        Size = 16,
                        Style = SimpleMarkerStyle.Square
                    };

                    // set the picture marker symbol used in the search result composite symbol.
                    var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Icons/SearchResult.png"));
                    var imageSource = await imageFile.OpenReadAsync();
                    var pictureMarkerSymbol = new PictureMarkerSymbol();
                    await pictureMarkerSymbol.SetSourceAsync(imageSource);
                    // apply an x and y offsets so that the tip of of the pin points to the correct location.
                    pictureMarkerSymbol.XOffset = SearchResultPinXOffset;
                    pictureMarkerSymbol.YOffset = SearchResultPinYOffset;

                    int ID = 1;
                    foreach (var r in result)
                    {
                        if (extent == null)
                            extent = r.Extent;
                        else if (r.Extent != null)
                            extent = extent.Union(r.Extent);

                        var textSymbol = new TextSymbol()
                        {
                            Text = ID.ToString(),
                            Font = new SymbolFont() { FontFamily = "Verdena", FontSize = 10, FontWeight = SymbolFontWeight.Bold },
                            Color = color2,
                            BorderLineColor = color2,
                            HorizontalTextAlignment = Esri.ArcGISRuntime.Symbology.HorizontalTextAlignment.Center,
                            VerticalTextAlignment = Esri.ArcGISRuntime.Symbology.VerticalTextAlignment.Bottom,
                            XOffset = SearchResultPinXOffset,
                            YOffset = SearchResultPinYOffset
                        }; //Looks like Top and Bottom are switched - potential CR                      

                        // a compsite symbol for both the PictureMarkerSymbol and the TextSymbol could be used, but we 
                        // wanted to higlight the pin without the text; therefore, we will add them separately.

                        // add the PictureMarkerSymbol to _searchResultLayer
                        Graphic pin = new Graphic()
                        {
                            Geometry = r.Extent.GetCenter(),
                            Symbol = pictureMarkerSymbol,
                        };
                        pin.Attributes["ID"] = ID;
                        pin.Attributes["Name"] = r.Name;
                        _searchResultLayer.Graphics.Add(pin);

                        // add the text to _searchResultLayer
                        Graphic pinText = new Graphic()
                        {
                            Geometry = r.Extent.GetCenter(),
                            Symbol = textSymbol,
                        };
                        pinText.Attributes["ID"] = ID;
                        pinText.Attributes["Name"] = r.Name;
                        _searchResultLayer.Graphics.Add(pinText);

                        ID++;
                    }

                    SetResult(result);
                    base.RaisePropertyChanged("IsClearGraphicsVisible");
                    if (extent != null)
                    {
                        var _ = SetViewAsync(extent, 50);
                    }
                }
            }
            finally
            {
                IsLoadingSearchResults = false;
                _searchCancellationTokenSource = null;
                _currentSearchString = null;
            }
        }

        private static double GetDistance(Envelope extent)
        {
            // get the distance between the center of the current map extent and one of its corners
            if (extent != null && !extent.IsEmpty)
            {
                var d = GeometryEngine.GeodesicLength(new Polyline(new Coordinate[] { extent.GetCenter().Coordinate, new Coordinate(extent.XMin, extent.YMin) },
                        extent.SpatialReference), GeodeticCurveType.GreatElliptic);

                // to increase the chances of finding results make sure the smallest returned distance is 5 Kilometers.
                return (d <= 5000.0) ? 5000.0 : d;
            }
           
            return 5000.0; // return a default distance of 5 Kilometers
        }

        private void SetResult(IEnumerable<LocatorFindResult> locations)
        {
            if (Locations == null)
                Locations = new ObservableCollection<Location>();
            else
                Locations.Clear();
            int i = 0;
            foreach (var location in locations)
            {
                Locations.Add(new Location(++i, location));
            }
        }

        #region Measure Tool

        private bool m_IsMeasureOpened;
        /// <summary>
        ///  Gets or sets a value indicating whether measure tool is the current item opened.
        ///  </summary>
        public bool IsMeasureOpened
        {
            get { return m_IsMeasureOpened; }
            set
            {
                if (m_IsMeasureOpened != value)
                {
                    m_IsMeasureOpened = value;
                    base.RaisePropertyChanged("IsMeasureOpened");
                }
            }
        }

        private RelayCommand<object> m_UpdateMeasureArea;
        /// <summary>
        ///  Gets the command that displays the measure items on the map.
        ///  </summary>
        public ICommand UpdateMeasureArea
        {
            get
            {
                if (m_UpdateMeasureArea == null)
                    m_UpdateMeasureArea = new RelayCommand<object>(OnUpdateMeasureArea);
                return m_UpdateMeasureArea;
            }
        }
        private bool measureHasItems = false;
        private void OnUpdateMeasureArea(object commandParameter)
        {
            if (!(commandParameter is EventArgs) || m_MeasureLayer == null)
                return;
            m_MeasureLayer.Graphics.Clear();
            measureHasItems = false;
            if (commandParameter is MeasureUpdatedEventArgs)
            {
                measureHasItems = true;
                var e = commandParameter as MeasureUpdatedEventArgs;
                if (e.Area is Polygon)
                    m_MeasureLayer.Graphics.Add(new Graphic() { Geometry = e.Area });
            }
            base.RaisePropertyChanged("IsClearGraphicsVisible");
        }

        private GraphicsLayer m_MeasureLayer;

        /// <summary>
        /// Adds graphics layer to hold measure results.
        /// </summary>
        private void CreateMeasureResultsLayer()
        {
            if (m_MeasureLayer == null)
            {
                m_MeasureLayer = new GraphicsLayer()
                {
                    ID = "MeasureLayer",
                    Renderer = new SimpleRenderer()
                    {
                        Symbol = new SimpleFillSymbol()
                        {
                            Color = Color.FromArgb(50, 255, 255, 255),
                            Outline = new SimpleLineSymbol() { Style = SimpleLineStyle.Dot, Width = 1 }
                        }
                    }
                };
            }
            AddGraphicsLayer(m_MeasureLayer);
        }

        #endregion Measure Tool


        #region Identify

        #region Public Properties

        /// <summary>
        /// Gets the on map tapped command. Used for binding a command to detect when user taps on the popup.
        /// </summary>
        public ICommand OnPopupTappedCommand { get; private set; }

        /// <summary>
        /// Command used to set the view on the selected item.
        /// </summary>
        public ICommand OnSetViewCommand { get; private set; }

        /// <summary>
        /// Command used when item in "view model" is considered selected.
        /// </summary>
        public ICommand OnSelectedItemCommand { get; private set; }

        /// <summary>
        /// Command used when going back to list of identified results.
        /// </summary>
        public ICommand OnBackClickCommand { get; private set; }

        public int Count
        {
            get
            {
                return IdentifyItems != null ? IdentifyItems.Count() : 0;
            }
        }

        private bool HasIdentifyItems
        {
            get { return IdentifyItems != null && IdentifyItems.Any(); }
        }

        public string FeaturesFoundText
        {
            get { return string.Format("{0} {1}", Count, Count == 1 ? "feature found" : "features found"); }
        }

        public IEnumerable<PopupItem> IdentifyItems
        {
            get { return _identifyItems; }
            set
            {
                if (!Equals(IdentifyItems, value))
                {
                    _identifyItems = value;
                    base.RaisePropertyChanged("IdentifyItems");
                    base.RaisePropertyChanged("Count");
                    base.RaisePropertyChanged("FeaturesFoundText");
                    base.RaisePropertyChanged("IsOverlayVisible");
                }
            }
        }
        private IEnumerable<PopupItem> _identifyItems;

        public MapPoint AnchorMapPoint
        {
            get { return _anchorMapPoint; }
            set
            {
                if (_anchorMapPoint != value)
                {
                    _anchorMapPoint = value;
                    base.RaisePropertyChanged("AnchorMapPoint");
                }
            }
        }
        private MapPoint _anchorMapPoint;

        public bool IsIdentifyPanelEnabled
        {
            get { return _isIdentifyPanelEnabled; }
            set
            {
                if (_isIdentifyPanelEnabled != value)
                {
                    _isIdentifyPanelEnabled = value;
                    base.RaisePropertyChanged("IsIdentifyPanelEnabled");
                }
            }
        }
        private bool _isIdentifyPanelEnabled;

        public bool IsOverlayVisible
        {
            get { return (IsIdentifying || HasIdentifyItems); }
        }

        public bool OpenIdentifyPanel
        {
            get { return _openIdentifyPanel; }
            set
            {
                if (_openIdentifyPanel != value)
                {
                    _openIdentifyPanel = value;
                    base.RaisePropertyChanged("OpenIdentifyPanel");
                }
            }
        }
        private bool _openIdentifyPanel;

        public bool IsIdentifying
        {
            get { return _isIdentifying; }
            private set
            {
                if (_isIdentifying != value)
                {
                    _isIdentifying = value;
                    base.RaisePropertyChanged("IsIdentifying");
                    base.RaisePropertyChanged("IsOverlayVisible");
                    base.RaisePropertyChanged("IsClearGraphicsVisible");
                }
            }
        }
        private bool _isIdentifying;

        public GraphicsLayer IdentifyLayer
        {
            get { return _identifyLayer ?? (_identifyLayer = new GraphicsLayer { ID = "IdentifyLayer" }); }
        }

        private GraphicsLayer _identifyLayer;

        public PopupItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value && ShowDetailView)
                {
                    _selectedItem = value;
                    base.RaisePropertyChanged("SelectedItem");
                    HighlightSelection();
                }
            }
        }
        private PopupItem _selectedItem;


        public bool ShowDetailView
        {
            get { return _showDetailView; }
            set
            {
                if (_showDetailView != value)
                {
                    _showDetailView = value;
                    base.RaisePropertyChanged("ShowDetailView");
                }
            }
        }
        private bool _showDetailView;



        #endregion Public Properties

        #region Public Methods

        public async Task Identify(Point position, MapPoint location, LayerCollection layerCollection)
        {
            try
            {
                IsIdentifying = true;
                AnchorMapPoint = location;
                IdentifyItems = ParseIdentifyResults(await IdentifyHelper.Identify(Controller, position, location, layerCollection));
                SelectedItem = null;
                ShowDetailView = false;
                if (Count == 1)
                {
                    ShowDetailView = true;
                    SelectedItem = IdentifyItems.First();
                }
            }            
            finally
            {
                IsIdentifying = false;
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Adds graphics layer to hold identify results.
        /// </summary>
        private void CreateIdentifyResultsLayer()
        {
            AddGraphicsLayer(IdentifyLayer);
        }

        private void OnPopupTapped(object obj)
        {
            ((TappedRoutedEventArgs)obj).Handled = IsIdentifyPanelEnabled = OpenIdentifyPanel = IsSidePaneOpen = true;
        }

        private void ResetIdentify()
        {
            SelectedItem = null;
            if (OpenIdentifyPanel)
                IsSidePaneOpen = false;
            IsIdentifyPanelEnabled = false;
            OpenIdentifyPanel = false;
            IdentifyItems = null;
        }

        private void OnSetView(object obj)
        {
            if (obj is PopupItem)
            {
                var popupItem = ((PopupItem)obj);
                OnSelectedItem(popupItem);
                var _ = SetViewAsync(popupItem.IdentifyFeature.Item.Feature.Geometry.Extent);
            }
            else if (obj is Esri.ArcGISRuntime.Geometry.Geometry)
            {
                var _ = SetViewAsync((Esri.ArcGISRuntime.Geometry.Geometry)obj);
            }
            if (obj is Bookmark)
            {
                var _ = FlyToAsync(((Bookmark)obj).Extent);
            }
        }

        private void OnSelectedItem(object obj)
        {
            ShowDetailView = true;
            SelectedItem = ((PopupItem)obj);
        }

        private void OnBackClick(object obj)
        {
            SelectedItem = null;
            ShowDetailView = false;
        }

        public  IEnumerable<PopupItem> ParseIdentifyResults(IDictionary<Layer, IEnumerable<IdentifyFeature>> identifyResults)
        {
            if (identifyResults == null)
                return null;

            IList<PopupItem> popupItems = new List<PopupItem>();
            foreach (var layer in identifyResults.Keys)
            {
                var identifyFeatures = identifyResults[layer];
                if (identifyFeatures == null)
                    continue;
                foreach (var identifyFeature in identifyFeatures)
                {
                    var popupInfo = GetPopupInfo(layer, identifyFeature.Item.LayerID);
                    if (popupInfo != null)
                        popupItems.Add(new PopupItem(identifyFeature, popupInfo));
                }
            }
            return popupItems;
        }

        private void HighlightSelection()
        {
            IdentifyLayer.Graphics.Clear();
            if (SelectedItem == null)
                return;
            var feature = SelectedItem.IdentifyFeature.Item.Feature;
            var selectionGraphic = CreateSelectionGraphic(feature);
            IdentifyLayer.Graphics.Add(selectionGraphic);
        }

        #endregion Private Methods

        #endregion Identify

        private Graphic CreateSelectionGraphic(Feature feature)
        {
            if (feature == null)
                return null;

            var selectionGraphic  = feature is GeodatabaseFeature ? ((GeodatabaseFeature)feature).AsGraphic() : new Graphic { Geometry = feature.Geometry };
            switch (selectionGraphic.Geometry.GeometryType)
            {
                case GeometryType.MultiPoint:
                case GeometryType.Point:
                    selectionGraphic.Symbol = _pointSelectionSymbol;
                    break;
                case GeometryType.Polygon:
                case GeometryType.Envelope:
                    selectionGraphic.Symbol = _polygonSelectionSymbol;
                    break;
                case GeometryType.Polyline:
                    selectionGraphic.Symbol = _polylineSelectionSymbol;
                    break;
            }            
            return selectionGraphic;
        }
    }

    public class PopupItem
    {
        public PopupItem(IdentifyFeature identifyFeature, PopupInfo popupInfo)
        {
            IdentifyFeature = identifyFeature;
            PopupInfo = popupInfo;
        }

        public IdentifyFeature IdentifyFeature { get; set; }

        public PopupInfo PopupInfo { get; set; }
    }
}
