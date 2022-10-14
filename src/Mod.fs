module XCommander.Mod
open System.IO
open XCommander.Regex
open XCommander.String

type Mod = {
    Enabled : bool
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

let isEnabled (m : Mod) = m.Enabled
let isDisabled (m : Mod) = not m.Enabled

let toMod (modFilePath : string)  =
    let expression = @"\r?\npublishedFileId=(?<publishedFileId>[0-9]*)|" +
                     @"\r?\nTitle=(?<title>.*)|" +
                     @"\r?\ntags=(?<tags>.*)|" +
                     @"\r?\ncontentImage=(?<contentImage>.*)|" +
                     @"\r?\nRequiresXPACK=(?<requiresXPACK>.*)|" +
                     @"\r?\nDescription=(?<description>.*(?:\r?\n(?!tags=|contentImage=|Title=|publishedFileId=|RequiresXPACK=).*)*)"
    let matches = modFilePath |> File.ReadAllText |> regexMatches expression
    { Enabled         = false
      Path            = modFilePath
      Filename        = Path.GetFileName modFilePath
      Name            = Path.GetFileNameWithoutExtension modFilePath
      PublishedFileId = matches |> getRegexMatchCapturedSubstring "publishedFileId" |> trim
      Title           = matches |> getRegexMatchCapturedSubstring "title" |> trim
      Description     = matches |> getRegexMatchCapturedSubstring "description" |> trim
      Category        = matches |> getOptionalRegexMatchCapturedSubstring "category" |> Option.map trim
      RequiresXPACK   = matches |> getOptionalRegexMatchCapturedSubstring "RequiresXPACK" |> Option.map (trim >> bool.Parse)
      Tags            = matches |> getOptionalRegexMatchCapturedSubstring "tags" |> Option.map trim
      ContentImage    = matches |> getOptionalRegexMatchCapturedSubstring "contentImage" |> Option.map trim }
      

let toString modification =
    sprintf "Title: %s \nPath: %s \nPublishedFileId: %s \nDescription: %s \nRequiresXPACK: %A \nTags: %A \nContentImage: %A \nCategory: %A \n"
        modification.Title 
        modification.Path 
        modification.PublishedFileId 
        modification.Description 
        modification.RequiresXPACK 
        modification.Tags 
        modification.ContentImage 
        modification.Category