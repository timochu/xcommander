module xcommander.Commands
open xcommander.Mod
open Utility.Process
open Utility.String
open Configuration

let enableAllMods () =
    downloaded
    |> Seq.where isDisabled
    |> Seq.iter enable

let disableAllMods () =
    downloaded
    |> Seq.where isEnabled
    |> Seq.iter disable

let enableMod name =
    downloaded
    |> Seq.tryFind (fun { Name = n } -> n = name)
    |> function
        | Some m -> enable m
        | None -> printfn $"No such mod as {name} is downloaded."

let disableMod name =
    downloaded
    |> Seq.tryFind (fun { Name = n } -> n = name)
    |> function
        | Some m -> disable m
        | None -> printfn $"No such mod as {name} is downloaded."

let listMods filter enabledOnly disabledOnly =
    downloaded
    |> Seq.where (match enabledOnly, disabledOnly with
                  | true, _ -> isEnabled
                  | _, true -> isDisabled
                  | _ -> (fun _ -> true))
    |> Seq.where (fun { Name = name ; Title = title } -> contains filter name || contains filter title)
    |> Seq.iter (fun m -> printfn " %s %s" (if isEnabled m then "•" else " ") m.Title)

let runXcom launchArguments =
    printfn "Launching XCOM 2 with arguments: %s\nExecutable used: %s" launchArguments Paths.ExecutableFile
    run Paths.ExecutableFile Paths.WorkingDirectory launchArguments
