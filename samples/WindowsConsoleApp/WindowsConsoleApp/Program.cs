using System;
using System.Net.Http;
using System.Threading.Tasks;
using W8lessLabs.GraphAPI;
using W8lessLabs.GraphAPI.Logging;
using W8lessLabs.GraphAPI.Windows;

namespace WindowsConsoleApp
{
    class Program
    {
#pragma warning disable UseAsyncSuffix
        private static async Task Main(string[] args)
#pragma warning restore UseAsyncSuffix
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
                    var loggerProvider = new ConsoleLoggerProvider(); // Can also implement your own ILoggerProvider
                    var json = new JsonSerializer();
                    var authService = new AuthService(authConfig, loggerProvider);
                    var httpService = new HttpService(http, json);
                    var graphService = new GraphService(authService, httpService, json, loggerProvider);

                    // Login the user and retrieve their account info.
                    // The accountId needs to be passed to Graph API calls.
                    // This call will pop a login dialog in Windows if you are not logged in already.
                    // The AuthService will cache the token locally so you should not be prompted when running a second time.
                    // The token is encrypted and stored in a file named "W8lessLabsGraphAPI.msalcache.bin" next to the application exe.
                    (GraphTokenResult result, GraphAccount account) = await authService.LoginAsync();

                    if (!result.Success)
                    {
                        Console.WriteLine("Failed to login and acquire token!");
                        return;
                    }

                    // Get the User's basic profile info.
                    var user = await graphService.GetMeAsync(account.AccountId);

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

                        response = await graphService.GetDriveItemsAsync(account.AccountId, new GetDriveItemsRequest("/", 5, skipToken));

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
                        deltaResponse = await graphService.GetDriveItemsDeltaAsync(account.AccountId);

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
                    //var permissions = await graphService.GetPermissionsAsync(account.AccountId, "/Folder"); // or /Folder/SubFolder/file.foo

                    // or by DriveItemId
                    //var permissions = await graphService.GetPermissionsByIdAsync(account.AccountId, "123456");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
