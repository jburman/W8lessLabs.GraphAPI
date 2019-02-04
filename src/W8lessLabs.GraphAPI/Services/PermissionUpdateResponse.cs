namespace W8lessLabs.GraphAPI
{
    public class PermissionUpdateResponse
    {
        public PermissionUpdateResponse(bool success, Permission permission, ErrorMessage errorMessage)
        {
            Success = success;
            Permission = permission;
            ErrorMessage = errorMessage;
        }

        public bool Success { get; private set; }
        public Permission Permission { get; private set; }
        public ErrorMessage ErrorMessage { get; set; }
    }
}
