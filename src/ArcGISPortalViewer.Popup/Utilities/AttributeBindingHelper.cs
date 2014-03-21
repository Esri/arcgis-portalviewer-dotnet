using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Foundation.Collections;

namespace ArcGISPortalViewer.Popup.Utilities
{
    internal static class AttributeBindingHelper
    {
        private const string AttributeBindingRegex = @"({)([^}]*)(})"; // Regular expression to identify attribute bindings

        public static string ResolveBinding(IDictionary<string, object> dict, string formatter)
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
