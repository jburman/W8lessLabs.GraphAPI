using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI.Windows
{
    public class AuthService : IAuthService
    {
        private AuthConfig _authConfig;
        private PublicClientApplication _appClient;

        public AuthService(AuthConfig authConfig)
        {
            _authConfig = authConfig ?? throw new ArgumentNullException(nameof(authConfig));

            var tokenCacheService = new TokenCacheService("W8lessLabsGraphAPI");
            _appClient = new PublicClientApplication(authConfig.ClientId,
                "https://login.microsoftonline.com/common",
                tokenCacheService.TokenCache);
        }

        private async Task<IAccount> _GetMicrosoftAccountAsync(GraphAccount account) =>
            await _appClient.GetAccountAsync(account.AccountId).ConfigureAwait(false);

        public async Task<GraphAccount[]> GetUserAccountsAsync()
        {
            var accounts = await _appClient.GetAccountsAsync().ConfigureAwait(false);
            if (accounts is null)
                return Array.Empty<GraphAccount>();
            else
                return accounts.Select(a => new GraphAccount(
                    accountName: a.Username,
                    identityProvider: a.Environment,
                    accountId: a.HomeAccountId?.Identifier,
                    azureObjectId: a.HomeAccountId?.ObjectId,
                    azureTenantId: a.HomeAccountId?.TenantId)).ToArray();
        }

        public async Task<bool> IsLoggedInAsync(GraphAccount account) =>
            (await _appClient.GetAccountAsync(account.AccountId).ConfigureAwait(false)) != null;

        public async Task<GraphAuthResponse> LoginAsync() => await LoginAsync(null).ConfigureAwait(false);

        public async Task<GraphAuthResponse> LoginAsync(GraphAccount account)
        {
            (bool success, GraphAuthResponse response) = await TryGetTokenAsync(account).ConfigureAwait(false);
            if (success)
                return response;
            else
                return default;
        }

        public async Task<bool> LogoutAsync(GraphAccount account)
        {
            var msAccount = await _GetMicrosoftAccountAsync(account);
            if (msAccount is null)
                return false;
            else
            {
                await _appClient.RemoveAsync(msAccount).ConfigureAwait(false);
                return true;
            }
        }

        public async Task<(bool success, GraphAuthResponse authResponse)> TryGetTokenAsync(GraphAccount account = null)
        {
            IAccount msAccount = null;

            if(account != null)
                msAccount = await _GetMicrosoftAccountAsync(account);
            
            AuthenticationResult authResult = null;
            if (msAccount != null)
                authResult = await _appClient.AcquireTokenSilentAsync(_authConfig.Scopes, msAccount).ConfigureAwait(false);
            else
                authResult = await _appClient.AcquireTokenAsync(_authConfig.Scopes).ConfigureAwait(false); // force an interactive login

            if (authResult != null && !string.IsNullOrEmpty(authResult.AccessToken))
                return (true, new GraphAuthResponse(
                    authResult.AccessToken, 
                    authResult.ExpiresOn, 
                    new GraphAccount(authResult.Account?.HomeAccountId?.Identifier,
                        authResult.Account?.Username,
                        authResult.Account?.Environment,
                        authResult.Account?.HomeAccountId?.ObjectId,
                        authResult.Account?.HomeAccountId?.TenantId)));
            else
                return (false, default);
        }
    }
}
