using ArcGISPortalViewer.Common;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using Esri.ArcGISRuntime.Portal;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;


namespace ArcGISPortalViewer.View
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class PortalCollectionPage : LayoutAwarePage
    {
        public PortalCollectionPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (navigationParameter == null)
                return;

            var collectionType = navigationParameter.GetType();
            if (collectionType == null)
                return;

            if (collectionType == typeof(IncremetalLoadingCollection))
                Messenger.Default.Send(new ChangeIncremetalCollectionMessage { ItemCollection = navigationParameter as IncremetalLoadingCollection });
            else if (collectionType == typeof(ObservableCollection<ArcGISPortalItem>))
                Messenger.Default.Send(new ChangeFavoritesCollectionMessage { ItemCollection = navigationParameter as ObservableCollection<ArcGISPortalItem>, Title = "Favorites" });
            else if (collectionType == typeof(PortalGroupCollection))
                Messenger.Default.Send(new ChangePortalGroupsCollectionMessage { ItemCollection = navigationParameter as ObservableCollection<ArcGISPortalGroup> });
            else if (collectionType == typeof(PortalItemCollection))
                Messenger.Default.Send(new ChangePortalItemsCollectionMessage { ItemCollection = navigationParameter as ObservableCollection<ArcGISPortalItem>, Title = "My Maps" });
        }
    }
}
