module xcommander.Commands
open Utility.Result
open Utility.Process
open Configuration
open Utility.String

let enableMod name =
    Mod.all
    |> Map.tryFind name
    |> function
        | Some name -> Mod.enable name
        | None -> Error $"No such mod as {name} is downloaded."
    |> print

let disableMod name =
    Mod.all
    |> Map.tryFind name
    |> function
        | Some name -> Mod.disable name
        | None -> Error $"No such mod as {name} is downloaded."
    |> print

let enableAll () =
    Mod.all
    |> Map.values
    |> Seq.where Mod.isDisabled
    |> Seq.map Mod.enable
    |> Seq.iter print

let disableAll () =
    Mod.all
    |> Map.values
    |> Seq.where Mod.isEnabled
    |> Seq.map Mod.disable
    |> Seq.iter print

let listAll (filter : string) =
    Mod.all
    |> Map.values
    |> Seq.where (fun m -> contains filter m.Name)
    |> Seq.iter (fun m -> printfn " %s %s" (if Mod.isEnabled m then "•" else " ") m.Title)

let listEnabled (filter : string) =
    Mod.all
    |> Map.values
    |> Seq.where Mod.isEnabled
    |> Seq.map Mod.getTitle
    |> Seq.where (contains filter)
    |> Seq.iter (printfn " • %s")

let listDisabled (filter : string) =
    Mod.all
    |> Map.values
    |> Seq.where Mod.isDisabled
    |> Seq.map Mod.getTitle
    |> Seq.where (contains filter)
    |> Seq.iter (printfn " • %s")

let runXcom launchArguments =
    printfn "Launching XCOM 2 with arguments: %s\nExecutable used: %s" launchArguments Paths.ExecutableFile
    run Paths.ExecutableFile Paths.WorkingDirectory launchArguments
