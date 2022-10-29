module xcommander.Mod
open System
open System.IO
open xcommander.Utility.Regex
open xcommander.Utility.String
open xcommander.Utility.Directory
open Configuration
open Utility.File

type Mod = {
    Path : string
    Filename : string
    Name : string
    PublishedFileId : string
    Title : string
    Description : string
    Category : string option
    RequiresXPACK : bool option
    Tags : string option
    ContentImage : string option }

let loadMod path  =
    let expression = @"\r?\npublishedFileId=(?<publishedFileId>[0-9]*)|" +
                     @"\r?\nTitle=(?<title>.*)|" +
                     @"\r?\ntags=(?<tags>.*)|" +
                     @"\r?\ncontentImage=(?<contentImage>.*)|" +
                     @"\r?\nRequiresXPACK=(?<requiresXPACK>.*)|" +
                     @"\r?\nDescription=(?<description>.*(?:\r?\n(?!tags=|contentImage=|Title=|publishedFileId=|RequiresXPACK=).*)*)"
    let matches = path |> File.ReadAllText |> matches expression
    { Path            = path
      Filename        = Path.GetFileName path
      Name            = Path.GetFileNameWithoutExtension path
      PublishedFileId = matches |> getCapturedSubstring "publishedFileId" |> trim
      Title           = matches |> getCapturedSubstring "title" |> trim
      Description     = matches |> getCapturedSubstring "description" |> trim
      Category        = matches |> tryGetCapturedSubstring "category" |> Option.map trim
      RequiresXPACK   = matches |> tryGetCapturedSubstring "RequiresXPACK" |> Option.map (trim >> bool.Parse)
      Tags            = matches |> tryGetCapturedSubstring "tags" |> Option.map trim
      ContentImage    = matches |> tryGetCapturedSubstring "contentImage" |> Option.map trim }

let downloaded =
    Paths.WorkshopContentFolder
    |> enumerateFiles "*.XComMod" SearchOption.AllDirectories
    |> Seq.map loadMod

let enabled =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> Seq.where (startsWith "ActiveMods=")
    |> Seq.map (replace "ActiveMods=" String.Empty)

let isEnabled { Name = name } = enabled |> Seq.contains name
let isDisabled { Name = name } = enabled |> Seq.contains name |> not

let disable { Title = title ; Name = name } =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> fun lines -> lines |> Seq.tryFindIndex (startsWith $"ActiveMods={name}"), lines
    |> function
    | Some index, lines ->
        Seq.removeAt index lines |> writeAllLines Paths.ModOptionsFile
        printfn $"{title} disabled."
    | _ -> printfn $"{title} is already disabled."

let enable  { Title = title ; Name = name } =
    match downloaded |> Seq.tryFind (fun m -> m.Name = name) with
    | None -> printfn $"No such mod as {title} is downloaded."
    | Some m when isEnabled m -> printfn $"{title} is already enabled."
    | _ ->
        Paths.ModOptionsFile
        |> File.ReadAllLines
        |> fun lines -> lines, lines |> Seq.tryFindIndexBack (startsWith "ActiveMods=") |> function | Some i -> i + 1 | None -> 1
        |> fun (lines, insertIndex) ->
            lines
            |> Seq.insertAt insertIndex $"ActiveMods={name}"
            |> writeAllLines Paths.ModOptionsFile
            printfn $"{title} enabled."
