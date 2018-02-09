namespace SlnMaker
open System

[<RequireQualifiedAccess>]
module MakerCli =

    [<Literal>]
    let private HELP = @"Usage: 
    -p <project path> 
    or
    -d <sln directory> -s <sln name> -p <project path> 
    or
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

    let private emptyCommandContent = { dir = None; projectName = None; sln = None; projectPath = None; invalidParam = None }

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

    let private validateCommand cmd =
        match cmd with
        | {dir = Some (Dir _); sln = Some (SlnName _); projectPath = Some (ProjectPath _); projectName = None; invalidParam = None}
        | {dir = Some (Dir _); sln = Some (SlnName _); projectPath = None; projectName = Some (ProjectName _); invalidParam = None}
        | {dir = Some (Dir _); sln = Some (SlnName _); projectPath = Some (ProjectPath _); projectName = None; invalidParam = None}
        | {dir = None; sln = None; projectPath = Some (ProjectPath _); projectName = None; invalidParam = None } -> Valid cmd
        | _ -> Invalid HELP

    let private buildCmdContent prms =
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

    let printWithColor color format data =
        let oldColor = Console.ForegroundColor
        Console.ForegroundColor <- color
        printfn format data
        Console.ForegroundColor <- oldColor

    let printfErr f d = printWithColor ConsoleColor.Red f d
    let printfInfo f d = printWithColor ConsoleColor.DarkGreen f d

    let printfText f d = printWithColor ConsoleColor.Cyan f d

    let printErr a = printfErr "%A" a
    let printInfo a = printfInfo "%A" a
    let printText a = printfText "%A" a

    let rec ifUserWantsToContinue() =
        printText "Are you willing to continue? [y/n]"
        match Console.ReadLine() with
        | "y" | "Y" -> true
        | "n" | "N" -> false
        | _ -> printText "Please press 'y' or 'n'"
               ifUserWantsToContinue()

    let private sayFairwell() =
        printText "Press any key to continue..."
        Console.ReadKey() |> ignore

    let executeCommand onFail onSuccess cmdText =
        match parseCommand cmdText with
        | EmptyCmd -> onFail()
        | InvalidCmd p -> printfErr "Invalid parameter %A" p
                          onFail()
        | Command prms -> match buildCmdContent prms with
                          | Error e -> printfErr "Invalid parameter %A" e
                                       onFail()
                          | Ok cmd -> match validateCommand cmd with
                                      | Invalid _ -> printErr "Invalid command"
                                                     onFail()
                                      | Valid cmd -> let result = Maker.generateSolution cmd
                                                     match result with
                                                     | Ok sln -> printInfo "Solution created successfully!"
                                                                 printInfo sln.path
                                                                 onSuccess()
                                                     | Error e -> printfErr "Errors occured: %s" e
                                                                  onSuccess()

    let onSuccess again () =
        if ifUserWantsToContinue() then again()
        else sayFairwell()

    let rec performDialog() =
        Console.ForegroundColor <- ConsoleColor.Green
        printText HELP
        let text = Console.ReadLine().Trim().Split(" ")
        if text.Length > 0 && text.[0] = ":q" then ()
        else
        executeCommand performDialog (onSuccess performDialog) text
