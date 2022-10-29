module xcommander.Commands
open Utility.Process
open Configuration
open Utility.String

let enableAllMods () =
    Mod.all
    |> Map.values
    |> Seq.where Mod.isDisabled
    |> Seq.iter Mod.enable

let disableAllMods () =
    Mod.all
    |> Map.values
    |> Seq.where Mod.isEnabled
    |> Seq.iter Mod.disable

let enableMod name =
    Mod.all
    |> Map.tryFind name
    |> function
        | Some name -> Mod.enable name
        | None -> printfn $"No such mod as {name} is downloaded."

let disableMod name =
    Mod.all
    |> Map.tryFind name
    |> function
        | Some name -> Mod.disable name
        | None -> printfn $"No such mod as {name} is downloaded."

let listMods filter enabledOnly disabledOnly =
    Mod.all
    |> Map.values
    |> Seq.where (match enabledOnly, disabledOnly with
                  | true, _ -> Mod.isEnabled
                  | _, true -> Mod.isDisabled
                  | _ -> (fun _ -> true))
    |> Seq.where (fun { Name = name } -> contains filter name)
    |> Seq.iter (fun m -> printfn " %s %s" (if Mod.isEnabled m then "•" else " ") m.Title)

let runXcom launchArguments =
    printfn "Launching XCOM 2 with arguments: %s\nExecutable used: %s" launchArguments Paths.ExecutableFile
    run Paths.ExecutableFile Paths.WorkingDirectory launchArguments
