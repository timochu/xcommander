module xcommander.Mod
open System
open System.IO
open xcommander.Utility.Regex
open xcommander.Utility.String
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

let getTitle m = m.Title

let enabled =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> Array.where (startsWith "ActiveMods=")
    |> Array.map (replace "ActiveMods=" String.Empty)

let isEnabled m = enabled |> Array.contains m.Name
let isDisabled m = isEnabled m |> not

let all =
    Paths.WorkshopContentFolder
    |> fun dir -> Directory.EnumerateFiles(dir, "*.XComMod", SearchOption.AllDirectories)
    |> Seq.map loadMod
    |> Seq.map (fun x -> x.Name, x)
    |> Map.ofSeq

let disable m =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> fun lines -> lines |> Array.tryFindIndex (startsWith $"ActiveMods={m.Name}"), lines
    |> function
    | Some index, lines ->
        Array.removeAt index lines |> writeAllLines Paths.ModOptionsFile
        Ok $"{m.Title} disabled."
    | _ -> Error $"{m.Title} is already disabled."

let enable m =
    match all |> Map.tryFind m.Name with
    | None -> Error $"No such mod as {m.Title} is downloaded."
    | Some m when isEnabled m -> Error $"{m.Title} is already enabled."
    | _ ->
        Paths.ModOptionsFile
        |> File.ReadAllLines
        |> fun lines -> lines, lines |> Array.tryFindIndexBack (startsWith "ActiveMods=") |> function | Some i -> i + 1 | None -> 1
        |> fun (lines, insertIndex) ->
            lines
            |> Array.insertAt insertIndex $"ActiveMods={m.Name}"
            |> writeAllLines Paths.ModOptionsFile
            Ok $"{m.Title} enabled."
