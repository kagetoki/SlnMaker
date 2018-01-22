namespace SlnMaker


[<RequireQualifiedAccess>]
module Sln =
    let internal addProjectInternal sln prj =
        { sln with projects = Set.add prj sln.projects }
    let getDependensiesRecursive parseProject project =
        let parseRefs = List.map parseProject
        let rec getDependensies parsingResults acc parsingErrors =
            match parsingResults with 
            | [] -> acc, parsingErrors
            | [parsingResult] -> 
                                match parsingResult with
                                 | Ok proj -> getDependensies (parseRefs proj.projectRefs) 
                                                <| Set.add proj acc 
                                                <| parsingErrors
                                 | Error e -> acc,Set.add e parsingErrors 
            | headResult::tailResults -> 
                                match headResult with
                                 | Ok proj -> let acc,parsingErrors = 
                                                getDependensies (parseRefs proj.projectRefs) 
                                                <| Set.add proj acc 
                                                <| parsingErrors
                                              getDependensies tailResults acc parsingErrors
                                 | Error e -> getDependensies tailResults acc <| Set.add e parsingErrors
        
        getDependensies (parseRefs project.projectRefs) <| Set.add project Set.empty <| Set.empty

    let addProject executeAdd sln project =
        match executeAdd sln project with
        | Ok() -> addProjectInternal sln project |> Ok
        | Error msg -> Error (msg, sln)

    let addProjectRecursive executeAdd parseProject sln project =
        let dependentProject,parsingErrors = getDependensiesRecursive parseProject project
        let executeAdd sln prj =
            match sln with
            | Error e -> Error e
            | Ok sln -> addProject executeAdd sln prj
        let sln = Ok sln        
        match dependentProject,parsingErrors with
        | dependencies, errors when errors.Count > 0 -> 
                    let res = Set.fold executeAdd sln dependencies
                    match res with
                    | Ok sln -> Error (errors, sln)
                    | Error (parsingErrors,sln) -> Error (Set.union errors parsingErrors, sln) 
        | dependencies, _ -> Set.fold executeAdd sln dependencies                             
        