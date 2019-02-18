using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI.Windows
{
    public interface IAuthService : IAuthTokenProvider
    {
        Task<GraphAccount[]> GetAvailableAccountsAsync();
        Task<(GraphTokenResult result, GraphAccount account)> LoginAsync();
        Task<(GraphTokenResult result, GraphAccount account)> LoginAsync(string accountId = null);
        Task<bool> LogoutAsync();
        Task<bool> LogoutAsync(string accountId);
    }
}
