open SlnMaker

[<EntryPoint>]
let main argv =
    let dontCare () = ()
    if argv.Length > 0 then
        MakerCli.executeCommand dontCare dontCare argv
    else
        MakerCli.performDialog()
    0 // return an integer exit code
