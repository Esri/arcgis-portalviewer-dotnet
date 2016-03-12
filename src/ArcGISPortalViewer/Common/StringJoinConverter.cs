// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace ArcGISPortalViewer.Common
{
    public sealed class StringJoinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IEnumerable<string>)
            {
                return string.Join(parameter is string ? (string)parameter : "", (value as IEnumerable<string>).ToArray());
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
