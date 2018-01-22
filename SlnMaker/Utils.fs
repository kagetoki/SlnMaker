namespace SlnMaker
type public ProjectFile = 
    {
        path:string
        name:string
        projectRefs:string list
        content:string
    }
type public  Solution =
    {
        path:string
        projects: Set<ProjectFile>
    }

module Utils =
    type MaybeBuilder() =
        member __.Bind(x,f) =
            match x with
            | None -> None
            | Some x -> f x
        member __.Return x =
            Some x

    type OperationBuilder() =
        member __.Bind(x, f) =
            match x with
            | Error e -> Error e
            | Ok x -> f x
        member __.Return x = Ok x

    type OrElseBuilder() =
        member __.ReturnFrom(x) = x
        member __.Combine (a,b) = 
            match a with
            | true  -> a  
            | false -> b   
        member __.Delay(f) = f()
    let maybe = MaybeBuilder()
    let operation = OperationBuilder()
    let orElse = OrElseBuilder()
    let isNullOrEmpty str =
        match str with
        | null | "" -> true
        | _ -> false

    let ifNotThenNone f x =
        match f x with
        | true -> Some x
        | false -> None     

    let tryDo f x =
        try
            f x |> Ok
        with
        | Failure msg -> Error msg    

    let optionToResult errorMsg x = 
        match x with
        | Some x -> Ok x
        | None -> Error errorMsg    

    let listToResult results =
        let rec loop results acc =
            match results with
            | [] -> Ok acc
            | res::tail -> match res with
                           | Error e -> Error e 
                           | Ok res -> res::acc |> loop tail 
                           
        loop results []    