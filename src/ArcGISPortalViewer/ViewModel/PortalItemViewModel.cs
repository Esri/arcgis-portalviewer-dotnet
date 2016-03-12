// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using ArcGISPortalViewer.View;
using Esri.ArcGISRuntime.Portal;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;

namespace ArcGISPortalViewer.ViewModel
{
    public class PortalItemViewModel : ViewModelBase
    {
        #region Constructor 

        public PortalItemViewModel()
        {
            Messenger.Default.Register<ChangeItemSelectedMessage>(this, msg =>
            {
                PortalItem = msg.Item as ArcGISPortalItem;
            });
        }

        #endregion Constructor

        #region Public Properties

        #region PortalItem

        public ArcGISPortalItem PortalItem
        {
            get { return _portalItem; }
            set
            {
                if (_portalItem != value)
                {
                    _portalItem = value;
                    _comments = null;
                    base.RaisePropertyChanged("PortalItem");
                }
            }
        }
        private ArcGISPortalItem _portalItem;

        #endregion ProtalItem

        #region IsDownloadingComments 

        public bool IsDownloadingComments { get; set; }

        #endregion IsDownloadingComments

        #region Comments

        public IEnumerable<ArcGISPortalComment> Comments
        {
            get
            {
                if (_comments != null)
                    return _comments;
                if (PortalItem != null && PortalItem.NumComments > 0 && !IsDownloadingComments)
                {
                    IsDownloadingComments = true;
                    PortalItem.GetCommentsAsync().ContinueWith((e) =>
                    {
                        if (e.IsCompleted)
                            Comments = e.Result ?? new ArcGISPortalComment[0];
                        else if (e.IsFaulted)
                            Comments = new ArcGISPortalComment[0];
                        IsDownloadingComments = false;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                return null;
            }
            private set
            {
                if (!Equals(_comments, value))
                {
                    _comments = value;
                    base.RaisePropertyChanged("Comments");
                }
            }
        }
        private IEnumerable<ArcGISPortalComment> _comments;

        #endregion Comments

        #region FullName

        public string FullName
        {
            get
            {
                return PortalItem.Title;
            }
        }

        #endregion FullName

        #region ProalItemUrl

        public string PortalItemUrl
        {
            get
            {
                string baseUrl = PortalItem.ArcGISPortal.ArcGISPortalInfo.CustomBaseUrl;
                if (string.IsNullOrEmpty(baseUrl))
                    return "";

                string portalUrl = "http://" + baseUrl;
                string urlBase = portalUrl + "/home/item.html?id=" + PortalItem.Id + "&token=" + PortalService.CurrentPortalService.Portal.Token;

                return urlBase;
            }
        }

        #endregion ProtalItemUrl        

        #endregion Public Properties       
    }
}
