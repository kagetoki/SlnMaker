namespace SlnMaker
open System.Diagnostics
open Utils
[<RequireQualifiedAccess>]
module DotNetCli =
    let internal getCmdProcessInfo cmd =
        let pInfo = ProcessStartInfo("cmd.exe", "/C "+cmd)
        //pInfo.FileName <- "cmd.exe"
        pInfo.WindowStyle <- ProcessWindowStyle.Hidden
        pInfo.UseShellExecute <- false
        pInfo.RedirectStandardOutput <- true
        pInfo.RedirectStandardError <- true
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
            let output = cmdProcess.StandardOutput.ReadToEnd() 
            let error = cmdProcess.StandardError.ReadToEnd()
            System.Console.WriteLine output
            match cmdProcess.WaitForExit(4000) with
            | true -> Ok output
            | false -> Error <| sprintf "Process exited with timeout %s" (error <??> output)
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
        let created = createSlnCmd slnDir slnName
                        |> executeCmd
                        |> validateOutput isValid
        match created with
        | Error m -> sprintf "Solution creation failed %s" m |> Error
        | ok -> ok
    let executeAdd sln projects =
        let isValid output = Utils.isNullOrEmpty output |> not 
                             && (output.Contains("added to the solution") || output.Contains("already contains"))
        addProjectCmd sln projects
        |> executeCmd
        |> validateOutput isValid
