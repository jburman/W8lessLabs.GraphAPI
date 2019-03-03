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
    let clientId = "c6181d4f-036c-4004-9faf-fe5c3f3cc358"
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

            // Example 1: get items from the root with a PageSize of 10
            printfn "%s" "=========================\n\
Example 1 - GetDriveItems with PageSize 10\n\
========================="

            let getItems accountId (skipToken: string Option) =
                async { 
                    return! graphService.GetDriveItemsAsync(accountId, 
                        GetDriveItemsRequest("/", 10, (if skipToken.IsSome then skipToken.Value else null))) 
                    |> Async.AwaitTask 
                } 
                |> Async.RunSynchronously
            let rec allItems skipToken =
                seq {
                    printfn "%s" "Fetching page of results..."
                    let response = getItems account.AccountId skipToken
                    yield response.DriveItems
                    if response.SkipToken <> null then
                        yield! allItems (Some(response.SkipToken))
                }
            allItems None |> Seq.collect (fun items -> items) |> Seq.iter (fun item -> printfn "%s/%s" item.ParentReference.Path item.Name)
        
        (*
            // An alternative approach (more imperative style). Just for comparison.

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
        *)

        // Example 2: get "delta" items
        // When the deltaLink argument is null (or not supplied) then it will return all items.
        // Otherwise, it will return any items that have changed since the deltaLink was retrieved.
        printfn "%s" "=========================\n\
Example 2 - GetDriveItemsDelta\n\
========================="

        let getDeltaItems accountId (nextLink: string Option) =
            async { 
                return! graphService.GetDriveItemsDeltaAsync(accountId, (if nextLink.IsSome then nextLink.Value else null)) 
                |> Async.AwaitTask 
            }
            |> Async.RunSynchronously
        let rec allDeltaItems nextLink =
            seq {
                let response = getDeltaItems account.AccountId nextLink
                printfn "Retrieved delta page. # items: %i" response.DriveItems.Count
                yield response.DriveItems
                if response.NextLink <> null then
                    yield! allDeltaItems (Some(response.NextLink))
            }
        allDeltaItems None |> Seq.collect (fun items -> items) |> Seq.iter (fun item -> printfn "%s/%s" item.ParentReference.Path item.Name)


    } |> Async.RunSynchronously
    0
