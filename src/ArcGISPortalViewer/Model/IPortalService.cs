// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Portal;

namespace ArcGISPortalViewer.Model
{
    public interface IPortalService
    {
        Task<IList<ArcGISPortalGroup>> GetGroups();
        Task<IList<ArcGISPortalItem>> GetPortalItems(SearchParameters searchParamaters);
    }
}
