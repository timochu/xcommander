module xcommander.Commands
open Utility.Result
open Utility.Process
open Configuration

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
    |> Seq.where (fun m -> m.Title.Contains(filter, System.StringComparison.OrdinalIgnoreCase)) 
    |> Seq.iter (fun m -> printfn " %s %s" (if Mod.isEnabled m then "•" else " ") m.Title)

let listEnabled (filter : string) =
    Mod.all
    |> Map.values
    |> Seq.where Mod.isEnabled
    |> Seq.where (fun m -> m.Title.Contains(filter, System.StringComparison.OrdinalIgnoreCase)) 
    |> Seq.map Mod.getTitle
    |> Seq.iter (printfn " • %s")

let listDisabled (filter : string) = 
    Mod.all
    |> Map.values
    |> Seq.where Mod.isDisabled
    |> Seq.where (fun m -> m.Title.Contains(filter, System.StringComparison.OrdinalIgnoreCase)) 
    |> Seq.map Mod.getTitle
    |> Seq.iter (printfn " • %s")

let runXcom () =
    run Paths.ExecutableFile Paths.WorkingDirectory config.LaunchArguments