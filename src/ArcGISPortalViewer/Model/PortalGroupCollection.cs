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
    public class PortalGroupCollection : ObservableCollection<ArcGISPortalGroup>
    {
        public PortalGroupCollection(IEnumerable<ArcGISPortalGroup> groups, string title)
            : base(groups)
        {
            Title = title;
        }

        public string UniqueId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Uri ThumbnailUri { get; set; }
    }
}
