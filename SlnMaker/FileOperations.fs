namespace SlnMaker
open SlnMaker.Utils
open System
open System.IO

module FileParser =
    open System.Xml.Linq;
    
    let rec internal containsAny (chars: char list) (str:string) =
        match chars with
        | [] -> false
        | h::t -> if str.IndexOf h > 0 then true
                  else containsAny t str
    let internal containsInvalidCharacters = Path.GetInvalidPathChars() 
                                                |> List.ofArray 
                                                |> containsAny 
    let internal isPathValid path = 
        orElse
              {
                  return! isNullOrEmpty path |> not
                  return! containsInvalidCharacters path |> not
                  return! Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute)
              }

    let internal toAbsolutePath slnDir path =
        match isPathValid path with
        | false -> None
        | true -> if Uri.IsWellFormedUriString(path, UriKind.Absolute) then Some path
                  else Path.Combine(slnDir, path.TrimStart('.').TrimStart('\\')) |> Some

    let internal parseReferences projectFileContent =
        let xmlContent = XDocument.Parse projectFileContent
        xmlContent.Descendants <| XName.Get("ProjectReference")
            |> Seq.map (fun x -> let attr = x.Attribute <| XName.Get("Include")
                                 attr.Value)
            |> List.ofSeq
        
    
    let parseProject path =
        let tryParse = tryDo File.ReadAllText
        let tryParseRefs = tryDo parseReferences
        let tryGetName = tryDo Path.GetFileNameWithoutExtension
        let projectFileInfo = FileInfo path
        let slnDir = projectFileInfo.Directory.Parent.FullName
        let parseErrorMsg = sprintf "invalid path: %s" path
        let toAbsolutePath = toAbsolutePath slnDir>>optionToResult parseErrorMsg
        operation
            {
                let! content = tryParse path
                let! references = tryParseRefs content 
                let! references = references |> List.map toAbsolutePath |> listToResult
                let! projectName = tryGetName path
                return { path=path; 
                         content = content; 
                         name = projectName; 
                         projectRefs = references }
            }