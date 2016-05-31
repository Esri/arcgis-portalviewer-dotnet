// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;
using Esri.ArcGISRuntime.Layers;

namespace ArcGISPortalViewer.Common
{
    class ReverseOrderConverter : IValueConverter  
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Reverse(value, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Reverse(value, targetType);
        }

        private static object Reverse(object value, Type targetType)
        {
            if (value is IEnumerable<Layer> && targetType == typeof(IEnumerable<Layer>))
                return ((IEnumerable<Layer>)value).Reverse();
            return value;
        }
    }
}
