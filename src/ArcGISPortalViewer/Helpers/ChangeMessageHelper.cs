// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Esri.ArcGISRuntime.Portal;
using System.Collections.ObjectModel;

namespace ArcGISPortalViewer.Helpers
{
    public enum PortalQuery
    {
        Default = 0,                // sorts by relevance
        Featured = 1,               // filters featured items
        Recent = 2,                 // sorts by most recent
        HighestRated = 3,           // sorts by highest rating
        Favorites = 4,              // filters favorite items
        MyGroups = 5,               // filters items in my groups 
        MyMaps = 6,                 // filters my maps
        MostPopular = 7,            // sorts by most views
        MostComments = 8,           // sorts by most comments
        Title = 9,                  // sorts by title
        Owner = 10                  // sorts by owner 
    }

    public class ChangePortalQueryMessage
    {
        public ChangePortalQueryMessage(PortalQuery portalQuery)
        {
            PQuery = portalQuery;
        }
        public PortalQuery PQuery { get; private set; }
    }

    class ChangeItemSelectedMessage
    {
        public ArcGISPortalItem Item { get; set; }
    }

    class ChangeGroupSelectedMessage
    {
        public ArcGISPortalGroup Group { get; set; }
    }

    class ChangeItemCollectionMessage
    {
        public object ItemCollection { get; set; }        
    }

    class ChangeIncremetalCollectionMessage
    {
        public IncremetalLoadingCollection ItemCollection { get; set; }
    }

    class ChangePortalItemsCollectionMessage
    {
        public ObservableCollection<ArcGISPortalItem> ItemCollection { get; set; }
        public string Title { get; set; }
    }

    class ChangeFavoritesCollectionMessage
    {
        public ObservableCollection<ArcGISPortalItem> ItemCollection { get; set; }
        public string Title { get; set; }
    }

    class ChangePortalGroupsCollectionMessage
    {
        public ObservableCollection<ArcGISPortalGroup> ItemCollection { get; set; }
    }

    class RemoveItemFromFavoritesMessage
    {
        public ArcGISPortalItem Item { get; set; }
    }

    class AddItemToFavoritesMessage
    {
        public ArcGISPortalItem Item { get; set; }
    }

    class ChangeSignInMessage { }

    class ChangeSignOutMessage { }

    class ChangeAnonymousAccessMessage { }

    class PopulateDataMessage { }

    class ChangedPortalServiceMessage { }
}
