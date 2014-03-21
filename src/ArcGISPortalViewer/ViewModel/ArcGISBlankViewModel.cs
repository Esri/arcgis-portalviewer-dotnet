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
