// This file is a copy of the one included in FAKE, with
// use of retry removed, as this causes a NullPointerException
// on Mono.

#I __SOURCE_DIRECTORY__
#r @"../packages/Octokit/lib/net45/Octokit.dll"

open Octokit
open System
open System.IO

type Draft = 
    { Client : GitHubClient
      Owner : string
      Project : string
      DraftRelease : Release }

let private isRunningOnMono = System.Type.GetType ("Mono.Runtime") <> null

let rec private retry count (asyncF: Async<'a>) : Async<'a> =
    if false then //isRunningOnMono then
      asyncF
    else
    try
      async { return! asyncF
        }
    with _ -> retry (count - 1) asyncF
    // async {
    //   let! r = Async.Catch asyncF
    //   match r with
    //   | Choice2Of2 e -> return! retry (count - 1) asyncF
    //   | Choice1Of2 r -> return r
    // }


let createClient user password = 
    async { 
        let github = new GitHubClient(new ProductHeaderValue("FAKE"))
        github.Credentials <- Credentials(user, password)
        return github
    }

let createDraft owner project version prerelease (notes: string seq) (client : Async<GitHubClient>) =     
    async { 
        let data = new NewRelease(version)
        data.Name <- version
        data.Body <- String.Join(Environment.NewLine, notes)
        data.Draft <- true
        data.Prerelease <- prerelease
        let! client' = client
        let! draft = Async.AwaitTask <| client'.Release.Create(owner, project, data)
        printfn "Created draft release id %d" draft.Id
        return { Client = client'
                 Owner = owner
                 Project = project
                 DraftRelease = draft }
    } |> retry 5

let uploadFile fileName (draft : Async<Draft>) = 
    async { 
        let fi = FileInfo(fileName)
        let archiveContents = File.OpenRead(fi.FullName)
        let assetUpload = new ReleaseAssetUpload(fi.Name,"application/octet-stream",archiveContents,Nullable<TimeSpan>())
        let! draft' = draft
        let! asset = Async.AwaitTask <| draft'.Client.Release.UploadAsset(draft'.DraftRelease, assetUpload)
        printfn "Uploaded %s" asset.Name
        return draft'
    } |> retry 5

let releaseDraft (draft : Async<Draft>) = 
    async { 
        let! draft' = draft
        let update = draft'.DraftRelease.ToUpdate()
        update.Draft <- Nullable<bool>(false)
        let! released = Async.AwaitTask <| draft'.Client.Release.Edit(draft'.Owner, draft'.Project, draft'.DraftRelease.Id, update)
        printfn "Released %d on github" released.Id
    } |> retry 5
