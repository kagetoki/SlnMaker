namespace SlnMaker
open SlnMaker.Utils

[<RequireQualifiedAccess>]
module public Maker =
    let private wrapSlnResult sln =
        match sln with
        | Ok sln -> Ok sln
        | Error (errs,_ ) -> String.concat System.Environment.NewLine errs |> Error 
    
    let generateSln rootProjectPath =
        let executeAdd sln (prj:ProjectFile) = DotNetCli.executeAdd sln.path prj.path
        operation{
            let! slnDir = FileParser.getSlnDir rootProjectPath
            let! projectName = FileParser.getProjectName rootProjectPath
            let slnPath = sprintf "%s\\%s.sln" slnDir projectName
            do! DotNetCli.createSln slnDir projectName
            return! Sln.addProjectsRecursive executeAdd FileParser.parseProject slnPath rootProjectPath |> wrapSlnResult
        }
        
