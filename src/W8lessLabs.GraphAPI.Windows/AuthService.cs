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

        private IUser _GetUser() => _appClient.Users.FirstOrDefault();

        public string GetUserName() => _appClient.Users.FirstOrDefault()?.Name;
        public bool IsLoggedIn() => _GetUser() != null;

        public void Login() =>
            TryGetTokenAsync().GetAwaiter().GetResult();

        public void Logout()
        {
            var user = _GetUser();
            if (user != null)
                _appClient.Remove(user);
        }

        public async Task<(bool success, string idToken, DateTimeOffset tokenExpires)> TryGetTokenAsync()
        {
            var user = _GetUser();
            AuthenticationResult authResult = null;
            if (user != null)
                authResult = await _appClient.AcquireTokenSilentAsync(_authConfig.Scopes, user);
            else
                authResult = await _appClient.AcquireTokenAsync(_authConfig.Scopes);

            if (authResult != null && !string.IsNullOrEmpty(authResult.AccessToken))
                return (true, authResult.AccessToken, authResult.ExpiresOn);
            else
                return (false, null, default);
        }
    }
}
