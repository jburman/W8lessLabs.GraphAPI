using System.Collections.Generic;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IGraphService
    {
        Task<GraphUser> GetMeAsync();
        Task<GetDriveItemsResponse> GetDriveItemsAsync(GetDriveItemsRequest request);
        Task<GetDriveItemsDeltaResponse> GetDriveItemsDeltaAsync(string deltaOrNextLink = null);
        string GetDeltaLinkFromToken(string deltaToken);

        Task<int> GetChildItemsCountAsync(string path);
        Task<IEnumerable<Permission>> GetPermissionsAsync(string path);
        Task<IEnumerable<Permission>> GetPermissionsByIdAsync(string itemId);
    }
}