#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"
#if !FAKE
  #r "netstandard"
  #r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open ASeward.MiscTools
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open System

let appPath = Path.getFullName "./src/HabitsApp"
let migrationPath = Path.getFullName "./src/Migrations"
let deployDir = Path.getFullName "./deploy"

module Util =
  let runDotNet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

  let runTool cmd args workingDir =
    let result =
      Process.execSimple (fun info ->
        { info with
            FileName = cmd
            WorkingDirectory = workingDir
            Arguments = args })
        TimeSpan.MaxValue
    if result <> 0 then failwithf "'%s %s' failed" cmd args

  let openBrowser url =
    let result =
      //https://github.com/dotnet/corefx/issues/10361
      Process.execSimple (fun info ->
        { info with
            FileName = url
            UseShellExecute = true })
        TimeSpan.MaxValue
    if result <> 0 then failwithf "opening browser failed"

open Util

Target.create "Clean" (fun _ ->
  !! "src/**/bin"
  ++ "src/**/obj"
  ++ deployDir
  |> Shell.cleanDirs
)

Target.create "Build" (fun _ ->
  !! "src/**/*.*proj"
  |> Seq.iter (DotNet.build id)
)

Target.create "Run" (fun _ ->
  let server = async {
    runDotNet "watch run" appPath
  }
  let browser = async {
    do! Async.Sleep 5000
    openBrowser "http://localhost:8085"
  }

  [server; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

Target.create "Bundle" (fun _ ->
  let appDeployDir = Path.combine deployDir "HabitsApp"

  runDotNet (sprintf "publish -c Release -o \"%s\"" appDeployDir) appPath
)

Target.create "Bundle:Migrations" (fun _ ->
  let migrationDeployDir = Path.combine deployDir "Migrations"

  runDotNet (sprintf "publish -c Release -o \"%s\"" migrationDeployDir ) migrationPath
)

Target.create "Heroku:Container:Push" (fun _ ->
  runTool "heroku" "container:push web --recursive" "."
)

Target.create "Heroku:Container:Release" (fun _ ->
  let release = async { runTool "heroku" "container:release web" "." }
  let browser = async {
    do! Async.Sleep 5000
    runTool "heroku" "open" "."
  }

  [release; browser]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

"Clean"
  ==> "Build"
  ==> "Bundle"
  ==> "Heroku:Container:Push"
  ==> "Heroku:Container:Release"

"Clean"
  ==> "Build"
  ==> "Bundle:Migrations"

Target.runOrList ()
