namespace SlnMaker
open System.Diagnostics

[<RequireQualifiedAccess>]
module DotNetCli =
    let internal getCmdProcessInfo cmd =
        let pInfo = ProcessStartInfo("cmd.exe", "/C "+cmd)
        //pInfo.FileName <- "cmd.exe"
        pInfo.WindowStyle <- ProcessWindowStyle.Hidden
        pInfo.UseShellExecute <- false
        pInfo.RedirectStandardOutput <- true
        pInfo
    let setCurrentDirCmd dir =
        sprintf "cd %s" dir
    let createSlnCmd dir slnName =
        sprintf "dotnet new sln -o %s -n %s" dir slnName

    let addProjectCmd sln project =
        sprintf "dotnet sln %s add %s" sln project

    let addProjectsCmd sln projects =
        String.concat " " projects 
        |> sprintf "dotnet sln %s add %s" sln

    let executeCmd cmd = 
        try
            use cmdProcess = new Process()
            cmdProcess.StartInfo <- getCmdProcessInfo cmd
            if not <| cmdProcess.Start() then Error "failed to start"
            else
            let output = cmdProcess.StandardOutput.ReadToEnd() //|> System.Console.WriteLine
            System.Console.WriteLine output
            cmdProcess.WaitForExit()
            Ok output
        with
        | Failure m -> Error m
       
    let validateOutput isValid =
        function
        | Error e -> Error e
        | Ok output -> match isValid output with
                        | true -> Ok ()
                        | false -> Error output
    let createSln slnDir slnName =
        let isValid output = Utils.isNullOrEmpty output |> not 
                             && output.Contains("successfully")
        createSlnCmd slnDir slnName
        |> executeCmd
        |> validateOutput isValid
    let executeAdd sln projects =
        let isValid output = Utils.isNullOrEmpty output |> not 
                             && (output.Contains("added to the solution") || output.Contains("already contains"))
        addProjectCmd sln projects
        |> executeCmd
        |> validateOutput isValid
// [<RequireQualifiedAccess>]
// module CompositionRoot =
//     let generateSln rootProjectPath =
//         2