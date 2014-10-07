// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Windows.Foundation;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace ArcGISPortalViewer.Helpers
{
    public class MapViewController : INotifyPropertyChanged
    {
        private WeakReference<MapView> m_map;

        public MapViewController()
        {
        }

        #region Commands

        public void ResetMapRotation()
        {
            if (MapView == null)
                return;
            MapView.SetRotationAsync(0);
        }

        public Task<bool> SetViewAsync(Geometry geometry)
        {
            var map = MapView;
            if (map != null && geometry != null)
                return map.SetViewAsync(geometry);
             
            return Task.FromResult(false);
        }

        public Task<bool> SetViewAsync(Geometry geometry, TimeSpan duration)
        {
            var map = MapView;
            if (map != null && geometry != null)
                return map.SetViewAsync(geometry, duration);

            return Task.FromResult(false);
        }

        public Task<bool> SetViewAsync(Geometry geometry, Thickness margin)
        {
            var map = MapView;
            if (map != null && geometry != null)
                return map.SetViewAsync(geometry, margin);

            return Task.FromResult(false);
        }

        public bool CanSetView(Envelope extent)
        {
            MapView map = MapView;
            return (map != null && map.SpatialReference != null && extent != null &&
                    (extent.Width > 0 || extent.Height > 0));
        }

        public async Task<bool> FlyToAsync(MapPoint to, double scale, Thickness margin)
        {
            var mapExtent = MapView.Extent;
            if (!GeometryEngine.Contains(mapExtent, to)) //Destination is outside current view
            {
                if (!(await MapView.SetViewAsync(mapExtent.Union(to), TimeSpan.FromSeconds(1), margin) && //Zoom out to see both destinations
                      await MapView.SetViewAsync(to, TimeSpan.FromSeconds(1.5), margin))) //center on destination
                    return false;
            }
            return await MapView.SetViewAsync(to, scale, margin);
        }

        public async Task<bool> FlyToAsync(Geometry flyTo, Thickness margin)
        {
            Envelope to = flyTo.Extent;
            if (to.Width > 0 || to.Height > 0)
            {
                var mapExtent = MapView.Extent;
                if (!GeometryEngine.Contains(mapExtent, to)) //Destination is outside current view
                {
                    if (!(await MapView.SetViewAsync(mapExtent.Union(to), TimeSpan.FromSeconds(1), margin) && //Zoom out to see both destinations
                           await MapView.SetViewAsync(to, TimeSpan.FromSeconds(1.5), margin))) //center on destination
                        return false;
                }
                return await MapView.SetViewAsync(to, margin);
            }            
            return await MapView.SetViewAsync(to, margin); //Pan to Point
        }


        private DelegateCommand m_SetViewCommand;

        /// <summary>
        /// Calls SetView on the Envelope provided in the <see cref="System.Windows.Input.ICommand.CommandParameter"/>
        /// </summary>
        public System.Windows.Input.ICommand SetViewCommand
        {
            get
            {
                if (m_SetViewCommand == null)
                {
                    m_SetViewCommand = new DelegateCommand(
                        (parameter) => SetViewAsync(parameter as Envelope),
                        (parameter) => CanSetView(parameter as Envelope));
                }
                return m_SetViewCommand;
            }
        }

        public  Task<IEnumerable<Graphic>> GraphicsLayerHitTestAsync(GraphicsLayer graphicsLayer, Point point,
            int maxHits = 10)
        {
            if (graphicsLayer == null)
                return Task.FromResult(System.Linq.Enumerable.Empty<Graphic>());

            var map = MapView;

            return graphicsLayer.HitTestAsync(map, point, maxHits);
        }

        public Task<long[]> FeatureLayerHitTestAsync(FeatureLayer featureLayer, Point point, int maxHits = 1)
        {
            return featureLayer.HitTestAsync(MapView, point, maxHits);
        }

        #endregion Commands

        #region Properties

        public double UnitsPerPixel
        {
            get
            {
                MapView map = MapView;
                if (map != null)
                {
                    return map.UnitsPerPixel;
                }
                return double.NaN;
            }
        }

        public SpatialReference SpatialReference
        {
            get
            {
                MapView map = MapView;
                if (map != null)
                {
                    return map.SpatialReference;
                }
                return null;
            }
        }

        public Envelope Extent
        {
            get
            {
                MapView map = MapView;
                if (map != null)
                {
                    return map.Extent;
                }
                return null;
            }
        }

        public TimeExtent TimeExtent
        {
            get { return MapView != null ? MapView.TimeExtent : null; }
        }

        #endregion Properties

        #region MapView handling

        private MapView MapView
        {
            get
            {
                MapView map = null;
                if (m_map.TryGetTarget(out map))
                    return map;
                return null;
            }
        }

        public static MapView GetMapView(DependencyObject obj)
        {
            return (MapView) obj.GetValue(MapProperty);
        }

        public static void SetMapView(DependencyObject obj, MapView value)
        {
            obj.SetValue(MapProperty, value);
        }

        public static readonly DependencyProperty MapProperty =
            DependencyProperty.RegisterAttached("MapView", typeof (MapView), typeof (MapViewController),
                new PropertyMetadata(null, OnMapViewPropertyChanged));

        private static void OnMapViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MapView && e.OldValue is MapViewController)
            {
                var controller = (e.OldValue as MapViewController);
                controller.m_map = null;
                var delegateCommand = controller.SetViewCommand as DelegateCommand;
                if (delegateCommand != null)
                    delegateCommand.OnCanExecuteChanged();
            }
            if (d is MapView && e.NewValue is MapViewController)
            {
                var controller = (e.NewValue as MapViewController);
                controller.m_map = new WeakReference<MapView>(d as MapView);

                var loadedListener = new WeakEventListener<MapView, object, PropertyChangedEventArgs>(d as MapView);
                loadedListener.OnEventAction =
                    (instance, source, eventArgs) => controller.MapViewController_PropertyChanged(source, eventArgs);

                // the instance passed to the action is referenced (i.e. instance.Loaded) so the lambda expression is 
                // compiled as a static method.  Otherwise it targets the map instance and holds it in memory.
                loadedListener.OnDetachAction = (instance, listener) =>
                {
                    if (instance != null)
                        instance.PropertyChanged -= listener.OnEvent;
                };
                (d as MapView).PropertyChanged += loadedListener.OnEvent;
                loadedListener = null;
                if (controller.m_SetViewCommand != null)
                    controller.m_SetViewCommand.OnCanExecuteChanged();
            }
        }

        private void MapViewController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SpatialReference")
            {
                if (m_SetViewCommand != null) m_SetViewCommand.OnCanExecuteChanged();
                OnPropertyChanged("SpatialReference");
            }
            else if (e.PropertyName == "UnitsPerPixel")
                OnPropertyChanged("UnitsPerPixel");
			else if (e.PropertyName == "TimeExtent")
				OnPropertyChanged("TimeExtent");
		}

        #endregion

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    internal class DelegateCommand : ICommand
    {
        private Action<object> m_execute;
        private Func<object, bool> m_canExecute;

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            m_execute = execute;
            m_canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return m_canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            m_execute(parameter);
        }

        public void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Implements a weak event listener that allows the owner to be garbage
    /// collected if its only remaining link is an event handler.
    /// </summary>
    /// <typeparam name="TInstance">Type of instance listening for the event.</typeparam>
    /// <typeparam name="TSource">Type of source for the event.</typeparam>
    /// <typeparam name="TEventArgs">Type of event arguments for the event.</typeparam>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Used as link target in several projects.")]
    internal class WeakEventListener<TInstance, TSource, TEventArgs> where TInstance : class
    {
        /// <summary>
        /// WeakReference to the instance listening for the event.
        /// </summary>
        private WeakReference _weakInstance;

        /// <summary>
        /// Gets or sets the method to call when the event fires.
        /// </summary>
        public Action<TInstance, TSource, TEventArgs> OnEventAction { get; set; }

        /// <summary>
        /// Gets or sets the method to call when detaching from the event.
        /// </summary>
        public Action<TInstance, WeakEventListener<TInstance, TSource, TEventArgs>> OnDetachAction { get; set; }

        /// <summary>
        /// Initializes a new instances of the WeakEventListener class.
        /// </summary>
        /// <param name="instance">Instance subscribing to the event.</param>
        public WeakEventListener(TInstance instance)
        {
            if (null == instance)
            {
                throw new ArgumentNullException("instance");
            }
            _weakInstance = new WeakReference(instance);
        }

        /// <summary>
        /// Handler for the subscribed event calls OnEventAction to handle it.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="eventArgs">Event arguments.</param>
        public void OnEvent(TSource source, TEventArgs eventArgs)
        {
            TInstance target = (TInstance) _weakInstance.Target;
            if (null != target)
            {
                // Call registered action
                if (null != OnEventAction)
                {
                    OnEventAction(target, source, eventArgs);
                }
            }
            else
            {
                // Detach from event
                Detach();
            }
        }

        /// <summary>
        /// Detaches from the subscribed event.
        /// </summary>
        public void Detach()
        {
            TInstance target = (TInstance) _weakInstance.Target;
            if (null != OnDetachAction)
            {
                OnDetachAction(target, this);
                OnDetachAction = null;
            }
        }
    }
}
