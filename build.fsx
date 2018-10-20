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

let TODO _ : unit = failwith "TODO"

Target.create "Clean" (fun _ ->
  !! "src/**/bin"
  ++ "src/**/obj"
  |> Shell.cleanDirs
)

Target.create "Build" (fun _ ->
  !! "src/**/*.*proj"
  |> Seq.iter (DotNet.build id)
)

Target.create "Run" TODO

Target.create "Bundle" TODO // Would put everything in /deploy

Target.create "Heroku:Container:Push" TODO

Target.create "Heroku:Container:Release" TODO

"Clean"
  ==> "Build"
  ==> "Bundle"
  ==> "Heroku:Container:Push"
  ==> "Heroku:Container:Release"

Target.runOrList ()
