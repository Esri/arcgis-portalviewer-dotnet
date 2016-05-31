// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using System;
using Windows.UI.Xaml.Data;

namespace ArcGISPortalViewer.Common
{
    public class ToStringFormatConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null) return value;
            if (value is Int32)
            {
                string format = (string)parameter;
                int integer = (int)value;
                return integer.ToString(format);
            }
            if (value is DateTime)
            {
                string format = (string)parameter;
                DateTime dt = (DateTime)value;
                return dt.ToString(format);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
