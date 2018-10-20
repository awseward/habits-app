#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
  #r "netstandard"
  #r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open System

let TODO _ : unit = failwith "TODO"

let appPath = Path.getFullName "./src/HabitsApp"
let deployDir = Path.getFullName "./deploy"

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

Target.create "Run" TODO

Target.create "Bundle" (fun _ ->
  let appDeployDir = Path.combine deployDir "HabitsApp"

  runDotNet (sprintf "publish -c Release -o \"%s\"" appDeployDir) appPath
)

Target.create "Heroku:Container:Push" (fun _ ->
  runTool "heroku" "container:push web --recursive" "."
)

Target.create "Heroku:Container:Release" (fun _ ->
  runTool "heroku" "container:release web" "."
  runTool "heroku" "open" "."
)

"Clean"
  ==> "Build"
  ==> "Bundle"
  ==> "Heroku:Container:Push"
  ==> "Heroku:Container:Release"

Target.runOrList ()
