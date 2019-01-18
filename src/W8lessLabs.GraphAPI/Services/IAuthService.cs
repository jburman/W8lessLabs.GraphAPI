using System;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IAuthService
    {
        Task<GraphAccount[]> GetUserAccountsAsync();
        Task<bool> IsLoggedInAsync(GraphAccount account);
        Task LoginAsync(GraphAccount account);
        Task<bool> LogoutAsync(GraphAccount account);
        Task<(bool success, string idToken, DateTimeOffset tokenExpires)> TryGetTokenAsync(GraphAccount account);
    }
}
