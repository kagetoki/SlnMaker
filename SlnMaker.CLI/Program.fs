﻿open SlnMaker
open System

[<EntryPoint>]
let main argv =
    
    MakerCli.performDialog()
    // match result with
    // | Ok sln -> for p in sln.projects do
    //                 printfn "prj: %s" p.name
    // | Error e -> printfn "Failure: %s" e                
    
    Console.ReadLine() |> ignore
    0 // return an integer exit code
