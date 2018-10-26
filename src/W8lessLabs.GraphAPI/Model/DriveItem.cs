using System;

namespace W8lessLabs.GraphAPI
{
    public class DriveItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateTimeOffset LastModifiedDateTime { get; set; }
        public string ETag { get; set; }
        public string WebUrl { get; set; }
        public File File { get; set; }
        public Folder Folder { get; set; }
        public ItemReference ParentReference { get; set; }
        public Shared Shared { get; set; }

        public bool IsFile => File != null;
        public bool IsFolder => Folder != null;
    }
}
