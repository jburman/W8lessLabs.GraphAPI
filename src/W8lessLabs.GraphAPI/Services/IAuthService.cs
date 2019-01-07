using System;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IAuthService
    {
        Task<string> GetUserNameAsync();
        bool IsLoggedIn();
        Task LoginAsync();
        Task LogoutAsync();
        Task<(bool success, string idToken, DateTimeOffset tokenExpires)> TryGetTokenAsync();
    }
}
