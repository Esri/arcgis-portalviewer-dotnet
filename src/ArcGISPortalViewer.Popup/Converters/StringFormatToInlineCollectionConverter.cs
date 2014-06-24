// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using ArcGISPortalViewer.Popup.Utilities;

namespace ArcGISPortalViewer.Popup.Converters
{
    internal sealed class StringFormatToInlineCollectionConverter : IValueConverter
    {
        private const string HtmlLineBreakRegex = @"<br ?/?>";     // Regular expression to strip HTML line break tag        
        private const string HtmlStripperRegex = @"<(.|\n)*?>";    // Regular expression to strip HTML tags         

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var dict = value as IDictionary<string, object>;
            //var text = Regex.Replace(parameter as string, HtmlLineBreakRegex, string.Empty);
            var formatter = parameter as string;
            if (dict == null || string.IsNullOrWhiteSpace(formatter))
                return null;
            if (!string.IsNullOrEmpty(formatter))
            {
                value = AttributeBindingHelper.ResolveBinding(dict, formatter);
            }
            if (value is string)
            {
                value = CreateInlineCollection(value as string);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private static object CreateInlineCollection(string value)
        {
            var inlineCollection = new List<Inline>();
            var lines = Regex.Split(value, HtmlLineBreakRegex, RegexOptions.IgnoreCase);
            var skip = true;
            foreach (var line in lines)
            {
                if (!skip)
                    inlineCollection.Add(new LineBreak());
                else
                    skip = false;

                // Remove the rest of HTML tags.
                var strRun = Regex.Replace(line, HtmlStripperRegex, string.Empty, RegexOptions.IgnoreCase);
                if (!string.IsNullOrEmpty(strRun))
                {
                    var run = new Run { Text = strRun };
                    inlineCollection.Add(run);
                }
            }
            return inlineCollection;
        }
    }
}

