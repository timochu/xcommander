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

let getTitle { Title = title } = title

let all =
    Paths.WorkshopContentFolder
    |> fun dir -> Directory.EnumerateFiles(dir, "*.XComMod", SearchOption.AllDirectories)
    |> Seq.map loadMod
    |> Seq.map (fun x -> x.Name, x)
    |> Map.ofSeq

let enabled =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> Array.where (startsWith "ActiveMods=")
    |> Array.map (replace "ActiveMods=" String.Empty)

let disabled =
    all
    |> Map.keys
    |> Seq.except enabled
    |> Array.ofSeq

let isEnabled { Name = name } = enabled |> Array.contains name
let isDisabled { Name = name } = disabled |> Array.contains name

let disable { Title = title ; Name = name } =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> fun lines -> lines |> Array.tryFindIndex (startsWith $"ActiveMods={name}"), lines
    |> function
    | Some index, lines ->
        Array.removeAt index lines |> writeAllLines Paths.ModOptionsFile
        Ok $"{title} disabled."
    | _ -> Error $"{title} is already disabled."

let enable  { Title = title ; Name = name } =
    match all |> Map.tryFind name with
    | None -> Error $"No such mod as {title} is downloaded."
    | Some m when isEnabled m -> Error $"{title} is already enabled."
    | _ ->
        Paths.ModOptionsFile
        |> File.ReadAllLines
        |> fun lines -> lines, lines |> Array.tryFindIndexBack (startsWith "ActiveMods=") |> function | Some i -> i + 1 | None -> 1
        |> fun (lines, insertIndex) ->
            lines
            |> Array.insertAt insertIndex $"ActiveMods={name}"
            |> writeAllLines Paths.ModOptionsFile
            Ok $"{title} enabled."
