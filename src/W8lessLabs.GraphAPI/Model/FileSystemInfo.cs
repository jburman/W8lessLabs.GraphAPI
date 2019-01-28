using System;

namespace W8lessLabs.GraphAPI
{
    public class FileSystemInfo
    {
        public DateTimeOffset? CreatedDateTime { get; set; }
        public DateTimeOffset? LastAccessedDateTime { get; set; }
        public DateTimeOffset? LastModifiedDateTime { get; set; }
    }
}
