using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IAuthService
    {
        Task<GraphAccount[]> GetUserAccountsAsync();
        Task<GraphAuthResponse> LoginAsync();
        Task<GraphAuthResponse> LoginAsync(GraphAccount account);
        Task<bool> LogoutAsync(GraphAccount account);
        Task<(bool success, GraphAuthResponse authResponse)> TryGetTokenAsync(GraphAccount account);
    }
}
