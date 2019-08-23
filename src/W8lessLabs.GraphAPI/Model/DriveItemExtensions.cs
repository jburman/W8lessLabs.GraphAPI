namespace W8lessLabs.GraphAPI
{
    public static class DriveItemExtensions
    {
        public static bool IsFile(this DriveItem item) => item.Folder == null;
        public static bool IsFolder(this DriveItem item) => item.Folder != null;
    }
}
