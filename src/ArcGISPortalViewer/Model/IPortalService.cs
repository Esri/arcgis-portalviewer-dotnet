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
        //Task<IEnumerator<ArcGISPortalGroup>> GetGroups();
        //Task<IList<PortalItem>> GetGroups();
        //Task<IList<PortalItem>> GetPortalItems(SearchParameters searchParamaters);
        Task<IList<ArcGISPortalItem>> GetPortalItems(SearchParameters searchParamaters);
        //Task<IList<ArcGISPortalItem>> GetFavorites();
    }
}
