using System;

namespace W8lessLabs.GraphAPI
{
    public class GetDriveItemsRequest
    {
        public const int MinimumPageSize = 10;

        public GetDriveItemsRequest(string path, int pageSize = MinimumPageSize, string skipToken = null, bool allowCache = true, SpecialFolder? specialFolder = null)
        {
            Path = path;
            PageSize = Math.Min(pageSize, pageSize);
            SkipToken = skipToken;
            AllowCache = allowCache;
            SpecialFolder = specialFolder;
        }

        public string Path { get; private set; }
        public int PageSize { get; private set; }
        public string SkipToken { get; private set; }
        public bool AllowCache { get; private set; }
        public SpecialFolder? SpecialFolder { get; private set; }
    }
}
