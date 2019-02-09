using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IGraphService
    {
        // Read calls
        Task<GraphUser> GetMeAsync(GraphAccount account);
        Task<DriveItem> GetDriveItemAsync(GraphAccount account, string itemPath);
        Task<DriveItem> GetDriveItemByIdAsync(GraphAccount account, string driveItemId);
        Task<GetDriveItemsResponse> GetDriveItemsAsync(GraphAccount account, GetDriveItemsRequest request);
        Task<GetDriveItemsDeltaResponse> GetDriveItemsDeltaAsync(GraphAccount account, string deltaOrNextLink = null);
        string GetDeltaLinkFromToken(string deltaToken);
        Task<int> GetChildItemsCountAsync(GraphAccount account, string path);

        // Write calls
        Task<DriveItem> CreateFolderAsync(GraphAccount account, string path, string newFolderName);
        Task<DriveItem> UploadFileAsync(GraphAccount account, FileUploadRequest request, Stream fileContent);
        Task<Stream> DownloadFileAsync(GraphAccount account, string path, (int start, int end) range = default);
        Task<DriveItem> UpdateItemByIdAsync(GraphAccount account, string driveItemId, Dictionary<string, object> updateValues);
        Task<bool> DeleteItemByIdAsync(GraphAccount account, string itemId);

        // Permissions
        Task<Permission> CreateSharingLinkAsync(GraphAccount account, string driveItemId, SharingLinkTypeOptions type, SharingLinkScopeOptions scope);
        Task<PermissionUpdateResponse> AddPermissionAsync(GraphAccount account, string driveItemId, SharingInvitationRequest request);
        Task<PermissionUpdateResponse> UpdatePermissionAsync(GraphAccount account, string driveItemId, string permissionId, params PermissionRoleOptions[] roles);
        Task<IEnumerable<Permission>> GetPermissionsAsync(GraphAccount account, string path);
        Task<IEnumerable<Permission>> GetPermissionsByIdAsync(GraphAccount account, string itemId);
        Task<bool> DeletePermissionByIdAsync(GraphAccount account, string itemId, string permissionId);
    }
}