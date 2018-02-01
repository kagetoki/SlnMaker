open System
open SlnMaker

let parseProject str =
    match str with
    | "ref" -> {path="ref";name="name";projectRefs=["lala";"lala1"];content=""} |> Ok
    | "lala" -> {path="lala";name="name";projectRefs=["lala1"];content=""} |> Ok
    | _ -> {path="ok";name="name";projectRefs=[];content=""} |> Ok
let fromOk result = 
    match result with
    | Ok s -> s
    | Error s -> sprintf "not ok: %A" s |> failwith 

[<EntryPoint>]
let main argv =
    let projectPath =  @""
    let result = Maker.generateSln projectPath
    match result with
    | Ok sln -> for p in sln.projects do
                    printfn "prj: %s" p.name
    | Error e -> printfn "Failure: %s" e                
    
    Console.ReadLine() |> ignore
    0 // return an integer exit code
