using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;

namespace ArcGISPortalViewer.Helpers
{
    public static class IdentifyHelper
    {
		private const int c_defaultTolerance = 15;

		public static async Task<IDictionary<Layer, IEnumerable<IdentifyFeature>>> Identify(MapViewController controller, Point tapPoint, MapPoint mapPoint, IEnumerable<Layer> layers = null, double toleranceInPixels = 5)
        {
            if (controller == null)
                throw new ArgumentNullException("controller");
            if(layers == null)
                throw new ArgumentNullException("layers");

            var identifyLayers = LayerCollection.EnumerateLeaves(layers);            

            var results = await Task.WhenAll((from l in identifyLayers where l.IsVisible select IdentifyLayer(controller, l, tapPoint, mapPoint)).Where((l => l != null)).ToArray()).ConfigureAwait(false);

            IDictionary<Layer, IEnumerable<IdentifyFeature>> taskResults = null;
            foreach (var result in results)
            {
                if(taskResults == null)
                    taskResults = new Dictionary<Layer, IEnumerable<IdentifyFeature>>();
                if(!taskResults.ContainsKey(result.Key))
                    taskResults.Add(result);
            }

            return taskResults;
        }

        private static async Task<KeyValuePair<Layer,IEnumerable<IdentifyFeature>>> IdentifyLayer(MapViewController controller, Layer layer, Point tapPoint, MapPoint mapPoint)
        {
            if (layer is ArcGISDynamicMapServiceLayer)
            {
                var dynamicLayer = ((ArcGISDynamicMapServiceLayer)layer);
                
                if (!dynamicLayer.ServiceInfo.Capabilities.Contains("Query"))                
                    return new KeyValuePair<Layer, IEnumerable<IdentifyFeature>>(layer, null);
                
                var identifyTask = new IdentifyTask(new Uri(dynamicLayer.ServiceUri, UriKind.Absolute));

				var resolution = controller.UnitsPerPixel;
				var center = controller.Extent.GetCenter();
				var extent = new Envelope(center.X - resolution * c_defaultTolerance, center.Y - resolution * c_defaultTolerance,
					center.X + resolution * c_defaultTolerance, center.Y + resolution * c_defaultTolerance, controller.SpatialReference);
                var identifyParameter = new IdentifyParameter(mapPoint, extent, c_defaultTolerance, c_defaultTolerance,
                    c_defaultTolerance, DisplayInformation.GetForCurrentView().LogicalDpi)
				{
                    LayerOption = LayerOption.Visible,
                    LayerTimeOptions = dynamicLayer.LayerTimeOptions,
                    LayerIDs = dynamicLayer.VisibleLayers,
                    DynamicLayerInfos = dynamicLayer.DynamicLayerInfos,
                    GeodatabaseVersion = dynamicLayer.GeodatabaseVersion,
                    TimeExtent = controller.TimeExtent,
                };
                var identifyItems = (await identifyTask.ExecuteAsync(identifyParameter)).Results;
                var identifyFeatures = new List<IdentifyFeature>();
                foreach (var identifyItem in identifyItems)
                {
                    var fields = await GetFieldInfo(dynamicLayer, identifyItem);
                    var identifyFeature = ReplaceAlaisWithFieldName(identifyItem, fields);
                    identifyFeatures.Add(identifyFeature);
                }
                return new KeyValuePair<Layer, IEnumerable<IdentifyFeature>>(layer, identifyFeatures);
            }
            if (layer is ArcGISTiledMapServiceLayer)
            {
                var tiledlayer = ((ArcGISTiledMapServiceLayer) layer);
                if (!tiledlayer.ServiceInfo.Capabilities.Contains("Query"))
                    return new KeyValuePair<Layer, IEnumerable<IdentifyFeature>>(layer, null);

                var identifyTask = new IdentifyTask(new Uri(tiledlayer.ServiceUri, UriKind.Absolute));
				var resolution = controller.UnitsPerPixel;
				var center = controller.Extent.GetCenter();
				var extent = new Envelope(center.X - resolution * c_defaultTolerance, center.Y - resolution * c_defaultTolerance,
					center.X + resolution * c_defaultTolerance, center.Y + resolution * c_defaultTolerance, controller.SpatialReference);
                var identifyParameter = new IdentifyParameter(mapPoint, extent, c_defaultTolerance, c_defaultTolerance,
                    c_defaultTolerance, DisplayInformation.GetForCurrentView().LogicalDpi)
                {
                    LayerOption = LayerOption.Visible,                    
                    TimeExtent = controller.TimeExtent,
                };

                var identifyItems = (await identifyTask.ExecuteAsync(identifyParameter)).Results;
                
                var identifyFeatures = new List<IdentifyFeature>();
                foreach (var identifyItem in identifyItems)
                {
                    var fields = await GetFieldInfo(tiledlayer, identifyItem);
                    var identifyFeature = ReplaceAlaisWithFieldName(identifyItem, fields);
                    identifyFeatures.Add(identifyFeature);
                }                                
                return new KeyValuePair<Layer, IEnumerable<IdentifyFeature>>(layer, identifyFeatures);
            }
            if (layer is FeatureLayer)
            {
                var featureLayer = ((FeatureLayer)layer);
                var featureIds = await controller.FeatureLayerHitTestAsync(featureLayer, tapPoint, 1000);                
                var features = await featureLayer.FeatureTable.QueryAsync(featureIds);

                return new KeyValuePair<Layer, IEnumerable<IdentifyFeature>>(layer, features.Select(f => new IdentifyFeature(new IdentifyItem(
                    -1, 
                    featureLayer.DisplayName, 
                    f.Schema.Fields.FirstOrDefault(x=>x.Type == FieldType.Oid).Name,
                    f.Attributes[f.Schema.Fields.FirstOrDefault(x => x.Type == FieldType.Oid).Name].ToString(),
                    f
                    ),f.Schema.Fields)));
            }
            if (layer is GraphicsLayer)
            {
                var graphicsLayer = ((GraphicsLayer) layer);
                var graphics = await controller.GraphicsLayerHitTestAsync(graphicsLayer, tapPoint, 1000);                
                return new KeyValuePair<Layer, IEnumerable<IdentifyFeature>>(layer, graphics.Select(f => new IdentifyFeature(new IdentifyItem(
                    -1, 
                    layer.DisplayName,
                    "",
                    "",
                    f
                    ))));
            }

            // Not supported or not implemented yet
            return new KeyValuePair<Layer, IEnumerable<IdentifyFeature>>(layer,null);
        }

