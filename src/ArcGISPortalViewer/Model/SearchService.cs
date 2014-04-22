using Esri.ArcGISRuntime.Portal;
using ArcGISPortalViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISPortalViewer.Model
{
    public static class SearchService
    {
        public static SearchParameters CreateSearchParameters(string searchText, PortalQuery portalQuery, int startIndex = 1, int limit = 20, IList<string> favoriteItemIDs = null)
        {
            string queryString = string.Format("{0} ({1})", searchText, "type:\"web map\" NOT type:\"web mapping application\"");            
            string sortField = "";
            QuerySortOrder sortOrder = QuerySortOrder.Descending;

            switch (portalQuery)
            {
                case PortalQuery.Default:
                    //searchParamters.QueryString = "sdfgsdfhh type:\"web map\" NOT \"web mapping application\"";
                    break;
                case PortalQuery.Recent:
                    sortField = "uploaded";
                    break;
                case PortalQuery.HighestRated:
                    sortField = "avgrating";
                    break;
                case PortalQuery.MostComments:
                    sortField = "numcomments";
                    break;
                case PortalQuery.MostPopular:
                    sortField = "numviews";
                    break;
                case PortalQuery.Favorites:
                    queryString = GenerateFavoriteQueryFromIDs(favoriteItemIDs);
                    break;
                case PortalQuery.MyGroups:
                    break;
                case PortalQuery.MyMaps:
                    break;
                case PortalQuery.Title:
                    sortField = "title";
                    sortOrder = QuerySortOrder.Ascending;
                    break;
                case PortalQuery.Owner:
                    sortField = "owner";
                    sortOrder = QuerySortOrder.Ascending;
                    break;
            }

            SearchParameters searchParamters = new SearchParameters(queryString);
            searchParamters.StartIndex = startIndex;
            searchParamters.Limit = limit;            
            searchParamters.SortField = sortField;
            searchParamters.SortOrder = sortOrder;
            return searchParamters;        
        }
        
        private static string GenerateFavoriteQueryFromIDs(IList<string> favoriteItemIDs)
        {
            if (favoriteItemIDs == null || favoriteItemIDs.Count <= 0)
                return "";
            
            var queryString = "";
            for (var i = 0; i < favoriteItemIDs.Count; i++)
            {
                queryString = (i == 0) ? 
                    String.Format("id: {0}" , favoriteItemIDs[i]) :
                    String.Format("{0} OR id: {1}", queryString, favoriteItemIDs[i]);
            }
            return queryString;
        }
    }
}
