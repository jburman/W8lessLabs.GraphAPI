using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IAuthTokenProvider
    {
        Task<GraphTokenResult> GetTokenAsync(string accountId = null, bool forceRefresh = false);
    }
}
