using System.Collections.Generic;

namespace W8lessLabs.GraphAPI
{
    public class GetDriveItemsResponse
    {
        public GetDriveItemsResponse(DriveItem[] driveItems, string skipToken)
        {
            DriveItems = new List<DriveItem>(driveItems);
            SkipToken = skipToken;
        }

        public List<DriveItem> DriveItems { get; private set; }
        public string SkipToken { get; private set; }
    }
}
