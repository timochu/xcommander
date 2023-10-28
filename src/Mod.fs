module xcommander.Mod
open System
open System.IO
open xcommander.Utility.Regex
open xcommander.Utility.String
open xcommander.Steam
open Configuration
open Utility.File

type Mod = {
    Path : string
    Filename : string
    Id : string
    Id64 : uint64
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
      Id64            = Path.GetFileNameWithoutExtension path |> trim |> uint64
      PublishedFileId = matches |> getCapturedSubstring "publishedFileId" |> trim
      Title           = matches |> getCapturedSubstring "title" |> trim
      Description     = matches |> getCapturedSubstring "description" |> trim
      Category        = matches |> tryGetCapturedSubstring "category" |> Option.map trim
      RequiresXPACK   = matches |> tryGetCapturedSubstring "RequiresXPACK" |> Option.map (trim >> bool.Parse)
      Tags            = matches |> tryGetCapturedSubstring "tags" |> Option.map trim
      ContentImage    = matches |> tryGetCapturedSubstring "contentImage" |> Option.map trim }

let downloadedIds =
    Paths.WorkshopContentFolder
    |> Directory.GetDirectories
    |> Array.map Path.GetFileName
    |> Array.map uint64

let downloaded =
    Paths.WorkshopContentFolder
    |> Directory.GetDirectories
    |> Array.collect Directory.GetFiles
    |> Array.where (endsWith ".XComMod")
    |> Array.map Path.GetFileNameWithoutExtension
    |> Array.sort

let enabled =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> Array.where (startsWith "ActiveMods=")
    |> Array.map (replace "ActiveMods=" String.Empty)
    |> Array.sort

let isEnabled id = enabled |> Array.contains id
let isDisabled = isEnabled >> not

let rec disable id =
    match id, downloaded |> Array.tryFind ((=) id)  with
    | "*", None -> downloaded |> Array.where isEnabled |> Array.iter disable
    | _, None -> printfn $"No such mod as {id} is downloaded."
    | _, Some id when isDisabled id -> printfn $"{id} is already disabled."
    | _, Some id ->
        Paths.ModOptionsFile
        |> File.ReadAllLines
        |> fun lines -> lines, lines |> Array.findIndex (startsWith $"ActiveMods={id}")
        |> fun (lines, index) ->
            lines
            |> Array.removeAt index
            |> writeAllLines Paths.ModOptionsFile
            printfn $"{id} disabled."

let rec enable id =
    match id, downloaded |> Array.tryFind ((=) id) with
    | "*", None -> downloaded |> Array.where isDisabled |> Array.iter enable
    | _, None -> printfn $"No such mod as {id} is downloaded."
    | _, Some id when isEnabled id -> printfn $"{id} is already enabled."
    | _, Some id ->
        Paths.ModOptionsFile
        |> File.ReadAllLines
        |> fun lines -> lines, lines |> Array.tryFindIndexBack (startsWith "ActiveMods=") |> function | Some i -> i + 1 | None -> 1
        |> fun (lines, index) ->
            lines
            |> Array.insertAt index $"ActiveMods={id}"
            |> writeAllLines Paths.ModOptionsFile
            printfn $"{id} enabled."

let list filter enabledOnly disabledOnly =
    downloaded
    |> Array.where (fun id ->
                  match enabledOnly, disabledOnly with
                  | true, false -> isEnabled id
                  | false, true -> isDisabled id
                  | _ -> true)
    |> Array.where (contains filter)
    |> Array.iter (fun id -> printfn "%s %s" (if isEnabled id then "•" else " ") id)



let details () =
    downloadedIds
    |> fetchModDetails |> Async.RunSynchronously
    |> function
        | Ok mods ->
            mods
            |> List.iter (fun m ->
                printfn "MOD: %s\nCreated: %A\nUpdated: %A\nSize: %s\n"
                    m.m_rgchTitle
                    (DateTimeOffset.FromUnixTimeSeconds(int64 m.m_rtimeCreated))
                    (DateTimeOffset.FromUnixTimeSeconds(int64 m.m_rtimeUpdated))
                    (m.m_nFileSize |> Utility.FileSize.humanReadableFileSize)
                )
        | Error error -> failwith error
