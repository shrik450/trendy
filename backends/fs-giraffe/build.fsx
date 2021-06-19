#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.Cli
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open System.Threading

// *** Properties ***
let buildDir = "./bin/"
let appPath = "." |> Path.getFullName
let projectPath = Path.combine appPath "Trendy.fsproj"

// *** Targets ***
Target.create "Clean" (fun _ ->
    Shell.cleanDirs [buildDir]
)

Target.create "Restore" (fun _ ->
    DotNet.restore id projectPath
)

Target.create "Build" (fun _ ->
    DotNet.build id projectPath
)

Target.create "Run" (fun _ ->
  let server = async {
    DotNet.exec (fun p -> { p with WorkingDirectory = appPath } ) "watch" "run" |> ignore
  }

  [ server ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
)

open Fake.Core.TargetOperators

// *** Dependencies ***

"Clean" ==> "Restore" ==> "Build"
"Clean" ==> "Restore" ==> "Run"

Target.runOrDefault "Build"
