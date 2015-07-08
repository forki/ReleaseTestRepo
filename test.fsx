#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit
//open System

type Draft = 
    { Client : GitHubClient
      Owner : string
      Project : string
      DraftRelease : Release }

let client = createClient "rneatherway" ""
let draft = createDraft "rneatherway" "ReleaseTestRepo" "1.2" false ["notes"] client
Async.RunSynchronously draft

let github = new GitHubClient(new ProductHeaderValue("FAKE"))
github.Credentials <- Credentials("rneatherway", "")

let data = new NewRelease("1.1")
data.Name <- "1.1"
data.Body <- "Some notes or other"
data.Draft <- true
data.Prerelease <- false
let draftTask = github.Release.Create("rneatherway", "ReleaseTestRepo", data)
let draft = Async.AwaitTask draftTask
let release = Async.RunSynchronously draft

let update = release.ToUpdate()
update.Draft <- Nullable<bool>(false)
let released = github.Release.Edit("rneatherway", "ReleaseTestRepo", release.Id, update)
               |> Async.AwaitTask
               |> Async.RunSynchronously
