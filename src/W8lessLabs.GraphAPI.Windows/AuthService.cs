using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI.Windows
{
    public class AuthService : IAuthService, IAuthTokenProvider
    {
        private AuthConfig _authConfig;
        private PublicClientApplication _appClient;

        private const string _Authority = "https://login.microsoftonline.com/common";

        public AuthService(AuthConfig authConfig)
        {
            _authConfig = authConfig ?? throw new ArgumentNullException(nameof(authConfig));

            var tokenCacheService = new TokenCacheService("W8lessLabsGraphAPI");
            _appClient = new PublicClientApplication(authConfig.ClientId,
                _Authority,
                tokenCacheService.TokenCache);
        }

        private async Task<IAccount> _GetMicrosoftAccountAsync(string accountId = null) =>
            (string.IsNullOrEmpty(accountId)) ?
                // if account not supplied then try to return first available account
                (await _appClient.GetAccountsAsync().ConfigureAwait(false)).FirstOrDefault() :
                // otherwise lookup the supplied account
                await _appClient.GetAccountAsync(accountId).ConfigureAwait(false);

        public async Task<GraphAccount[]> GetAvailableAccountsAsync()
        {
            var accounts = await _appClient.GetAccountsAsync().ConfigureAwait(false);
            if (accounts is null)
                return Array.Empty<GraphAccount>();
            else
                return accounts.Select(a => a.ToGraphAccount()).ToArray();
        }

        private async Task<GraphAccount> _FindAccount(string accountId) =>
            (await _GetMicrosoftAccountAsync(accountId).ConfigureAwait(false))
                .ToGraphAccount();

        public async Task<(GraphTokenResult result, GraphAccount account)> LoginAsync() => await LoginAsync(null).ConfigureAwait(false);

        public async Task<(GraphTokenResult result, GraphAccount account)> LoginAsync(string accountId)
        {
            var result = await GetTokenAsync(accountId, true);
            var account = await _FindAccount(accountId).ConfigureAwait(false);
            return (result, account);
        }

        public async Task<bool> LogoutAsync()
        {
            var msAccount = await _GetMicrosoftAccountAsync();
            if (msAccount is null)
                return false;
            else
            {
                await _appClient.RemoveAsync(msAccount).ConfigureAwait(false);
                return true;
            }
        }

        public async Task<bool> LogoutAsync(string accountId)
        {
            var msAccount = await _GetMicrosoftAccountAsync(accountId);
            if (msAccount is null)
                return false;
            else
            {
                await _appClient.RemoveAsync(msAccount).ConfigureAwait(false);
                return true;
            }
        }

        public async Task<GraphTokenResult> GetTokenAsync(string accountId = null, bool forceRefresh = false)
        {
            IAccount msAccount = await _GetMicrosoftAccountAsync(accountId);
            
            AuthenticationResult authResult;
            if (msAccount != null)
                authResult = await _appClient.AcquireTokenSilentAsync(_authConfig.Scopes, msAccount, _Authority, forceRefresh).ConfigureAwait(false);
            else
                authResult = await _appClient.AcquireTokenAsync(_authConfig.Scopes).ConfigureAwait(false); // force an interactive login

            if (authResult != null && !string.IsNullOrEmpty(authResult.AccessToken))
            {
                IAccount authAccount = authResult.Account;

                return new GraphTokenResult(true,
                    authResult.AccessToken,
                    authResult.ExpiresOn);
            }

            return GraphTokenResult.Failed;
        }
    }
}
