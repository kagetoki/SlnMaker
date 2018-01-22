open System
open SlnMaker

let parseProject str =
    match str with
    | "ref" -> {path="ref";name="name";projectRefs=["lala";"lala1"];content=""} |> Ok
    | "lala" -> {path="lala";name="name";projectRefs=["lala1"];content=""} |> Ok
    //| _ -> {path="";name="name";projectRefs=["lala1"];content=""}
    //| _ -> Error "error"
    | _ -> {path="ok";name="name";projectRefs=[];content=""} |> Ok
let fromOk result = 
    match result with
    | Ok s -> s
    | Error s -> sprintf "not ok: %A" s |> failwith 
let executeAdd sln prj = Ok()

[<EntryPoint>]
let main argv =
    let project = FileParser.parseProject @"C:\Users\elusi\Source\Repos\bandlab\bandlab-backend\src\BandLab.Activity.Functions\BandLab.Activity.Functions.csproj"
                    |> fromOk
    let res = Sln.addProjectRecursive executeAdd parseProject {path=""; projects = Set.empty} project
    printfn "sln: %A" res
    // for d in dependencies do
    //     printfn "dep: %A" d
    // for e in errors do
    //     printfn "error: %A" e
    Console.ReadLine() |> ignore
    0 // return an integer exit code
