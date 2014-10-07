// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using Esri.ArcGISRuntime.Portal;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISPortalViewer.Helpers
{
    public class NavPageParam
    {
        public string PageName { get; set; }
        public object Parameter { get; set; }
    }

    public class NavigationService : INavigationService
    {
        public void Navigate(string sourcePageName)
        {
            if (string.IsNullOrEmpty(sourcePageName))
                return;

            Type sourcePageType = Type.GetType(sourcePageName);
            if (sourcePageType != null)
                Navigate(sourcePageType);
        }

        public void Navigate(Type sourcePageType)
        {
            ((Frame)Window.Current.Content).Navigate(sourcePageType);
            // reset AppViewModel when navigating between pages
            ArcGISPortalViewer.ViewModel.AppViewModel.CurrentAppViewModel.ResetProperties();
        }

        public void Navigate(string sourcePageName, object parameter)
        {
            if (string.IsNullOrEmpty(sourcePageName))
                return;

            Type sourcePageType = Type.GetType(sourcePageName);
            if (sourcePageType != null)
                Navigate(sourcePageType, parameter);
        }

        public void Navigate(Type sourcePageType, object parameter)
        {
            ((Frame)Window.Current.Content).Navigate(sourcePageType, parameter);
            // reset AppViewModel when navigating between pages
            if (parameter is ArcGISPortalItem)
                ArcGISPortalViewer.ViewModel.AppViewModel.CurrentAppViewModel.SelectedPortalItem = (ArcGISPortalItem)parameter;
            else
                ArcGISPortalViewer.ViewModel.AppViewModel.CurrentAppViewModel.ResetProperties();
        }

        public void GoBack()
        {
            ((Frame)Window.Current.Content).GoBack();
        }
    }
}
