// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Esri.ArcGISRuntime.Portal;
using ArcGISPortalViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ArcGISPortalViewer.Model
{
    public class FavoritesService : IFavoritesService
    {
        private ObservableCollection<ArcGISPortalItem> _favorites;
        public ObservableCollection<ArcGISPortalItem> Favorites
        {
            get
            {
                return _favorites;
            }
            set
            {
                _favorites = value;
            }
        }

        private static FavoritesService _currentFavoritesService;
        public static FavoritesService CurrentFavoritesService
        {
            get
            {
                if (_currentFavoritesService == null)
                    _currentFavoritesService = new FavoritesService();

                return _currentFavoritesService;
            }
        }

        public FavoritesService()
        {
            //DeleteFavoritesList();            
           _currentFavoritesService = this;          
        }       

        public List<string> GetFavoritesIds()
        {
            var favorites = new List<string>();
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values["FavoritesList"] != null)
            {
                var ids = roamingSettings.Values["FavoritesList"] as string[];
                favorites.AddRange(ids);
            }
            return favorites;
        }

        public void AddToFavorites(ArcGISPortalItem portalItemViewModel)
        {
            if (portalItemViewModel == null || portalItemViewModel == null)
                return;

            string itemId = portalItemViewModel.Id;
            List<string> favoriteItems = GetFavoritesIds();
            if (string.IsNullOrEmpty(itemId) || favoriteItems == null || favoriteItems.Contains(itemId))
                return;
            
            //add item to Favorites collection
            Favorites.Add(portalItemViewModel);
            // add item id to favoriteItems
            favoriteItems.Add(itemId);
            // persist favoriteItems
            SaveFavorites(favoriteItems);
        }

        public bool DeleteFavoritesList()
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values["FavoritesList"] != null)
                return roamingSettings.Values.Remove("FavoritesList");
            return false;
        }

        public bool RemoveFromFavorites(ArcGISPortalItem portalItemViewModel)
        {
            if (portalItemViewModel == null || portalItemViewModel == null)
                return false;

            string itemId = portalItemViewModel.Id;
            List<string> favoriteItems = GetFavoritesIds();
            if (string.IsNullOrEmpty(itemId) || !favoriteItems.Contains(itemId))
                return false;

            try
            {
                // remove portalItemViewModel from favorites                
                Favorites.Remove(portalItemViewModel);
            }
            catch (Exception)
            {
            }

            // remove item id from favoriteItems and persist it
            if (favoriteItems.Remove(itemId))
            {
                SaveFavorites(favoriteItems);
                return true;
            }
            return false;
        }

        public void SaveFavorites(List<string> favoriteItems)
        {
            if (favoriteItems == null)
                return;

            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            try
            {
                roamingSettings.Values["FavoritesList"] = favoriteItems.Count > 0 ? favoriteItems.ToArray() : new string[]{""};
            }
            catch (Exception ex)
            {
                var _ = App.ShowExceptionDialog(ex);
            }
        }

        public async Task<bool> SetFavoritesCollection()
        {
            SearchParameters sp = SearchService.CreateSearchParameters("", PortalQuery.Favorites, 0, 20, GetFavoritesIds());
            IList<ArcGISPortalItem> r = await PortalService.CurrentPortalService.GetPortalItems(sp);
            if (r != null)
            {
                Favorites = new ObservableCollection<ArcGISPortalItem>(r);
                return true;
            }
            else
            {
                Favorites = new ObservableCollection<ArcGISPortalItem>();
                return false;
            }
        }
    }
}
