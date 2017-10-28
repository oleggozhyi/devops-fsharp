namespace Devops

open Command
open FileSystem

module Git =
    let git cmd workDir  = 
        let gitCmd = sprintf "git %s" cmd
        executeCommand gitCmd (fun () -> runCmd gitCmd workDir)

open Git
module GitTest = 
    open System

    let test() =
        let workDir = "C:/Work/devops-fsharp"
        workDir |> git "pull"  |> ignore
        workDir |> git "checkout master" |> ignore
        workDir |> git "branch -l"  |> ignore