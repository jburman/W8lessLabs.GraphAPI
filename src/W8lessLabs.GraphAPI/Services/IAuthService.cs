using System;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IAuthService
    {
        string GetUserName();
        bool IsLoggedIn();
        void Login();
        void Logout();
        Task<(bool success, string idToken, DateTimeOffset tokenExpires)> TryGetTokenAsync();
    }
}
