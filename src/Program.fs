open System
open xcommander.Configuration
open xcommander.Utility.Process
open xcommander.Utility.Result
open xcommander

open type Environment

Console.OutputEncoding <- Text.Encoding.UTF8
match GetCommandLineArgs() |> Array.tryItem 1, GetCommandLineArgs() |> Array.tryItem 2 with
| None, _ -> printfn "Valid arguments are: \n   activate [<name> | *] \n   disable [<name> | *] \n   list [all | enabled | disabled | downloaded]"
| Some "version", _ -> printfn "version 0.1"

// Enable arguments
| Some "enable", Some "*" -> 
    Mod.all
    |> Map.values
    |> Seq.where Mod.isDisabled 
    |> Seq.map Mod.enable
    |> Seq.iter print

| Some "enable", Some name -> 
    Mod.all
    |> Map.tryFind name
    |> function
        | Some name -> Mod.enable name
        | None -> Error $"No such mod as {name} is downloaded."
    |> print

// Disable arguments
| Some "disable", Some "*" -> 
    Mod.all 
    |> Map.values 
    |> Seq.where Mod.isEnabled
    |> Seq.map Mod.disable
    |> Seq.iter print

| Some "disable", Some name -> 
    Mod.all
    |> Map.tryFind name
    |> function
        | Some name -> Mod.disable name
        | None -> Error $"No such mod as {name} is downloaded."
    |> print

// List arguments
| Some "list", None -> printfn "Valid list parameters are:\n   all\n   enabled\n   disabled\n   downloaded"
| Some "list", Some "enabled" ->
    printfn "Listing enabled mods..."
    Mod.all.Values 
    |> Seq.where Mod.isEnabled
    |> Seq.map Mod.getTitle
    |> Seq.iter (printfn " • %s")
| Some "list", Some "disabled" ->
    printfn "Listing disabled mods..."
    Mod.all.Values 
    |> Seq.where Mod.isDisabled
    |> Seq.map Mod.getTitle
    |> Seq.iter (printfn " • %s")
| Some "list", Some "all" -> 
    printfn "Listing all mods..."
    let longestTitleLength  = Mod.all |> Map.values |> Seq.map (Mod.getTitle >> String.length) |> Seq.max
    Mod.all |> Map.iter (fun k m -> printfn " %s %-*s     (%s)" (if Mod.isEnabled m then "•" else " ") longestTitleLength m.Title k)

| Some "run", _ -> run Paths.ExecutableFile Paths.WorkingDirectory config.LaunchArguments

// Argument missing
| _ -> printfn "No such command exists"
