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
    |> Seq.map Path.GetFileNameWithoutExtension

let enabled =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> Seq.where (startsWith "ActiveMods=")
    |> Seq.map (replace "ActiveMods=" String.Empty)

let isEnabled name = enabled |> Seq.contains name
let isDisabled = isEnabled >> not

let rec disable (name : string) =
    match name, downloaded |> Seq.tryFind (fun m -> m = name) with
    | "*", None -> downloaded |> Seq.where isEnabled |> Seq.iter disable
    | _, None -> printfn $"No such mod as {name} is downloaded."
    | _, Some name when isDisabled name -> printfn $"{name} is already disabled."
    | _, Some name ->
        Paths.ModOptionsFile
        |> File.ReadAllLines
        |> fun lines -> lines, lines |> Seq.findIndex (startsWith $"ActiveMods={name}")
        |> fun (lines, index) ->
            lines
            |> Seq.removeAt index
            |> writeAllLines Paths.ModOptionsFile
            printfn $"{name} disabled."

let rec enable name =
    match name, downloaded |> Seq.tryFind (fun m -> m = name) with
    | "*", None -> downloaded |> Seq.where isDisabled |> Seq.iter enable
    | _, None -> printfn $"No such mod as {name} is downloaded."
    | _, Some name when isEnabled name -> printfn $"{name} is already enabled."
    | _, Some name ->
        Paths.ModOptionsFile
        |> File.ReadAllLines
        |> fun lines -> lines, lines |> Seq.tryFindIndexBack (startsWith "ActiveMods=") |> function | Some i -> i + 1 | None -> 1
        |> fun (lines, index) ->
            lines
            |> Seq.insertAt index $"ActiveMods={name}"
            |> writeAllLines Paths.ModOptionsFile
            printfn $"{name} enabled."

let list filter enabledOnly disabledOnly =
    downloaded
    |> Seq.where (match enabledOnly, disabledOnly with
                  | true, _ -> isEnabled
                  | _, true -> isDisabled
                  | _ -> (fun _ -> true))
    |> Seq.where (fun name -> contains filter name)
    |> Seq.iter (fun name -> printfn "%s %s" (if isEnabled name then "•" else " ") name)
