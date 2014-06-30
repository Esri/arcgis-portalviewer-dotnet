// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Esri.ArcGISRuntime.Portal;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using ArcGISPortalViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ArcGISPortalViewer.Common
{
    class CollectionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;

            Type valueType = value.GetType();
            if (valueType == typeof(IncremetalLoadingCollection))
                return value as IncremetalLoadingCollection;
            if (valueType == typeof(ObservableCollection<ArcGISPortalItem>))
                return value as ObservableCollection<ArcGISPortalItem>;
            if (valueType == typeof(ObservableCollection<ArcGISPortalGroup>))
                return value as ObservableCollection<ArcGISPortalGroup>;
            if (valueType == typeof(PortalGroupCollection))
                return value as ObservableCollection<ArcGISPortalGroup>;
            if (valueType == typeof(PortalItemCollection))
                return value as ObservableCollection<ArcGISPortalItem>;
            if (valueType == typeof(ObservableCollection<CollectionAndTitle>))
                return value as ObservableCollection<CollectionAndTitle>;
            else
                return null;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
