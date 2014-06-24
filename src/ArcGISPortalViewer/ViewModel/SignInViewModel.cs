// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using ArcGISPortalViewer.Helpers;
using ArcGISPortalViewer.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Esri.ArcGISRuntime.Portal;

namespace ArcGISPortalViewer.ViewModel
{
    public class SignInViewModel : ViewModelBase
    {
        private string _username = "";
        private string _password = "";

        /// <summary>
        /// The <see cref="IsSigningIn" /> property's name.
        /// </summary>
        public const string IsSigningInPropertyName = "IsSigningIn";

        private bool _isSigningIn = false;

        /// <summary>
        /// Sets and gets the IsSigningIn property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSigningIn
        {
            get
            {
                return _isSigningIn;
            }
            set
            {
                Set(IsSigningInPropertyName, ref _isSigningIn, value);
            }
        }

        public bool SaveCredentials = false;
        public bool IsCredentialsPersisted{ get; private set; }        
      
        public SignInViewModel()
        {
           Initialize();
            Messenger.Default.Register<ChangeSignInMessage>(this, msg => { var _ = SignInAsync(); });            
            Messenger.Default.Register<ChangeSignOutMessage>(this, msg => { var _ = SignOutAsync(); });            
        }

        private void Initialize()
        {
            ResetSignInProperties();
            if (App.IsOrgOAuth2) return;

            try
            { 
                // init credentials from PasswordVault
                var vault = new PasswordVault();
                var cred = vault.FindAllByResource(App.OrganizationUrl) != null ? vault.FindAllByResource(App.OrganizationUrl).FirstOrDefault() : null;
                if (cred != null)
                {
                    IsCredentialsPersisted = true;
                    _username = cred.UserName;
                    cred.RetrievePassword();
                    _password = cred.Password;
                }                 
            }
            catch (Exception)
            {
                ResetSignInProperties();
            }
        }

        public async Task TrySigningInAsync()
        {
            try
            {
                var _ = await SignInAsync();
            }
            catch (Exception ex)
            {
                var _ = App.ShowExceptionDialog(ex);
            }
        }

        public async Task<bool> GetAnonymousAccessStatusAsync()
        {
            bool b = false;
            if (PortalService.CurrentPortalService.Portal != null)
                b = PortalService.CurrentPortalService.Portal.ArcGISPortalInfo.Access == PortalAccess.Public;
            else
            {
                var p = await ArcGISPortal.CreateAsync(App.PortalUri.Uri);
                if (p != null && p.ArcGISPortalInfo != null)
                {
                    b = p.ArcGISPortalInfo.Access == PortalAccess.Public;
                }
            }
            return b;
        }

        public async Task SignInAnonymouslyAsync()
        {
            await PortalService.CurrentPortalService.AttemptAnonymousAccessAsync();
        }

        public async Task<bool> SignInAsync()
        {
            IsSigningIn = true;
            try
            {
                bool result = await PortalService.CurrentPortalService.SignIn(_username, _password);
                if (result)
                {
                    //    if (SaveCredentials)
                    //    {
                    //        //store credentials using PasswordVault
                    //        new PasswordVault().Add(new PasswordCredential(App.OrganizationUrl, _username, _password));
                    //    }

                    // navigate to the main page 
                    (new NavigationService()).Navigate(App.MainPageName);

                    // send a message to populate the data
                    Messenger.Default.Send<PopulateDataMessage>(new PopulateDataMessage());

                    IsSigningIn = false;
                    return true;
                }
            }
            catch (Exception)
            {
            }
            IsSigningIn = false;
            return false;
        }

        public async Task SignOutAsync()
        {
            PortalService.CurrentPortalService.SignOut();

            try
            {
                if (!App.IsOrgOAuth2)
                {
                    // remove credentials from vault
                    var vault = new PasswordVault();
                    PasswordCredential cred = vault.FindAllByResource(App.OrganizationUrl) != null ? vault.FindAllByResource(App.OrganizationUrl).FirstOrDefault() : null;
                    if (cred != null)
                        vault.Remove(cred);
                }
            }
            catch (Exception ex)
            {
                var _ = App.ShowExceptionDialog(ex);
            }

            ResetSignInProperties();

            // if anonymous access is enabled go back to the main page otherwise go back to the signin page
            bool isAnonymousAccess = await GetAnonymousAccessStatusAsync();
            if (isAnonymousAccess)
            {
                (new NavigationService()).Navigate(App.MainPageName);
                await SignInAnonymouslyAsync();
            }
            else
                (new NavigationService()).Navigate(App.BlankPageName);
        }

        private void ResetSignInProperties()
        {
            // set IsCredentialsPersisted to false
            IsCredentialsPersisted = false;

            // reset username and password
            _username = "";
            _password = "";
        }
    }
}
