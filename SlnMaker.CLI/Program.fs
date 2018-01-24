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
let executeAdd sln (prj:ProjectFile) = DotNetCli.executeAdd sln.path prj.path//Ok()

[<EntryPoint>]
let main argv =
    let projectPath =  @"C:\Users\elusi\Source\Repos\bandlab\bandlab-backend\src\BandLab.Activity.Functions\BandLab.Activity.Functions.csproj"
    let slnDir = @"C:\Users\elusi\Source\Repos\bandlab\bandlab-backend\src\"
    let slnName = "BandLab.Activity.Functions1"
    let slnPath = slnDir + slnName + ".sln"                
    match DotNetCli.createSln slnDir slnName with
    | Ok () -> printfn "sln created"
    | Error e -> printf "sln creation failed"
                 printfn "error: %s" e
    let res = Sln.addProjectsRecursive executeAdd FileParser.parseProject slnPath projectPath
    match res with
    | Ok sln -> for p in sln.projects do
                    printfn "prj: %s" p.name
    | Error (err,sln) -> for e in err do
                            printfn "error: %s" e
                         printfn "result sln: %A" sln
    
    Console.ReadLine() |> ignore
    0 // return an integer exit code
