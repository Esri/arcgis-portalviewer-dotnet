// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Windows.Security.Credentials;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace ArcGISPortalViewer.Model
{
    public class PortalService : IPortalService, INotifyPropertyChanged
    {
        private static PortalService _currentPortalService;
        private Credential _credential = null;

        public bool OrganizationResultsOnly = true;

        private string _organizationName = "";
        public string OrganizationName
        {
            get { return _organizationName; }
            set { if (_organizationName != value) { _organizationName = value; NotifyPropertyChanged(); } }
        }

        private string _organizationThumbnail = "";
        public string OrganizationThumbnail
        {
            get { return _organizationThumbnail; }
            set { if (_organizationThumbnail != value) { _organizationThumbnail = value; NotifyPropertyChanged(); } }
        }

        private string _organizationBanner = "";
        public string OrganizationBanner
        {
            get { return _organizationBanner; }
            set { if (_organizationBanner != value) { _organizationBanner = value; NotifyPropertyChanged(); } }
        }

        private string _userName = "";
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        private string _password = "";
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private bool _isSigningIn = false;
        public bool IsSigningIn
        {
            get { return _isSigningIn; }
            set { if (_isSigningIn != value) { _isSigningIn = value; NotifyPropertyChanged(); } }
        }

        public ArcGISPortal Portal { get; private set; }
        public ArcGISPortalUser CurrentUser { get; private set; }
        public bool IsAnonymousUser
        {
            get
            {
                if (CurrentPortalService.CurrentUser == null || string.IsNullOrEmpty(CurrentPortalService.CurrentUser.UserName))
                    return true;
                else
                    return false;
            }
        }

        public static PortalService CurrentPortalService
        {
            get
            {
                if (_currentPortalService == null)
                    _currentPortalService = new PortalService();
                return _currentPortalService;
            }
        }

        public PortalService()
        {
            _currentPortalService = this;
        }

        public async Task AttemptAnonymousAccessAsync()
        {
            var challengeHandler = IdentityManager.Current.ChallengeHandler;
            // Deactivate the challenge handler temporarily before creating the portal (else challengehandler would be called for portal secured by native)
            IdentityManager.Current.ChallengeHandler = new ChallengeHandler(crd => null);

            try
            {
                var p = await ArcGISPortal.CreateAsync(App.PortalUri.Uri);
                if (p != null)
                {
                    //set the ArcGISPortal
                    Portal = p;
                    SetOrganizationProperties();
                    GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<ArcGISPortalViewer.Helpers.ChangedPortalServiceMessage>(new ArcGISPortalViewer.Helpers.ChangedPortalServiceMessage());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                // Restore ChallengeHandler
                IdentityManager.Current.ChallengeHandler = challengeHandler;
            }
        }

        public async Task<IList<ArcGISPortalGroup>> GetGroups()
        {
            ArcGISPortalUser portalUser = this.CurrentUser;
            if (portalUser == null)
            {
                await Task.Delay(10000);
                portalUser = this.CurrentUser;
                if (portalUser == null)
                {
                    var r = await Task.FromResult<IList<ArcGISPortalGroup>>(null);
                    return r;
                }
            }

            IEnumerable<ArcGISPortalGroup> groups = await this.CurrentUser.GetGroupsAsync();
            IList<ArcGISPortalGroup> results = new List<ArcGISPortalGroup>(groups);

            return results;
        }

        public async Task<SearchResultInfo<ArcGISPortalItem>> GetSearchResults(SearchParameters searchParameters)
        {
            if (searchParameters == null || string.IsNullOrEmpty(searchParameters.QueryString))
                return null;

            if (CurrentPortalService.Portal == null)
                return null;

            string accountId = CurrentPortalService.Portal.ArcGISPortalInfo == null ? "" : CurrentPortalService.Portal.ArcGISPortalInfo.Id;
            if (!string.IsNullOrEmpty(accountId) && OrganizationResultsOnly) //!this.Portal.ArcGISPortalInfo.CanSearchPublic)
            {
                string queryString = string.Format("({0}) AND accountid:{1}", searchParameters.QueryString, accountId);
                searchParameters = new SearchParameters(queryString)
                {
                    Limit = searchParameters.Limit,
                    SortField = searchParameters.SortField,
                    SortOrder = searchParameters.SortOrder,
                    StartIndex = searchParameters.StartIndex
                    //QueryString = string.Format("({0}) AND accountid:{1}", searchParameters.QueryString, accountId)                    
                };
            }

            try
            {
                return await this.Portal.SearchItemsAsync(searchParameters);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IList<ArcGISPortalItem>> GetPortalItems(SearchParameters searchParameters)
        {
            return await GetPortalItemsInternal(ItemsRequestedType.Default, searchParameters);
        }

        public async Task<IList<ArcGISPortalItem>> GetMyMapsAsync(SearchParameters searchParameters)
        {

            return await GetPortalItemsInternal(ItemsRequestedType.Default, searchParameters);
        }

        public async Task<IList<ArcGISPortalItem>> GetBasemaps(SearchParameters searchParameters)
        {
            return await GetPortalItemsInternal(ItemsRequestedType.Basemaps, searchParameters);
        }

        public async Task<IList<ArcGISPortalItem>> GetFeaturedItems(SearchParameters searchParameters)
        {
            return await GetPortalItemsInternal(ItemsRequestedType.Featured, searchParameters);
        }

        private enum ItemsRequestedType
        {
            Default = 0, // defined by the search parameters
            Featured = 1,
            Basemaps = 2
        }

        private async Task<IList<ArcGISPortalItem>> GetPortalItemsInternal(
            ItemsRequestedType itemsRequestedType = ItemsRequestedType.Default,
            SearchParameters searchParameters = null)
        {
            IList<ArcGISPortalItem> results = new List<ArcGISPortalItem>();
            if (CurrentPortalService.Portal == null || CurrentPortalService.Portal.ArcGISPortalInfo == null)
                return results;

            SearchResultInfo<ArcGISPortalItem> items = null;
            try
            {
                switch (itemsRequestedType)
                {
                    case ItemsRequestedType.Default:
                        if (searchParameters != null)
                            items = await GetSearchResults(searchParameters);
                        break;
                    case ItemsRequestedType.Featured:
                        items = await CurrentPortalService.Portal.ArcGISPortalInfo.SearchFeaturedItemsAsync(searchParameters);
                        break;
                    case ItemsRequestedType.Basemaps:
                        items = await CurrentPortalService.Portal.ArcGISPortalInfo.SearchBasemapGalleryAsync(searchParameters);
                        break;
                }
                if (items != null)
                {
                    foreach (ArcGISPortalItem item in items.Results)
                        results.Add(item);
                }
                return results;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SignOut()
        {
            IsSigningIn = true;

            try
            {
                ResetIdentityManager();

                //// reset it to anonymous access if possible                 
                //CurrentPortalService.Portal = await ArcGISPortal.CreateAsync(App.PortalUri.Uri);

                CurrentPortalService.Portal = null;
                CurrentPortalService.OrganizationName = CurrentPortalService.Portal != null ? CurrentPortalService.Portal.ArcGISPortalInfo.Name : "";
                CurrentPortalService.UserName = "";
                CurrentPortalService.Password = "";
                CurrentPortalService.CurrentUser = null;
                CurrentPortalService.OrganizationThumbnail = "";
                CurrentPortalService.OrganizationBanner = "";
            }
            catch { }

            //await InitializePortal();            
            IsSigningIn = false;
        }

        public void ResetIdentityManager()
        {
            var im = IdentityManager.Current;
            //im.ChallengeMethod = null;
            foreach (var crd in im.Credentials)
                im.RemoveCredential(crd);
        }

        public async Task<bool> SignIn()
        {
            IsSigningIn = true;
            try
            {
                bool b = await SignInUsingIdentityManager();
                if (b)
                {
                    Portal = await ArcGISPortal.CreateAsync(App.PortalUri.Uri); //, CancellationToken.None, ((TokenCredential)_credential).Token);
                    if (Portal != null)
                        CurrentUser = await ArcGISPortalUser.CreateAsync(Portal, ((TokenCredential)_credential).UserName);

                    SetOrganizationProperties();
                    IsSigningIn = false;
                    return true;
                }
            }
            catch (Exception ex)
            {
                IsSigningIn = false;
                var _ = App.ShowExceptionDialog(ex);
            }

            IsSigningIn = false;
            return false;
        }

        private void SetOrganizationProperties()
        {
            OrganizationName = !string.IsNullOrEmpty(Portal.ArcGISPortalInfo.Name) ? Portal.ArcGISPortalInfo.Name : Portal.ArcGISPortalInfo.PortalName;
            OrganizationThumbnail = Portal.ArcGISPortalInfo.ThumbnailUri != null ? Portal.ArcGISPortalInfo.ThumbnailUri.AbsoluteUri : "";
            // Need to expose banner id and url on ArcGISPortalInfo - for now I am hardcoding banner-2.jpg.
            OrganizationBanner = "http://portalhost.esri.com/gis/home/images/banner-5.jpg";
            //OrganizationBanner = "https://arcgis.esri.com/gis/home/images/banner-5.jpg";
        }

        public async Task<bool> SignInUsingIdentityManager()
        {
            IsSigningIn = true;

            // if oauth2 required params are set, register the server for oauth2 authentication.            
            if (App.IsOrgOAuth2)
            {
                var serverInfo = new ServerInfo()
                {
                    ServerUri = App.PortalUri.Uri.ToString(),
                    TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode,
                    OAuthClientInfo = new OAuthClientInfo() { ClientId = App.AppServerId, RedirectUri = App.AppRedirectUri }
                };
                IdentityManager.Current.RegisterServer(serverInfo);
            }
            // check if a TokenCredential can be retrieved without challenging the user
            else if (IdentityManager.Current.Credentials.Any())
            {
                try
                {
                    var credential = IdentityManager.Current.Credentials.ElementAt(0) as TokenCredential;
                    if (credential != null && !string.IsNullOrEmpty(credential.Token))
                    {
                        // set the credential 
                        _credential = credential;
                        IsSigningIn = false;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    IsSigningIn = false;
                    var _ = App.ShowExceptionDialog(ex);
                    return false;
                }
            }

            // Since a credential could not be retrieved, try getting it by challenging the user
            var credentialRequestInfo = new CredentialRequestInfo
            {
                ServiceUri = App.PortalUri.Uri.ToString(),
                AuthenticationType = AuthenticationType.Token,
            };

            try
            {
                var credential = await IdentityManager.Current.GetCredentialAsync(credentialRequestInfo, true);
                if (credential != null)
                {
                    //set the credential 
                    _credential = credential;
                    IdentityManager.Current.AddCredential(_credential);
                    IsSigningIn = false;
                    return true;
                }
            }
            catch (Exception ex)
            {
                IsSigningIn = false;
            }

            IsSigningIn = false;
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

