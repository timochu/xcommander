module xcommander.Commands
open Utility.Process
open Utility.String
open Configuration

let enableAllMods () =
    Mod.all
    |> Seq.where Mod.isDisabled
    |> Seq.iter Mod.enable

let disableAllMods () =
    Mod.all
    |> Seq.where Mod.isEnabled
    |> Seq.iter Mod.disable

let enableMod name =
    Mod.all
    |> Seq.tryFind (fun m -> m.Name = name)
    |> function
        | Some m -> Mod.enable m
        | None -> printfn $"No such mod as {name} is downloaded."

let disableMod name =
    Mod.all
    |> Seq.tryFind (fun m -> m.Name = name)
    |> function
        | Some m -> Mod.disable m
        | None -> printfn $"No such mod as {name} is downloaded."

let listMods filter enabledOnly disabledOnly =
    Mod.all
    |> Seq.where (match enabledOnly, disabledOnly with
                  | true, _ -> Mod.isEnabled
                  | _, true -> Mod.isDisabled
                  | _ -> (fun _ -> true))
    |> Seq.where (fun { Name = name ; Title = title } -> contains filter name || contains filter title)
    |> Seq.iter (fun m -> printfn " %s %s" (if Mod.isEnabled m then "•" else " ") m.Title)

let runXcom launchArguments =
    printfn "Launching XCOM 2 with arguments: %s\nExecutable used: %s" launchArguments Paths.ExecutableFile
    run Paths.ExecutableFile Paths.WorkingDirectory launchArguments
