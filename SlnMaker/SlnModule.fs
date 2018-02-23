namespace SlnMaker


[<RequireQualifiedAccess>]
module Sln =
    let internal createSln slnPath projects =
        {path = slnPath; projects = projects  }
    let internal addProjectInternal sln prj =
        { sln with projects = Set.add prj sln.projects }
        
    let parseDependenciesRecursive (parseProject: string->Result<ProjectFile,string>) projectPath =
        let rec parseDependencies parseProject acc path  =
            match parseProject path with
            | Error e -> acc |> Set.add (Error e) 
            | Ok project -> let acc = Set.add <| Ok project <| acc
                            let accs = Seq.collect (parseDependencies parseProject acc) project.projectRefs
                                        |> Set.ofSeq
                            Set.union acc accs
        parseDependencies parseProject Set.empty projectPath
        |> combineResults                        
    
    let addProject executeAdd sln project =
        match executeAdd sln project with
        | Ok() -> addProjectInternal sln project |> Ok
        | Error msg -> Error (msg, sln)
    
    let addProjectsRecursive (executeAdd: Solution -> ProjectFile-> Result<unit,string>) 
                              parseProject slnPath projectPath =
        let createSln = createSln slnPath
        let executeAdd sln prj = 
                match addProject executeAdd sln prj with
                | Error (msg, sln) -> Error(Set.singleton msg,sln)
                | Ok sln -> Ok sln
        let rec add projects (sln:Result<Solution,(string Set*Solution)>) =
            match sln, projects with
            | sln, [] -> sln
            | Ok sln, prj::projects -> executeAdd sln prj |> add projects
            | Error (e, sln), prj::projects -> match (executeAdd sln prj |> add projects) with
                                                   | Ok sln -> Error(e, sln)
                                                   | Error (e1, sln) -> Error(Set.union e1 e, sln)
        result {
            let! sln = match parseDependenciesRecursive parseProject projectPath with
                                | Ok dependencies -> createSln dependencies |> Ok
                                | Error (dependencies,errors) -> Error(errors, createSln dependencies)
            
            return! add <| List.ofSeq sln.projects <| Ok (createSln Set.empty)
        }
   