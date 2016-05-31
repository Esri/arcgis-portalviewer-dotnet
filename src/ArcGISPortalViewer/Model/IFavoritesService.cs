// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using Esri.ArcGISRuntime.Portal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISPortalViewer.Model
{
    public interface IFavoritesService
    {
        ObservableCollection<ArcGISPortalItem> Favorites { get; set; }
        List<string> GetFavoritesIds();
        void AddToFavorites(ArcGISPortalItem portalItemViewModel);
        bool DeleteFavoritesList();
        bool RemoveFromFavorites(ArcGISPortalItem portalItemViewModel);
        void SaveFavorites(List<string> favoriteItems);
        Task<bool> SetFavoritesCollection();
    }
}
