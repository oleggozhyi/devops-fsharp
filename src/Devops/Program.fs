open Devops
open Devops.Git



// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
[<EntryPoint>]
let main argv = 
    DevopsScripts.createBranche "test" "C:\Work\gitTest" |> ignore

    0 // return an integer exit code
