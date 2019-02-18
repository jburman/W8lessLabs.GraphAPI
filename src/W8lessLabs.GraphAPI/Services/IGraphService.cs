using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IGraphService
    {
        // Read calls
        Task<GraphUser> GetMeAsync(string accountId);
        Task<DriveItem> GetDriveItemAsync(string accountId, string itemPath);
        Task<DriveItem> GetDriveItemByIdAsync(string accountId, string driveItemId);
        Task<GetDriveItemsResponse> GetDriveItemsAsync(string accountId, GetDriveItemsRequest request);
        Task<GetDriveItemsDeltaResponse> GetDriveItemsDeltaAsync(string accountId, string deltaOrNextLink = null);
        string GetDeltaLinkFromToken(string deltaToken);
        Task<int> GetChildItemsCountAsync(string accountId, string path);

        // Write calls
        Task<DriveItem> CreateFolderAsync(string accountId, string path, string newFolderName);
        Task<DriveItem> UploadFileAsync(string accountId, FileUploadRequest request, Stream fileContent);
        Task<Stream> DownloadFileAsync(string accountId, string path, (int start, int end) range = default);
        Task<DriveItem> UpdateItemByIdAsync(string accountId, string driveItemId, Dictionary<string, object> updateValues);
        Task<bool> DeleteItemByIdAsync(string accountId, string itemId);

        // Permissions
        Task<Permission> CreateSharingLinkAsync(string accountId, string driveItemId, SharingLinkTypeOptions type, SharingLinkScopeOptions scope);
        Task<PermissionUpdateResponse> AddPermissionAsync(string accountId, string driveItemId, SharingInvitationRequest request);
        Task<PermissionUpdateResponse> UpdatePermissionAsync(string accountId, string driveItemId, string permissionId, params PermissionRoleOptions[] roles);
        Task<IEnumerable<Permission>> GetPermissionsAsync(string accountId, string path);
        Task<IEnumerable<Permission>> GetPermissionsByIdAsync(string accountId, string itemId);
        Task<bool> DeletePermissionByIdAsync(string accountId, string itemId, string permissionId);
    }
}