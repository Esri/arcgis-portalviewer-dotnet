// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using ArcGISPortalViewer.ViewModel;

namespace ArcGISPortalViewer.Common
{
    public class PopupItemFormatConverter : IValueConverter
    {
        private const string AttributeBindingRegex = @"({)([^}]*)(})"; // Regular expression to identify attribute bindings

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is PopupItem))
                return value;

            var popupItem = ((PopupItem)value);

            var dict = popupItem.IdentifyFeature.Item.Feature.Attributes;
            var formatter = popupItem.PopupInfo.Title;
            if (string.IsNullOrEmpty(formatter) && popupItem.PopupInfo.MediaInfos != null)
            {
                var mediaInfo = popupItem.PopupInfo.MediaInfos.FirstOrDefault();
                if (mediaInfo != null)
                    formatter = mediaInfo.Title;
            }
            if (dict == null || string.IsNullOrWhiteSpace(formatter))
                return null;
            if (!string.IsNullOrEmpty(formatter))
                value = ResolveBinding(dict, formatter);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        internal static string ResolveBinding(IDictionary<string, object> dict, string formatter)
        {
            if (dict == null || formatter == null) return null;
            var splitStringArray = Regex.Split(formatter, AttributeBindingRegex);
            var isAttributeName = false;
            var sb = new StringBuilder();
            foreach (var str in splitStringArray)
            {
                if (str == "{") { isAttributeName = true; continue; }
                if (str == "}") { isAttributeName = false; continue; }
                if (isAttributeName && dict.ContainsKey(str))
                {
                    var temp = dict[str];
                    if (temp != null)
                    {
                        sb.AppendFormat("{0}", temp);
                    }
                }
                else if (!isAttributeName)
                    sb.AppendFormat("{0}", str);
            }
            return sb.ToString().Replace("$LINEBREAK$", "<br/>").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&apos;", "'").Replace("&quot;", "\"");
        }
    }
}
