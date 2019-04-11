module Graph

open W8lessLabs.GraphAPI

type GraphFolders(graphService: IGraphService, accountId: string) =
    
    let pageSize = 100

    let getItems path accountId (skipToken: string Option) =
        async { 
            return! graphService.GetDriveItemsAsync(accountId, 
                GetDriveItemsRequest(path, pageSize, (if skipToken.IsSome then skipToken.Value else null))) 
            |> Async.AwaitTask 
        } 
        |> Async.RunSynchronously

    let getItem path =
        async {
            return! graphService.GetDriveItemAsync(accountId, path)
            |> Async.AwaitTask
        } |> Async.RunSynchronously

    let rec allItemsInFolder path skipToken =
        seq {
            let response = getItems path accountId skipToken
            yield response.DriveItems
            if response.SkipToken <> null then
                yield! allItemsInFolder path (Some(response.SkipToken))
        }

    member this.GetFolder path =
        new GraphFolder(this, getItem path, path)

    member this.GetFolders path =
        let parent = 
            match path with
            | Folder(path) -> path
            | Path(path) -> this.GetFolder path

        allItemsInFolder parent.Path None
        |> Seq.collect (fun items -> items)
        |> Seq.where (fun item -> item.IsFolder())
        |> Seq.map (fun item -> new GraphFolder(this, item, parent.Path + "/" + item.Name))
        |> List.ofSeq

    member this.GetFiles path =
        let parent = 
            match path with
            | Folder(path) -> path
            | Path(path) -> this.GetFolder path

        allItemsInFolder parent.Path None
        |> Seq.collect (fun items -> items)
        |> Seq.where (fun item -> item.IsFile())
        |> Seq.map (fun item -> new GraphFile(parent, item))
        |> List.ofSeq

and GraphFolder(graphFolders: GraphFolders, driveItem: DriveItem, path: string) =
    let mutable files: GraphFile list option = None
    let mutable folders: GraphFolder list option = None

    let trimPathRoot (drivePath: string) =
        let index = drivePath.IndexOf(':')
        match index with
        | -1 -> drivePath
        | _ -> drivePath.Substring(index + 1)

    member this.Path =
        if driveItem.ParentReference.Path = null then
            "/"
        else
            trimPathRoot(driveItem.ParentReference.Path) + "/" + driveItem.Name

    member this.Files =
        match files with
        | Some(cachedFiles) -> cachedFiles
        | None ->
            files <- Some(graphFolders.GetFiles (Folder this))
            files.Value
    member this.Folders =
        match folders with
        | Some(cachedFolders) -> cachedFolders
        | None ->
            folders <- Some(graphFolders.GetFolders (Folder this))
            folders.Value

and GraphFile(folder: GraphFolder, driveItem: DriveItem) =
    member this.Name =
        driveItem.Name
    member this.Size =
        driveItem.Size

and FileParent =
| Path of string
| Folder of GraphFolder
