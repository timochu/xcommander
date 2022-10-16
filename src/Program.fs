open System
open System.IO
open XCommander.Configuration
open XCommander.Utility.String
open XCommander.Utility.Process
open XCommander.Utility.File
open XCommander.Utility.Result
open XCommander.Mod

open type Environment

let enabledMods =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> Array.where (startsWith "ActiveMods=")
    |> Array.map (replace "ActiveMods=" String.Empty)

let isEnabled m = enabledMods |> Array.contains m.Name
let isDisabled m = isEnabled m |> not

let mods = 
    Paths.WorkshopContentFolder
    |> fun dir -> Directory.EnumerateFiles(dir, "*.XComMod", SearchOption.AllDirectories)
    |> Seq.map loadMod
    |> Seq.map (fun x -> x.Name, x)
    |> Map.ofSeq

let disableMod m =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> fun lines -> lines |> Array.tryFindIndex (startsWith $"ActiveMods={m.Name}"), lines
    |> function
    | Some index, lines ->
        Array.removeAt index lines |> writeAllLines Paths.ModOptionsFile
        Ok $"{m.Title} disabled."
    | _ -> Error $"{m.Title} is already disabled."

let enableMod m =
    match mods |> Map.tryFind m.Name with
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
            Ok $"{m.Title} activated"


Console.OutputEncoding <- Text.Encoding.UTF8
printfn "XCommander v0.1"
match GetCommandLineArgs() |> Array.tryItem 1, GetCommandLineArgs() |> Array.tryItem 2 with
| None, _ -> printfn "Valid arguments are: \n   activate [<name> | *] \n   disable [<name> | *] \n   list [all | enabled | disabled | downloaded]"
| Some "version", _ -> printfn "version 1.0"

// Enable arguments
| Some "enable", Some "*" -> 
    mods
    |> Map.values 
    |> Seq.where isDisabled 
    |> Seq.map enableMod
    |> Seq.iter print

| Some "enable", Some name -> 
    name
    |> mods.TryFind
    |> function
        | Some name -> enableMod name
        | None -> Error $"No such mod as {name} is downloaded."
    |> print

// Disable arguments
| Some "disable", Some "*" -> 
    mods 
    |> Map.values 
    |> Seq.where isEnabled
    |> Seq.map disableMod
    |> Seq.iter print
| Some "disable", Some name -> 
    name
    |> mods.TryFind
    |> function
        | Some name -> disableMod name
        | None -> Error $"No such mod as {name} is downloaded."
    |> print

// List arguments
| Some "list", None -> printfn "Valid list parameters are:\n   all\n   enabled\n   disabled\n   downloaded"
| Some "list", Some "enabled" ->
    printfn "Listing enabled mods..."
    mods.Values 
    |> Seq.where isEnabled
    |> Seq.map getTitle
    |> Seq.iter (printfn " • %s")
| Some "list", Some "disabled" ->
    printfn "Listing disabled mods..."
    mods.Values 
    |> Seq.where isDisabled
    |> Seq.map getTitle
    |> Seq.iter (printfn " • %s")
| Some "list", Some "all" -> 
    printfn "Listing all mods..."
    let longestTitleLength  = mods |> Map.values |> Seq.map (getTitle >> String.length) |> Seq.max
    mods |> Map.iter (fun k m -> printfn " %s %-*s     (%s)" (if isEnabled m then "•" else " ") longestTitleLength m.Title k)

| Some "run", _ -> run Paths.ExecutableFile Paths.WorkingDirectory (configuration.Item "LaunchArguments")

// Argument missing
| _ -> printfn "No such command exists"
