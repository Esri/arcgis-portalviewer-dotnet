using System.Linq;
using Esri.ArcGISRuntime.Portal;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;

namespace ArcGISPortalViewer.ViewModel
{
    public class PortalGroupViewModel : ViewModelBase
    {
        #region Constructor

        public PortalGroupViewModel()
        {
            Messenger.Default.Register<ChangeGroupSelectedMessage>(this,
                msg => { PortalGroup = msg.Group; });

            // initialize ListGroupContent RelayCommand
            ListGroupContentCommand = new RelayCommand<ArcGISPortalGroup>(async pg =>
            {
                if (pg == null)
                    return;

                var gsItems = await pg.GetItemsAsync();
                if (gsItems == null || !gsItems.Any())
                    return;

                // filter out any ArcGISPortalItem that is not a WebMap 
                var webMapItems = gsItems.Where(item => item.Type.ToString().ToLower().Equals("webmap"));

                // create an observable collection of the WebMap items 
                var groupSharedItems = new ObservableCollection<ArcGISPortalItem>(webMapItems);
                if (!groupSharedItems.Any())
                    return;

                // send ChangePortalItemsCollectionMessage message to other ViewModels who are registered with it.
                Messenger.Default.Send<ChangePortalItemsCollectionMessage>(new ChangePortalItemsCollectionMessage()
                {
                    ItemCollection = groupSharedItems,
                    Title = string.Format("Content of Group: {0}", pg.Title)
                });
                // use the navigation service to navigate to the page showing the specific collection of portal items
                (new NavigationService()).Navigate(App.CollectionPageName);
            });
        }

        #endregion Constructor

        #region Public Properties

        #region PortalGroup

        public ArcGISPortalGroup PortalGroup { get; set; }

        #endregion PortalGroup

        #region ListGroupContentCommand

        /// <summary>
        /// Gets the ListGroupContent.
        /// </summary>
        public RelayCommand<ArcGISPortalGroup> ListGroupContentCommand { get; set; }

        #endregion ListGroupContent

        #region FullName

        public string FullName
        {
            get { return PortalGroup.Title; }
        }

        #endregion FullName

        #region PortalGroupUrl

        public string PortalGroupUrl
        {
            get
            {
                //var portalUrl = "http://" + PortalGroup.ArcGISPortal.ArcGISPortalInfo.CustomBaseUrl;
                var portalUrl = App.OrganizationUrl;
                string urlBase = portalUrl + "/home/group.html?id=" + PortalGroup.Id + "&token=" +
                                 PortalService.CurrentPortalService.Portal.Token;

                return urlBase;
            }
        }

        #endregion PortalGroupUrl        

        #endregion Public Properties

    }
}

