using System;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ArcGISPortalViewer.Common
{
    /// <summary>
    /// The Bi-Conditional Converter is designed to have two possible 
    /// outcomes produced by the evaluation of the proposed conversion value. 
    /// If the condition evaluates to'true' then the left hand value is returned 
    /// otherwise the right hand value is returned. In addition a "reverse" keyword
    /// can be used in the converter parameter property to invert the condition in 
    /// which case an evaluation that results in 'true' will return the right hand value 
    /// and 'false' will return the left hand value.
    /// </summary>
    public sealed class BiConditionalConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ConvertValue(value, targetType, (string)parameter == "reverse");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertValue(value, targetType, (string)parameter == "reverse");
        }

        private object ConvertValue(object value, Type targetType, bool invert = false)
        {

            if (value == null)
            {
                if (targetType == typeof(Visibility))
                    return (invert) ? Visibility.Visible : Visibility.Collapsed;
                if (targetType == typeof(bool))
                    return (invert);
            }
            else
            {
                if (value is Visibility)
                {
                    if (targetType == typeof(bool))
                        return (((Visibility)value == Visibility.Visible) != invert);
                }
                else if (value is bool)
                {
                    if (targetType == typeof(Visibility))
                        return (((bool)value) != invert) ? Visibility.Visible : Visibility.Collapsed;
                    if (targetType == typeof(bool))
                        return (((bool)value) != invert);
                }
                else if (value is int)
                {
                    if (targetType == typeof(Visibility))
                        return ((((int)value) > 0) != invert) ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (value is IEnumerable)
                {
                    if (targetType == typeof(Visibility))
                        return ((value as IEnumerable).GetEnumerator().MoveNext() && !invert) ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    if (targetType == typeof(Visibility))
                        return (!invert) ? Visibility.Visible : Visibility.Collapsed;
                    if (targetType == typeof(bool))
                        return (!invert);    
                }                
            }                        

            return value;
        }
    }
}
