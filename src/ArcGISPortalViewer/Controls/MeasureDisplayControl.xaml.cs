// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using ArcGISPortalViewer.Common;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace ArcGISPortalViewer.Controls
{
    public sealed partial class MeasureDisplayControl : UserControl
    {
        public MeasureDisplayControl()
        {
            this.InitializeComponent();
            MeasureItems.ItemsSource = MeasureItemCollection;
            ResultSummary.DataContext = MeasureSummary;
        }

        #region Properties
        public Editor Editor
        {
            get { return (Editor) GetValue(EditorProperty); }
            set { SetValue(EditorProperty, value); }
        }

        public static readonly DependencyProperty EditorProperty =
            DependencyProperty.Register("Editor", typeof (Editor), typeof (MeasureDisplayControl),
                new PropertyMetadata(null));

        public LinearUnitType LinearUnitType
        {
            get { return (LinearUnitType) GetValue(LinearUnitTypeProperty); }
            set { SetValue(LinearUnitTypeProperty, value); }
        }

        public static readonly DependencyProperty LinearUnitTypeProperty =
            DependencyProperty.Register("LinearUnitType", typeof (LinearUnitType), typeof (MeasureDisplayControl),
                new PropertyMetadata(LinearUnitType.Metric, OnLinearUnitTypePropertyChanged));

        private static void OnLinearUnitTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var measureDisplayControl = d as MeasureDisplayControl;
            if (measureDisplayControl == null)
                return;
            var linearUnitType = (LinearUnitType)e.NewValue;
            foreach (var measureItem in measureDisplayControl.MeasureItemCollection)
                measureItem.LinearUnitType = linearUnitType;
            measureDisplayControl.MeasureSummary.LinearUnitType = linearUnitType;
        }

        public CoordinateFormat CoordinateFormat
        {
            get { return (CoordinateFormat) GetValue(CoordinateFormatProperty); }
            set { SetValue(CoordinateFormatProperty, value); }
        }

        public static readonly DependencyProperty CoordinateFormatProperty =
            DependencyProperty.Register("CoordinateFormat", typeof (CoordinateFormat), typeof (MeasureDisplayControl),
                new PropertyMetadata(CoordinateFormat.DecimalDegrees, OnCoordinateFormatPropertyChanged));

        private static void OnCoordinateFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var measureDisplayControl = d as MeasureDisplayControl;
            if (measureDisplayControl == null)
                return;
            var coordinateFormat = (CoordinateFormat) e.NewValue;
            foreach (var measureItem in measureDisplayControl.MeasureItemCollection)
                measureItem.CoordinateFormat = coordinateFormat;
        }

        public bool IsMeasureEnabled
        {
            get { return (bool) GetValue(IsMeasureEnabledProperty); }
            set { SetValue(IsMeasureEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMeasureEnabledProperty =
            DependencyProperty.Register("IsMeasureEnabled", typeof (bool), typeof (MeasureDisplayControl),
                new PropertyMetadata(false, OnIsMeasureEnabledPropertyChanged));

        private static void OnIsMeasureEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var measureDisplayControl = d as MeasureDisplayControl;
            if (measureDisplayControl == null || measureDisplayControl.Editor == null)
                return;
            measureDisplayControl.Editor.IsSuspended = !measureDisplayControl.IsMeasureEnabled;
            if(!measureDisplayControl.Editor.IsActive)
            {
                measureDisplayControl.ExecuteMeasure();
            }
        }
        

        private ObservableCollection<MeasureItem> m_MeasureItemCollection;
        /// <summary>
        /// Gets the collection of <see cref="MeasureItem"/>.
        /// </summary>
        public ObservableCollection<MeasureItem> MeasureItemCollection
        {
            get
            {
                return m_MeasureItemCollection ?? (m_MeasureItemCollection = new ObservableCollection<MeasureItem>());
            }
        }

        private MeasureSummary m_MeasureSummary;
        /// <summary>
        /// Gets the <see cref="MeasureSummary"/>.
        /// </summary>
        public MeasureSummary MeasureSummary
        {
            get { return m_MeasureSummary ?? (m_MeasureSummary = new MeasureSummary()); }
        }
        #endregion Properties
        
        private void MeasureItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            var measureItem = item.DataContext as MeasureItem;
            if (Editor != null && measureItem != null && Editor.DeleteVertex.CanExecute(measureItem.CoordinateIndex))
                FlyoutBase.ShowAttachedFlyout((FrameworkElement) sender);
        }

        private void DeleteVertex_Click(object sender, RoutedEventArgs e)
        {
            if (Editor == null || !(sender is FrameworkElement) ||
                !(((FrameworkElement)sender).DataContext is MeasureItem)) 
                return;
            int coordinateIndex = ((MeasureItem)((FrameworkElement)sender).DataContext).CoordinateIndex;
            if (Editor.DeleteVertex.CanExecute(coordinateIndex))
                Editor.DeleteVertex.Execute(coordinateIndex);
        }

        private void ResetMeasure_Click(object sender, RoutedEventArgs e)
        {
            ResetMeasure();
        }

        private void ResetMeasure()
        {
            ResetDisplay();
        }

        private void ResetDisplay()
        {
            MeasureItemCollection.Clear();
            MeasureSummary.TotalLength = 0;
            MeasureSummary.Area = 0;
            if (Editor != null && Editor.IsActive)
            {
                if (Editor.Cancel.CanExecute(null))
                    Editor.Cancel.Execute(null);
            }
        }

        private async void ExecuteMeasure()
        {
            if (Editor == null) return;
            ResetDisplay();
            OnMeasureStarted();
            Exception error = null;
            Polyline polyline = null;
            bool isCanceled = false;
            try
            {
                var result = await Editor.RequestShapeAsync(DrawShape.Polyline,
                    new SimpleLineSymbol()
                    {
                        Color = Colors.CornflowerBlue,
                        Width = 4
                    }, new Progress<GeometryEditStatus>(OnStatusReported));
                polyline = result as Polyline;
            }
            catch (TaskCanceledException)
            {
                isCanceled = true;
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                Polygon area = null;
                if (polyline != null && polyline.Paths[0].Count > 2)
                {
                  area = new Polygon(polyline.Paths, polyline.SpatialReference);
                }
                polyline = null;
                OnMeasureCompleted(area, error, isCanceled);
                if (IsEnabled)
                    ExecuteMeasure();
            }
        }

        /// <summary>
        /// Updates the m_Polyline and MeasureItemsCollection based on the progress 
        /// reported by <see cref="Esri.ArcGISRuntime.Controls.Editor"/>
        /// </summary>
        /// <param name="status"></param>
        private void OnStatusReported(GeometryEditStatus status)
        {
            var polyline = status.NewGeometry as Polyline;
            switch (status.GeometryEditAction)
            {
                case GeometryEditAction.AddedVertex:
                {
                    if (status.NewVertex != null)
                        MeasureItemCollection.Insert(status.VertexPosition.CoordinateIndex,
                            new MeasureItem()
                            {
                                Location = status.NewVertex,
                                LinearUnitType = LinearUnitType,
                                CoordinateFormat = CoordinateFormat
                            });
                    break;
                }
                case GeometryEditAction.DeletedVertex:
                {
                    MeasureItemCollection.RemoveAt(status.VertexPosition.CoordinateIndex);
                    break;
                }
                default:
                {
                    MeasureItemCollection.Clear();
                    if (polyline != null)
                    {
                        foreach (var p in polyline.Paths[0])
                        {
                            MeasureItemCollection.Add(new MeasureItem()
                            {
                                Location = new MapPoint(p, polyline.SpatialReference),
                                LinearUnitType = LinearUnitType,
                                CoordinateFormat = CoordinateFormat
                            });
                        }
                    }
                    break;
                }
            }
            UpdateDisplay(polyline);
        }

        /// <summary>
        /// Raises the <see cref="MeasureUpdated"/> and updates display based on <see cref="MeasureItemCollection"/>, 
        /// <see cref="TotalLength"/> and <see cref="Area"/> properties.
        /// </summary>
        private void UpdateDisplay(Polyline polyline)
        {
            Polygon area = null;
            if (polyline != null && polyline.Paths[0].Count > 2)
            {
                area = new Polygon(polyline.Paths, polyline.SpatialReference);
            }
            OnMeasureUpdated((Geometry)area ?? polyline);
            MapPoint previousPoint = null;
            int coordinateIndex = 0;
            foreach (var measureItem in MeasureItemCollection)
            {
                measureItem.CoordinateIndex = coordinateIndex++;
                if (previousPoint != null && measureItem.Location != null)
                {
                    measureItem.Length = GeometryEngine.GeodesicLength(
                        new Polyline(new Coordinate[] {previousPoint.Coordinate, measureItem.Location.Coordinate},
                            measureItem.Location.SpatialReference),
                        GeodeticCurveType.GreatElliptic);
                }
                previousPoint = measureItem.Location;
            }

            MeasureSummary.TotalLength = polyline == null || polyline.Paths[0].Count < 2
                ? 0
                : GeometryEngine.GeodesicLength(polyline);
            MeasureSummary.Area = area == null
                ? 0
                : GeometryEngine.GeodesicArea(area);
        }

        #region Events

        public event EventHandler MeasureStarted;
        public event EventHandler<MeasureUpdatedEventArgs> MeasureUpdated;
        public event EventHandler<MeasureCompletedEventArgs> MeasureCompleted;

        private void OnMeasureStarted()
        {
            if (MeasureStarted != null)
                MeasureStarted(this, EventArgs.Empty);
        }

        private void OnMeasureUpdated(Geometry geometry)
        {
            if (MeasureUpdated != null)
                MeasureUpdated(this, new MeasureUpdatedEventArgs(geometry));
        }

        private void OnMeasureCompleted(Geometry area = null, Exception error = null, bool isCanceled = false)
        {
            if (MeasureCompleted != null)
                MeasureCompleted(this, new MeasureCompletedEventArgs(area, error, isCanceled));
        }

        #endregion Events
    }

    #region EventArgs
    /// <summary>
    /// Data for <see cref="MeasureDisplayControl.MeasureUpdated"/> event.
    /// </summary>
    public sealed class MeasureUpdatedEventArgs : EventArgs
    { 
        /// <summary>
        /// Gets the current <see cref="Geometry"/> that represent the geodesic area covered during measure.
        /// </summary>
        public Geometry Area { get; private set; }

        public MeasureUpdatedEventArgs(Geometry area)
        {
            Area = area;
        }
    }


    /// <summary>
    /// Data for <see cref="MeasureDisplayControl.MeasureCompleted"/> event.
    /// </summary>
    public sealed class MeasureCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the resulting <see cref="Geometry"/> that represent the geodesic area covered during measure.
        /// </summary>
        public Geometry Area { get; private set; }

        /// <summary>
        /// Gets the <see cref="Exception"/> that caused measure to fail.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets a value indicating whether measure was canceled.
        /// </summary>
        public bool IsCanceled { get; private set; }

        internal MeasureCompletedEventArgs(Geometry area = null, Exception error = null, bool isCanceled = false)
        {
            Area = area;
            Error = error;
            IsCanceled = isCanceled;
        }
    }
    #endregion EventArgs

    public abstract class MeasureDisplay : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        private LinearUnitType m_LinearUnitType;
        /// <summary>
        /// Gets or sets <see cref="LinearUnitType"/>
        /// </summary>
        public LinearUnitType LinearUnitType
        {
            get { return m_LinearUnitType; }
            set
            {
                if (m_LinearUnitType != value)
                {
                    m_LinearUnitType = value;
                    OnPropertyChanged();
                    OnLinearUnitTypeChanged();
                }
            }
        }

        private CoordinateFormat m_CoordinateFormat;
        /// <summary>
        /// Gets or sets <see cref="CoordinateFormat"/>
        /// </summary>  
        public CoordinateFormat CoordinateFormat
        {
            get { return m_CoordinateFormat; }
            set
            {
                if (m_CoordinateFormat != value)
                {
                    m_CoordinateFormat = value;
                    OnPropertyChanged();
                    OnCoordinateFormatChanged();
                }
            }
        }

        protected string GeodesicAreaToString(double area, LinearUnitType linearUnitType)
        {
            area = Math.Abs(area);
            switch (linearUnitType)
            {
                case LinearUnitType.Metric:
                    {
                        if (area < 1000000)
                            return string.Format("{0:0} m²", area);
                        return string.Format("{0:0.##} km²", area / 1000000);
                    }
                case LinearUnitType.ImperialUS:
                    {
                        double squareMiles = area * 3.86102e-7;
                        if (squareMiles >= 1)
                            return string.Format("{0:0.###} mi²", squareMiles);
                        double squareFeet = area * 10.7639;
                        return string.Format("{0:0} ft²", squareFeet);
                    }
            }
            return null;
        }

        protected string GeodesicLengthToString(double length, LinearUnitType linearUnitType)
        {
            length = Math.Abs(length);
            switch (linearUnitType)
            {
                case LinearUnitType.Metric:
                    {
                        if (length < 10000)
                            return string.Format("{0:0} m", length);
                        return string.Format("{0:0.###} km", length / 1000);
                    }
                case LinearUnitType.ImperialUS:
                    {
                        double miles = length * 0.000621371;
                        if (miles > .25)
                            return string.Format("{0:0.###} mi", miles);
                        return string.Format("{0:0} ft", length * 3.28084);
                    }
            }
            return null;
        }

        protected string LocationToString(MapPoint location, CoordinateFormat coordinateFormat)
        {
            if (location == null)
                return null;

            switch (coordinateFormat)
            {
                case CoordinateFormat.DecimalDegrees:
                    return CoordinateConversion.MapPointToDecimalDegrees(location, 5);
                case CoordinateFormat.DegreesDecimalMinutes:
                    return CoordinateConversion.MapPointToDegreesDecimalMinutes(location, 3);
                case CoordinateFormat.Dms:
                    return CoordinateConversion.MapPointToDegreesMinutesSeconds(location, 1);
                case CoordinateFormat.Mgrs:
                    return CoordinateConversion.MapPointToMgrs(location, MgrsConversionMode.Automatic, 5, true, true);
            }
            return null;
        }

        protected virtual void OnLinearUnitTypeChanged()
        {

        }

        protected virtual void OnCoordinateFormatChanged()
        {

        }
    }

    public sealed class MeasureSummary : MeasureDisplay
    {
        private double m_Area;
        /// <summary>
        /// Gets or sets the geodesic area of geometry drawn by the <see cref="Esri.ArcGISRuntime.Controls.Editor"/>.
        /// </summary>
        public double Area
        {
            get { return m_Area; }
            set
            {
                if (!double.Equals(m_Area, value))
                {
                    m_Area = value;
                    OnPropertyChanged();
                    OnPropertyChanged("AreaDisplay");
                }
            }
        }

        private double m_TotalLength;
        /// <summary>
        /// Gets or sets the total geodesic length of geometry drawn by the <see cref="Esri.ArcGISRuntime.Controls.Editor"/>.
        /// </summary>
        public double TotalLength
        {
            get { return m_TotalLength; }
            set
            {
                if (!double.Equals(m_TotalLength, value))
                {
                    m_TotalLength = value;
                    OnPropertyChanged();
                    OnPropertyChanged("TotalLengthDisplay");
                }
            }
        }

        /// <summary>
        /// Gets the geodesic area in <see cref="MeasureDisplay.LinearUnitType"/>
        /// </summary>
        public string AreaDisplay
        {
            get{return base.GeodesicAreaToString(Area, base.LinearUnitType);}
        }

        /// <summary>
        /// Gets the total geodesic length in <see cref="MeasureDisplay.LinearUnitType"/>.
        /// </summary>
        public string TotalLengthDisplay
        {
            get { return base.GeodesicLengthToString(TotalLength, base.LinearUnitType); }
        }

        protected override void OnLinearUnitTypeChanged()
        {
            OnPropertyChanged("TotalLengthDisplay");
            OnPropertyChanged("AreaDisplay");
            base.OnLinearUnitTypeChanged();
        }
    }

    /// <summary>
    /// Represents the individual measure item in <see cref="MeasureDisplayControl"/>
    /// </summary>
    public sealed class MeasureItem : MeasureDisplay
    {
        private int m_CoordinateIndex;
        /// <summary>
        /// Gets or sets the actual position of the vertex in the coordinate collection.
        /// </summary>
        public int CoordinateIndex
        {
            get { return m_CoordinateIndex; }
            set
            {
                if (m_CoordinateIndex != value)
                {
                    m_CoordinateIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Index");
                }
            }
        }

        /// <summary>
        /// Gets the index of the measure item in the collection.
        /// </summary>
        public int Index
        {
            get { return CoordinateIndex + 1; }
        }

        private MapPoint m_Location;
        /// <summary>
        /// Gets or sets the location of vertex in map coordinate.
        /// </summary>
        public MapPoint Location
        {
            get { return m_Location; }
            set
            {
                if (m_Location != value)
                {
                    m_Location = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CoordinateDisplay");
                }
            }
        }

        private double m_Length;
        /// <summary>
        /// Gets or sets the Geodesic length.
        /// </summary>
        public double Length
        {
            get { return m_Length; }
            set
            {
                if (!double.Equals(m_Length,value))
                {
                    m_Length = value;
                    OnPropertyChanged();
                    OnPropertyChanged("LengthDisplay");
                }
            }
        }

        /// <summary>
        /// Gets the location/coordinate notation string in <see cref="MeasureDisplay.CoordinateFormat"/>
        /// </summary>
        public string CoordinateDisplay
        {
            get { return base.LocationToString(Location, base.CoordinateFormat); }
        }

        /// <summary>
        /// Gets the geodesic length in <see cref="MeasureDisplay.LinearUnitType"/>.
        /// </summary>
        public string LengthDisplay
        {
          get { return base.GeodesicLengthToString(Length, base.LinearUnitType); }
        }

        protected override void OnLinearUnitTypeChanged()
        {
            OnPropertyChanged("LengthDisplay");
            base.OnLinearUnitTypeChanged();
        }

        protected override void OnCoordinateFormatChanged()
        {
            OnPropertyChanged("CoordinateDisplay");
            base.OnCoordinateFormatChanged();
        }     
    }
}