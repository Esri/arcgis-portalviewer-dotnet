// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISPortalViewer.Helpers
{
    public interface INavigationService
    {
        void Navigate(string sourcePageName); 
        void Navigate(Type sourcePageType);
        void Navigate(string sourcePageName, object parameter);
        void Navigate(Type sourcePageType, object parameter);
        void GoBack();        
    }
}
