// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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

        public ArcGISBlankViewModel()
        {
        }
    }
}
