namespace SlnMaker

[<AutoOpen>]
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
        member __.ReturnFrom x = x

    let maybe = MaybeBuilder()
    let operation = OperationBuilder()

    let isNullOrEmpty str =
        match str with
        | null | "" -> true
        | _ -> false

    let tryDo f x =
        try
            f x |> Ok
        with
        | Failure msg -> Error msg

    let optionToResult errorMsg x = 
        match x with
        | Some x -> Ok x
        | None -> Error errorMsg

    let combineResults results =
        let combiner (acc) (item:Result<'a,'e>) =
            match item,acc with
            | Error e, Error (oks, errors) -> Error (oks, Set.add e errors)
            | Ok ok, Ok lst -> Set.add ok lst |> Ok
            | Error e, Ok lst -> Error (lst, Set.singleton e) 
            | Ok ok, Error (oks, errors) -> Error(Set.add ok oks, errors)
        Seq.fold combiner <| Ok Set.empty <| results

    let combineSetResults results =
        let combiner acc (result:Result<Set<'a>,Set<'a> *Set<'b>>) =
            match result, acc with
            | Error (ok, e) , Error (oks, errs) -> Error(Set.union ok oks, Set.union errs e)
            | Ok ok, Ok lst -> Set.union ok lst |> Ok
            | Error (ok, e) , Ok lst -> Error(Set.union ok lst, e)
            | Ok ok, Error (oks, errors) -> Error(Set.union ok oks, errors)
        Seq.fold combiner <| Ok Set.empty <| results

    let listToResult results =
        let rec loop results acc =
            match results with
            | [] -> Ok acc
            | res::tail -> match res with
                           | Error e -> Error e 
                           | Ok res -> res::acc |> loop tail

        loop results []

    let (<??>) a b = if isNull a then b else a