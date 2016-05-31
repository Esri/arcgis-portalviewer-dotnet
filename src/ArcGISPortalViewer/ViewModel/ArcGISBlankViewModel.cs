// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see https://opensource.org/licenses/ms-pl for details.
// All other rights reserved

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ArcGISPortalViewer.Helpers;
using System;

namespace ArcGISPortalViewer.ViewModel
{
    public class ArcGISBlankViewModel : ViewModelBase
    {
        private RelayCommand _signInCommand;
        public RelayCommand SignInCommand
        {
            get
            {
                return _signInCommand ?? (_signInCommand = new RelayCommand(() =>
                    {
                        Messenger.Default.Send<ChangeSignInMessage>(new ChangeSignInMessage());
                    }
                    ));
            }
        }

        //private RelayCommand _anonymousAccessCommand;
        //public RelayCommand AnonymousAccessCommand
        //{
        //    get
        //    {
        //        return _anonymousAccessCommand ?? (_anonymousAccessCommand = new RelayCommand(() =>
        //            Messenger.Default.Send<ChangeAnonymousAccessMessage>(new ChangeAnonymousAccessMessage())));
        //    }
        //}

        public ArcGISBlankViewModel()
        {            
        }
    }
}
