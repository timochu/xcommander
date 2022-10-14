open System
open System.IO
open XCommander
open XCommander.Path
open XCommander.String
open XCommander.Mod
open Microsoft.Extensions.Configuration
open type Environment

let configuration = (new ConfigurationBuilder()).AddJsonFile("settings.json").AddCommandLine(GetCommandLineArgs()).Build();

module Paths =
    let ModOptionsFile = GetFolderPath SpecialFolder.MyDocuments |> join @"\My Games\XCOM2 War of the Chosen\XComGame\Config\XComModOptions.ini"
    let WorkshopContentFolder = configuration.Item "SteamPath" |> join @"steamapps\workshop\content\268500"

let enabledMods =
    Paths.ModOptionsFile
    |> File.ReadAllLines
    |> Array.where (startsWith "ActiveMods=")
    |> Array.map (replace "ActiveMods=" String.Empty)

let mods = 
    Paths.WorkshopContentFolder
    |> fun dir -> Directory.EnumerateFiles(dir, "*.XComMod", SearchOption.AllDirectories)
    |> Seq.map (toMod >> (fun x -> x.Name, { x with Enabled = enabledMods |> Array.contains x.Name}))
    |> Map.ofSeq

let disableMod m =
    Paths.ModOptionsFile |> File.ReadAllLines
    |> fun lines -> lines |> Array.tryFindIndex (startsWith $"ActiveMods={m.Name}"), lines
    |> function
    | Some index, lines ->
        Array.removeAt index lines |> File.writeAllLines Paths.ModOptionsFile
        Ok $"{m.Title} disabled."
    | _ -> Error $"{m.Title} is already disabled."

let enableMod m =
    match mods |> Map.tryFind m.Name with
    | None -> Error $"No such mod as {m.Title} is downloaded."
    | Some m when m.Enabled -> Error $"{m.Title} is already enabled."
    | _ -> 
        Paths.ModOptionsFile |> File.ReadAllLines
        |> fun lines -> (lines |> Array.tryFindIndexBack (startsWith "ActiveMods=") |> function | Some index -> index + 1 | None -> 1), lines
        |> fun (index, lines) -> 
            lines 
            |>Array.insertAt (index) $"ActiveMods={m}"
            |> File.writeAllLines Paths.ModOptionsFile
            Ok $"{m} activated"

let longestTitleLength  = mods.Values |> Seq.maxBy (fun x -> x.Title.Length) |> fun x -> x.Title.Length

let printMod key value = 
    let isEnabledToBullet (m : Mod) = if m.Enabled then "•" else " "
    printfn " %s %-*s     (%s)" (isEnabledToBullet value) longestTitleLength value.Title key

let printResult r = 
    match r with 
    | Ok msg -> msg 
    | Error msg -> msg
    |> printfn "   %s"

Console.OutputEncoding <- Text.Encoding.UTF8
printfn "XCommander v0.1"
match GetCommandLineArgs() |> Array.tryItem 1, GetCommandLineArgs() |> Array.tryItem 2 with
| None, _ -> printfn "Valid arguments are: \n   activate [{name}|*] \n   disable [{name}|*] \n   list [all|enabled|disabled|downloaded]"
| Some "version", _ -> printfn "version 1.0"

// Enable arguments
| Some "enable", Some "*" -> 
    mods
    |> Map.values 
    |> Seq.where isDisabled 
    |> Seq.map enableMod
    |> Seq.iter printResult

| Some "enable", Some name -> 
    name
    |> mods.TryFind
    |> function
        | Some name -> enableMod name
        | None -> Error $"No such mod as {name} is downloaded."
    |> printResult

// Disable arguments
| Some "disable", Some "*" -> 
    mods 
    |> Map.values 
    |> Seq.where isEnabled
    |> Seq.map disableMod
    |> Seq.iter printResult
| Some "disable", Some name -> 
    name
    |> mods.TryFind
    |> function
        | Some name -> disableMod name
        | None -> Error $"No such mod as {name} is downloaded."
    |> printResult

// List arguments
| Some "list", None -> printfn "Valid list parameters are:\n   all\n   enabled\n   disabled\n   downloaded"
| Some "list", Some "enabled" ->
    printfn "Enabled mods"
    mods.Values 
    |> Seq.where isEnabled
    |> Seq.iter (fun m -> printfn " • %s" m.Title)
| Some "list", Some "disabled" ->
    printfn "Disabled mods"
    mods.Values 
    |> Seq.where isDisabled
    |> Seq.iter (fun m -> printfn " • %s" m.Title)
| Some "list", Some "all" -> 
    printfn "Mods"
    mods |> Map.iter printMod

// Argument missing
| _ -> printfn "No such command exists"
