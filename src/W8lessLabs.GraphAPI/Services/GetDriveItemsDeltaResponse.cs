using System.Collections.Generic;

namespace W8lessLabs.GraphAPI
{
    public class GetDriveItemsDeltaResponse
    {
        public GetDriveItemsDeltaResponse(DriveItem[] driveItems, string nextLink, string deltaLink)
        {
            DriveItems = new List<DriveItem>(driveItems);
            NextLink = nextLink;
            DeltaLink = deltaLink;
        }

        public List<DriveItem> DriveItems { get; private set; }
        public string NextLink { get; private set; }
        public string DeltaLink { get; private set; }

        public bool TryGetDeltaToken(out string deltaToken)
        {
            deltaToken = null;
            string deltaLink = DeltaLink;

            if (deltaLink != null)
            {
                int index = deltaLink.IndexOf(GraphService.DeltaTokenParam);

                if (index != -1)
                {
                    string token = deltaLink.Substring(index + GraphService.DeltaTokenParam.Length);
                    
                    // check for any trailing parameters and remove them
                    index = token.IndexOf('&');
                    if (index != -1)
                        token = token.Substring(0, index);

                    deltaToken = token;
                }
            }
            return deltaToken != null;
        }
    }
}
