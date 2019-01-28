using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IGraphService
    {
        Task<GraphUser> GetMeAsync(GraphAccount account);
        Task<GetDriveItemsResponse> GetDriveItemsAsync(GraphAccount account, GetDriveItemsRequest request);
        Task<GetDriveItemsDeltaResponse> GetDriveItemsDeltaAsync(GraphAccount account, string deltaOrNextLink = null);
        string GetDeltaLinkFromToken(string deltaToken);

        Task<int> GetChildItemsCountAsync(GraphAccount account, string path);
        Task<IEnumerable<Permission>> GetPermissionsAsync(GraphAccount account, string path);
        Task<IEnumerable<Permission>> GetPermissionsByIdAsync(GraphAccount account, string itemId);

        Task<DriveItem> CreateFolderAsync(GraphAccount account, string path, string newFolderName);
        Task<DriveItem> UploadFileAsync(GraphAccount account, FileUploadRequest request, Stream fileContent);
    }
}