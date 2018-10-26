using System;
using System.Net.Http;
using W8lessLabs.GraphAPI;
using W8lessLabs.GraphAPI.Windows;

namespace WindowsConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // register an App at https://apps.dev.microsoft.com/ to get a ClientID (add the Native Application platform)
            const string ClientId = "... client ID here ..."; 

            try
            {
                var authConfig = new AuthConfig(ClientId,
                    new[] {
                        "https://graph.microsoft.com/user.read", // specify desired Graph API permissions
                        "https://graph.microsoft.com/files.read"
                    });

                // Provide an Http Handler
                using (var http = new HttpClient())
                {
                    var authService = new AuthService(authConfig);
                    var httpService = new HttpService(http, new JsonSerializer());
                    var graphService = new GraphService(authService, httpService);

                    // Get the User's basic profile info - this will pop a login dialog in Windows if you are not logged in already.
                    // The AuthService will cache the token locally so you should not be prompted when running a second time.
                    // The token is encrypted and stored in a file named "W8lessLabsGraphAPI.msalcache.bin" next to the application exe.
                    var user = graphService.GetMeAsync().GetAwaiter().GetResult();

                    if (user != null)
                        Console.WriteLine(user.DisplayName);
                    else
                        Console.WriteLine("User not loaded");

                    // Example 1: get items from the root with a PageSize of 5 (to show paging)
                    GetDriveItemsResponse response;
                    string skipToken = null; // this is used to page through the results

                    do
                    {
                        Console.WriteLine("Fetching page of results...");

                        response = graphService.GetDriveItemsAsync(new GetDriveItemsRequest("/", 5, skipToken))
                            .GetAwaiter().GetResult();

                        foreach (var item in response.DriveItems)
                            Console.WriteLine($"    {item.ParentReference.Path}/{item.Name}");

                        skipToken = response.SkipToken;

                    } while (response.SkipToken != null);

                    // Example 2: get "delta" items
                    // When the deltaLink argument is null (or not supplied) then it will return all items.
                    // Otherwise, it will return any items that have changed since the deltaLink was retrieved.
                    string deltaLink = null;
                    string nextLink = null; // used for paging through
                    GetDriveItemsDeltaResponse deltaResponse;

                    do
                    {
                        deltaResponse = graphService.GetDriveItemsDeltaAsync().GetAwaiter().GetResult();

                        Console.WriteLine("Retrieved delta page. # items: " + deltaResponse.DriveItems.Count);
                        // Uncomment this line to loop through all results...
                        //nextLink = deltaResponse.NextLink;

                        deltaLink = deltaResponse.DeltaLink; // this is only returned on the final page of results

                        // another way to check if delta token is available
                        if(deltaResponse.TryGetDeltaToken(out string deltaToken))
                        {
                            Console.WriteLine("Delta Token: " + deltaToken);

                            // a delta token can be converted back to a link if desired
                            deltaLink = graphService.GetDeltaLinkFromToken(deltaToken);
                        }
                    } while (nextLink != null);

                    // Example 3: get permissions for a drive item

                    // By path/name
                    //var permissions = await graphService.GetPermissionsAsync("/Folder"); // or /Folder/SubFolder/file.foo

                    // or by DriveItemId
                    //var permissions = await graphService.GetPermissionsByIdAsync("123456");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