        private static IdentifyFeature ReplaceAlaisWithFieldName(IdentifyItem identifyItem, IReadOnlyList<FieldInfo> fields)
        {
            if (identifyItem == null || identifyItem.Feature == null || identifyItem.Feature.Attributes == null || fields == null || !fields.Any())
                return new IdentifyFeature(identifyItem);

            var identifyFeature = new IdentifyFeature(identifyItem);
            var attr = identifyItem.Feature.Attributes;
            foreach (var field in fields)
            {
                var key = attr.Keys.FirstOrDefault(k => k == field.Alias && k != field.Name);
                if (string.IsNullOrEmpty(key)) continue;
                attr[field.Name] = attr[key];
                attr.Remove(key);
                if (identifyItem.DisplayFieldName == key)
                    identifyFeature = new IdentifyFeature(new IdentifyItem(identifyItem.LayerID,
                        identifyItem.LayerName, field.Name, identifyItem.Value, identifyItem.Feature));
            }
            return identifyFeature;
        }

        private static async Task<IReadOnlyList<FieldInfo>> GetFieldInfo(Layer l, IdentifyItem item)
        {
            if (l == null || item == null || item.LayerID == -1)
                return null;

            if (l is ArcGISDynamicMapServiceLayer)
            {
                var dynamicLayer = (ArcGISDynamicMapServiceLayer)l;
                var layerinfo = await dynamicLayer.GetDetailsAsync(item.LayerID);
                return layerinfo != null ? layerinfo.Fields : null;
            }
            if (l is ArcGISTiledMapServiceLayer)
            {
                var tiledLayer = (ArcGISTiledMapServiceLayer)l;
                var layerinfo = await tiledLayer.GetDetailsAsync(item.LayerID);
                return layerinfo != null ? layerinfo.Fields : null;
            }
            return null;
        }
    }

    public class IdentifyFeature
    {
        public IdentifyFeature() { }
        public IdentifyFeature(IdentifyItem item)
        {
            Item = item;
        }
        public IdentifyFeature(IdentifyItem item, IEnumerable<FieldInfo> fields) : this(item)
        {
            Fields = fields;
        }
        public IEnumerable<FieldInfo> Fields { get; set; }
        public IdentifyItem Item { get; set; }
    }
}
