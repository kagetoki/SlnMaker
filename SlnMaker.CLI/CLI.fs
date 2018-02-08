namespace SlnMaker
open System

module CLI =

    [<Literal>]
    let private HELP = @"Usage: 
-p <project path> or
-d <sln directory> -s <sln name> -p <project path> or
-d <sln directory> -s <sln name> -pn <project name>

When project name is specified, it's assumed, that project is stored to its folder with same name.
It's also assumed that by default project folder is located in solution folder.

For exit print :q"

    [<Literal>]
    let private DIR = "-d"
    [<Literal>]
    let private SLN = "-s"
    [<Literal>]
    let private PROJECT_NAME = "-pn"
    [<Literal>]
    let private PROJECT_PATH = "-p"

    let emptyCommandContent = { dir = None; projectName = None; sln = None; projectPath = None; invalidParam = None }

    let private parseCommand (args: string []) =
        let rec parseCmdRec index command =
            match command with
            | InvalidCmd _ as invalid -> invalid
            | EmptyCmd -> EmptyCmd
            | Command prms as cmd -> 
                if index = args.Length - 1 then InvalidParam args.[index] |> InvalidCmd
                elif index > args.Length - 1 then cmd
                else 
                let payload = args.[index + 1]
                let param = match args.[index] with
                            | DIR -> Dir payload
                            | SLN -> SlnName payload 
                            | PROJECT_PATH -> ProjectPath payload
                            | PROJECT_NAME -> ProjectName payload 
                            | p -> InvalidParam p
                
                match param with
                | InvalidParam _ as p -> InvalidCmd p
                | param -> param::prms |> Command |> parseCmdRec (index + 2)                     
        match args with
        | null | [||] -> EmptyCmd
        | args when args.Length < 2 -> InvalidCmd <| InvalidParam args.[0]
        | _ -> Command [] |> parseCmdRec 0 

    let validateCommand cmd =
        match cmd with
        | {dir = Some (Dir _); sln = Some (SlnName _); projectPath = Some (ProjectPath _); projectName = None; invalidParam = None}
        | {dir = Some (Dir _); sln = Some (SlnName _); projectPath = None; projectName = Some (ProjectName _); invalidParam = None}
        | {dir = Some (Dir _); sln = Some (SlnName _); projectPath = Some (ProjectPath _); projectName = None; invalidParam = None}
        | {dir = None; sln = None; projectPath = Some (ProjectPath _); projectName = None; invalidParam = None } -> Valid cmd
        | _ -> Invalid HELP

    let buildCmdContent prms =
        let update cmd prm =
            match cmd with
            | Error e -> Error e
            | Ok cmd -> 
                match prm with
                | InvalidParam _ as p -> Error p
                | Dir _ as dir -> if cmd.dir = None then Ok {cmd with dir = Some dir} else dir |> Error
                | SlnName _ as sln -> if cmd.sln = None then Ok {cmd with sln = Some sln} else sln |> Error
                | ProjectPath _ as p -> if cmd.projectPath = None then Ok {cmd with projectPath = Some p} else p |> Error
                | ProjectName _ as p -> if cmd.projectName = None then Ok {cmd with projectName = Some p} else p |> Error

        List.fold update (Ok emptyCommandContent) prms

    let rec openDialog() =
        printfn "%s" HELP
        let text = Console.ReadLine().Split(" ")
        if text.Length > 0 && text.[0] = ":q" then ()
        else
        match parseCommand text with
        | EmptyCmd -> openDialog()
        | InvalidCmd p -> printfn "Invalid parameter %A" p
                          openDialog()
        | Command prms -> match buildCmdContent prms with
                          | Error e -> printfn "Invalid parameter %A" e
                                       openDialog()
                          | Ok cmd -> match validateCommand cmd with
                                      | Invalid _ -> Console.WriteLine "Invalid command"
                                                     openDialog()
                                      | Valid cmd -> let result = Maker.generateSolution cmd
                                                     match result with
                                                     | Ok sln -> printfn "Solution created successfully!"
                                                                 printfn "%A" sln.path
                                                     | Error e -> printfn "Errors occured: %s" e