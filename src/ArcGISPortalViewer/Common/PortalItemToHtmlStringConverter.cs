// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Text;
using Windows.UI.Xaml.Data;
using Esri.ArcGISRuntime.Portal;

namespace ArcGISPortalViewer.Common
{
    class PortalItemToHtmlStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ArcGISPortalItem && targetType == typeof(string))
                return ConvertPortalItemToHtmlString((ArcGISPortalItem)value);
            if (value is ArcGISPortalGroup && targetType == typeof(string))
                return ConvertPortalGroupToHtmlString((ArcGISPortalGroup)value);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private static string ConvertPortalItemToHtmlString(ArcGISPortalItem portalItem)
        {
            if (portalItem != null)
            {
                var sb = new StringBuilder();
                sb.Append("<h2>");
                sb.Append("Description");
                sb.Append("</h2>");
                sb.Append(portalItem.Description);
                sb.Append("<h2>");
                sb.Append("Access and Use Constraints");
                sb.Append("</h2>");
                sb.Append(portalItem.LicenseInfo);
                sb.Append("<h2>");
                sb.Append("Credits");
                sb.Append("</h2>");
                sb.Append(portalItem.AccessInformation);
                sb.Append("<h2>");
                sb.Append("Tags");
                sb.Append("</h2>");
                var index = 0;
                foreach (var tag in portalItem.Tags)
                {
                    if (index++ != 0)
                        sb.Append("<span>, </span>");
                    sb.AppendFormat("<a href=\"arcgis://search/{0}\" style=\"text-decoration:none\" >{1}</a>", Uri.EscapeDataString(tag), tag);
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        private static string ConvertPortalGroupToHtmlString(ArcGISPortalGroup portalGroup)
        {
            if (portalGroup != null)
            {
                var sb = new StringBuilder();
                sb.Append("<h2>");
                sb.Append("Description");
                sb.Append("</h2>");
                sb.Append(portalGroup.Description);
                sb.Append("<h2>");
                sb.Append("Tags");
                sb.Append("</h2>");
                var index = 0;
                foreach (var tag in portalGroup.Tags)
                {
                    if (index++ != 0)
                        sb.Append("<span>, </span>");
                    sb.AppendFormat("<a href=\"arcgis://search/{0}\" style=\"text-decoration:none\" >{1}</a>", Uri.EscapeDataString(tag), tag);
                }
                return sb.ToString();
            }
            return string.Empty;
        }
    }
}
