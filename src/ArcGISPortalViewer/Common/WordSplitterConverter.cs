using System;
using Windows.UI.Xaml.Data;

namespace ArcGISPortalViewer.Common
{
    public class WordSplitterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                var str = value as string;
                int idx = str.IndexOf(' ');
                if (idx <= 0) return value;
                return (parameter as string == "first") ? str.Substring(0, idx) : str.Substring(idx);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
