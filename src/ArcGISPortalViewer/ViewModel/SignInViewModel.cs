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
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Windows.Security.Credentials.UI;

namespace ArcGISPortalViewer.ViewModel
{
    public class SignInViewModel : ViewModelBase
    {
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
            get { return _isSigningIn; }
            set { Set(IsSigningInPropertyName, ref _isSigningIn, value); }
        }

        public bool IsCredentialsPersisted { get; private set; }

        public SignInViewModel()
        {
            Initialize();
            Messenger.Default.Register<ChangeSignInMessage>(this, msg => { var _ = SignInAsync(); });
            Messenger.Default.Register<ChangeSignOutMessage>(this, msg => { var _ = SignOutAsync(); });
        }

        private void Initialize()
        {
            // set IsCredentialsPersisted to false
            IsCredentialsPersisted = false;

            if (App.IsOrgOAuth2) return;

            // Initialize challenge handler to allow storage in the credential locker and restore the credentials
            var defaultChallengeHandler = IdentityManager.Current.ChallengeHandler as DefaultChallengeHandler;
            if (defaultChallengeHandler != null)
            {
                defaultChallengeHandler.AllowSaveCredentials = true;
                defaultChallengeHandler.CredentialSaveOption = CredentialSaveOption.Selected;
                // set it to CredentialSaveOption.Hidden if it's not an user choice                
            }

            IsCredentialsPersisted = IdentityManager.Current.Credentials.Any();
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
                var challengeHandler = IdentityManager.Current.ChallengeHandler;
                // Deactivate the challenge handler temporarily before creating the portal (else challengehandler would be called for portal secured by native)
                IdentityManager.Current.ChallengeHandler = new ChallengeHandler(crd => null);

                ArcGISPortal p = null;
                try
                {
                    p = await ArcGISPortal.CreateAsync(App.PortalUri.Uri);
                }
                catch
                {
                }

                if (p != null && p.ArcGISPortalInfo != null)
                {
                    b = p.ArcGISPortalInfo.Access == PortalAccess.Public;
                }

                // Restore ChallengeHandler
                IdentityManager.Current.ChallengeHandler = challengeHandler;
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
                bool result = await PortalService.CurrentPortalService.SignIn();
                if (result)
                {
                    // navigate to the main page 
                    (new NavigationService()).Navigate(App.MainPageName);

                    // send a message to populate the data
                    Messenger.Default.Send<PopulateDataMessage>(new PopulateDataMessage());

                    IsSigningIn = false;
                    return true;
                }
            }
            catch
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
                ClearAllCredentials();
                // set IsCredentialsPersisted to false
                IsCredentialsPersisted = false;
            }
            catch (Exception ex)
            {
                var _ = App.ShowExceptionDialog(ex);
            }

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

        private void ClearAllCredentials()
        {
            // Remove all credentials (even those for external services, hosted services, federated services) from IM and from the CredentialLocker
            foreach (var crd in IdentityManager.Current.Credentials.ToArray())
                IdentityManager.Current.RemoveCredential(crd);
            var defaultChallengeHandler = IdentityManager.Current.ChallengeHandler as DefaultChallengeHandler;
            if (defaultChallengeHandler != null)
                defaultChallengeHandler.ClearCredentialsCache(); // remove stored credentials
        }
    }
}