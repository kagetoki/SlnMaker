namespace SlnMaker

type public ProjectFile = 
    {
        path:string
        name:string
        projectRefs:string list
        content:string
    }
type public Solution =
    {
        path:string
        projects: ProjectFile Set
    }

type ValidationResult<'Model, 'Message> =
    | Valid of 'Model
    | Invalid of 'Message

type CommandParameter = 
    | Dir of string
    | SlnName of string
    | ProjectPath of string
    | ProjectName of string
    | InvalidParam of string

type Command =
    | EmptyCmd
    | InvalidCmd of CommandParameter
    | Command of CommandParameter list


type CommandContent =
    {
        dir: CommandParameter option
        sln: CommandParameter option
        projectPath: CommandParameter option
        projectName: CommandParameter option
        invalidParam: CommandParameter option
    }