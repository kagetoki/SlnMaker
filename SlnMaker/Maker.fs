namespace SlnMaker
open SlnMaker.Utils

[<RequireQualifiedAccess>]
module public Maker =
    let private wrapSlnResult sln =
        match sln with
        | Ok sln -> Ok sln
        | Error (errs,_ ) -> String.concat System.Environment.NewLine errs |> Error 
    
    let private generateSlnPrivate slnDir slnName projectPath =
        let executeAdd sln (prj:ProjectFile) = DotNetCli.executeAdd sln.path prj.path
        let slnPath = FileOperation.combinePath slnDir slnName
        operation{
            let! slnName = FileOperation.getFileNameWithoutExtension slnName
            do! DotNetCli.createSln slnDir slnName
            return! Sln.addProjectsRecursive executeAdd FileOperation.parseProject slnPath projectPath |> wrapSlnResult
        }

    let private generateSln rootProjectPath =
        operation{
            let! slnDir = FileOperation.getSlnDir rootProjectPath
            let! projectName = FileOperation.getFileNameWithoutExtension rootProjectPath
            let slnName = sprintf "%s\\%s" projectName ".sln"
            return! generateSlnPrivate slnDir slnName rootProjectPath
        }

    let generateSolution (cmd: CommandContent) =
        match cmd with
        | { invalidParam = Some p; } -> sprintf "Invalid parameter %A" p |> Error
        | { projectPath = Some (ProjectPath p); dir = None; sln = None} -> generateSln p
        | { dir = Some (Dir d); sln = Some (SlnName sln); projectName = Some (ProjectName pn)} ->
            let projectDir = FileOperation.getProjectDir pn
            let projPath = FileOperation.combinePath projectDir pn |> FileOperation.combinePath d
            generateSlnPrivate d sln projPath
        | { dir = Some (Dir d); sln = Some (SlnName sln); projectPath = Some (ProjectPath p)} ->
            generateSlnPrivate d sln p
        | unknown -> sprintf "Unknown cmd type %A" unknown |> Error