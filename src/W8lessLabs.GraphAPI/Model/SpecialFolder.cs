using System;

namespace W8lessLabs.GraphAPI
{
    public enum SpecialFolder
    {
        Documents,
        Photos,
        CameraRoll,
        AppRoot,
        Music
    }

    public static class SpecialFolderExtensions
    {
        public static string ToFolderName(this SpecialFolder folder)
        {
            switch(folder)
            {
                case SpecialFolder.Documents:
                    return "documents";
                case SpecialFolder.Photos:
                    return "photos";
                case SpecialFolder.CameraRoll:
                    return "cameraroll";
                case SpecialFolder.AppRoot:
                    return "approot";
                case SpecialFolder.Music:
                    return "music";
                default:
                    throw new ArgumentOutOfRangeException("Unsupported Special Folder: " + folder, nameof(folder));
            }
        }
    }
}
