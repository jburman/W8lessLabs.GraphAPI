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

        private async Task<IAccount> _GetAccountAsync() => (await _appClient.GetAccountsAsync().ConfigureAwait(false)).FirstOrDefault();

        public async Task<string> GetUserNameAsync() => (await _GetAccountAsync().ConfigureAwait(false))?.Username;
        public bool IsLoggedIn() => _GetAccountAsync() != null;

        public async Task LoginAsync() =>
            await TryGetTokenAsync().ConfigureAwait(false);

        public async Task LogoutAsync()
        {
            var account = await _GetAccountAsync().ConfigureAwait(false);
            if (account != null)
                await _appClient.RemoveAsync(account).ConfigureAwait(false);
        }

        public async Task<(bool success, string idToken, DateTimeOffset tokenExpires)> TryGetTokenAsync()
        {
            var account = await _GetAccountAsync();
            AuthenticationResult authResult = null;
            if (account != null)
                authResult = await _appClient.AcquireTokenSilentAsync(_authConfig.Scopes, account).ConfigureAwait(false);
            else
                authResult = await _appClient.AcquireTokenAsync(_authConfig.Scopes).ConfigureAwait(false);

            if (authResult != null && !string.IsNullOrEmpty(authResult.AccessToken))
                return (true, authResult.AccessToken, authResult.ExpiresOn);
            else
                return (false, null, default);
        }
    }
}
