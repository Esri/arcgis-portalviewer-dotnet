// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using System.Collections.Generic;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Portal;
using ArcGISPortalViewer.Model;

namespace ArcGISPortalViewer.Design
{
    class DesignPortalService : IPortalService
    {

        #region IPortalService Members

        public Task<IList<ArcGISPortalGroup>> GetGroups()
        {
            return Task.FromResult<IList<ArcGISPortalGroup>>(new List<ArcGISPortalGroup>());
        }

        public Task<IList<ArcGISPortalItem>> GetPortalItems(SearchParameters searchParamaters)
        {
            return Task.FromResult<IList<ArcGISPortalItem>>(new List<ArcGISPortalItem>());
        }

        #endregion
    }
}
