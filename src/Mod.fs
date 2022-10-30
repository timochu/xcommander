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
    Id : string
    PublishedFileId : string
    Title : string
    Description : string
    Category : string option
    RequiresXPACK : bool option
    Tags : string option
    ContentImage : string option }

let readModInfo path  =
    let expression = @"\r?\npublishedFileId=(?<publishedFileId>[0-9]*)|" +
                     @"\r?\nTitle=(?<title>.*)|" +
                     @"\r?\ntags=(?<tags>.*)|" +
                     @"\r?\ncontentImage=(?<contentImage>.*)|" +
                     @"\r?\nRequiresXPACK=(?<requiresXPACK>.*)|" +
                     @"\r?\nDescription=(?<description>.*(?:\r?\n(?!tags=|contentImage=|Title=|publishedFileId=|RequiresXPACK=).*)*)"
    let matches = path |> File.ReadAllText |> matches expression
    { Path            = path
      Filename        = Path.GetFileName path
      Id              = Path.GetFileNameWithoutExtension path
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
    |> Seq.map Path.GetFileNameWithoutExtension
    |> Seq.sort

let enabled =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> Seq.where (startsWith "ActiveMods=")
    |> Seq.map (replace "ActiveMods=" String.Empty)
    |> Seq.sort

let isEnabled id = enabled |> Seq.contains id
let isDisabled = isEnabled >> not

let rec disable id =
    match id, downloaded |> Seq.tryFind ((=) id)  with
    | "*", None -> downloaded |> Seq.where isEnabled |> Seq.iter disable
    | _, None -> printfn $"No such mod as {id} is downloaded."
    | _, Some id when isDisabled id -> printfn $"{id} is already disabled."
    | _, Some id ->
        Paths.ModOptionsFile
        |> File.ReadAllLines
        |> fun lines -> lines, lines |> Seq.findIndex (startsWith $"ActiveMods={id}")
        |> fun (lines, index) ->
            lines
            |> Seq.removeAt index
            |> writeAllLines Paths.ModOptionsFile
            printfn $"{id} disabled."

let rec enable id =
    match id, downloaded |> Seq.tryFind (fun m -> m = id) with
    | "*", None -> downloaded |> Seq.where isDisabled |> Seq.iter enable
    | _, None -> printfn $"No such mod as {id} is downloaded."
    | _, Some id when isEnabled id -> printfn $"{id} is already enabled."
    | _, Some id ->
        Paths.ModOptionsFile
        |> File.ReadAllLines
        |> fun lines -> lines, lines |> Seq.tryFindIndexBack (startsWith "ActiveMods=") |> function | Some i -> i + 1 | None -> 1
        |> fun (lines, index) ->
            lines
            |> Seq.insertAt index $"ActiveMods={id}"
            |> writeAllLines Paths.ModOptionsFile
            printfn $"{id} enabled."

let list filter enabledOnly disabledOnly =
    downloaded
    |> Seq.where (fun id ->
                  match enabledOnly, disabledOnly with
                  | true, false -> isEnabled id
                  | false, true -> isDisabled id
                  | _ -> true)
    |> Seq.where (contains filter)
    |> Seq.iter (fun id -> printfn "%s %s" (if isEnabled id then "•" else " ") id)
