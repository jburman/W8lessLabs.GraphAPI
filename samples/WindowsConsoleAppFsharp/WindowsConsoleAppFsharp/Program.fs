open System.Net.Http
open W8lessLabs.GraphAPI
open W8lessLabs.GraphAPI.Logging
open W8lessLabs.GraphAPI.Windows
open Json

type SkipToken =
    | BeginToken
    | Token of string
    | EndToken

type DeltaLink =
    | BeginLink
    | Link of string
    | EndLink

[<EntryPoint>]
let main argv =
    // register an App at https://apps.dev.microsoft.com/ to get a ClientID (add the Native Application platform)
    let clientId = "... client ID here ..."
    let json = new JsonSerializer() :> IJsonSerializer
    let authConfig = new AuthConfig(clientId, [|
                        "https://graph.microsoft.com/user.read"; // specify desired Graph API permissions
                        "https://graph.microsoft.com/files.read"
                    |]);

    use http = new HttpClient()
    let loggerProvider = new ConsoleLoggerProvider() // Can also implement your own ILoggerProvider
    let authService = new AuthService(authConfig, loggerProvider)
    let httpService = new HttpService(http, json)
    let graphService = new GraphService(authService, httpService, json, loggerProvider)

    // Login the user and retrieve their account info.
    // The accountId needs to be passed to Graph API calls.
    // This call will pop a login dialog in Windows if you are not logged in already.
    // The AuthService will cache the token locally so you should not be prompted when running a second time.
    // The token is encrypted and stored in a file named "W8lessLabsGraphAPI.msalcache.bin" next to the application exe.
    async {
        let! struct (result, account) = authService.LoginAsync() |> Async.AwaitTask
                
        if result.Success <> true then
            printfn "Failed to login and acquire token!"
        else
            // Get the User's basic profile info.
            let! user = graphService.GetMeAsync(account.AccountId) |> Async.AwaitTask

            if user <> null then
                printfn "%s" user.DisplayName
            else
                printfn "User not loaded"

            // Example 1: get items from the root with a PageSize of 5 (to show paging)
            let skipTokenVal token =
                match token with
                | Token(tokenVal) -> tokenVal
                | _ -> null

            let mutable skipToken = BeginToken
            while skipToken <> EndToken do
                printfn "%s" "Fetching page of results..."
                let! response = graphService.GetDriveItemsAsync(account.AccountId, new GetDriveItemsRequest("/", 10, skipTokenVal(skipToken))) |> Async.AwaitTask
                response.DriveItems |> Seq.iter (fun item -> printfn "%s/%s" item.ParentReference.Path item.Name)
                if response.SkipToken <> null then
                        skipToken <- Token(response.SkipToken)
                    else
                        skipToken <- EndToken
        (*
            
        //
        // More verbose approach, but produces a seq of DriveItems
        //

        let skipTokenVal token = 
            match token with
            | Token(token) -> token
            | _ -> null

        let getDriveItems (skipToken: SkipToken) =
            async {
                let! response = graphService.GetDriveItemsAsync(account.AccountId, new GetDriveItemsRequest("/", 10, skipTokenVal(skipToken))) |> Async.AwaitTask
                if response.SkipToken <> null then
                    return (Token(response.SkipToken), response);
                else
                    return (End, response)
            } |> Async.RunSynchronously

        let getAllDriveItems =
            let mutable skipToken = Begin
            seq {
                while skipToken <> End do
                    let (nextSkipToken, response) = getDriveItems skipToken
                    yield response.DriveItems
                    skipToken <- nextSkipToken
            }
        Seq.collect (fun items -> items) getAllDriveItems 
        |> Seq.iter (fun item -> printfn "%s/%s" item.ParentReference.Path item.Name)
        *)

        // Example 2: get "delta" items
        // When the deltaLink argument is null (or not supplied) then it will return all items.
        // Otherwise, it will return any items that have changed since the deltaLink was retrieved.
        let nextLinkVal link =
            match link with
            | Link(linkVal) -> linkVal
            | _ -> null
        
        let mutable nextLink = BeginLink; // used for paging through

        while nextLink <> EndLink do
            let! deltaResponse = graphService.GetDriveItemsDeltaAsync(account.AccountId, nextLinkVal(nextLink)) |> Async.AwaitTask
            printfn "Retrieved delta page. # items: %i" deltaResponse.DriveItems.Count
            deltaResponse.DriveItems |> Seq.iter (fun item -> printfn "%s/%s" item.ParentReference.Path item.Name)

            // Uncomment this line to loop through all results...
            nextLink <- EndLink
            if deltaResponse.NextLink <> null then
                nextLink <- Link(deltaResponse.NextLink)
            else
                nextLink <- EndLink

            match deltaResponse.TryGetDeltaToken() with
            | (true, token) -> 
                printfn "%s" token
                // a delta token can be converted back to a link if desired
                printfn "Delta Link: %s" (graphService.GetDeltaLinkFromToken token)
            | _ -> ()

    } |> Async.RunSynchronously
    0
