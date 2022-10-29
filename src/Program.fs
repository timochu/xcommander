open System
open System.CommandLine
open xcommander.Commands
open xcommander.Configuration

let root       = RootCommand "XCOM 2 Mod Manager."
let list       = Command("list", "List all mods.")
let enable     = Command("enable", "Enables a specified mod.")
let disable    = Command("disable", "Disables a specified mod.")
let enableAll  = Command("enable-all", "Enables all disabled mods.")
let disableAll = Command("disable-all", "Disables all enabled mod.")
let run        = Command("run", "Launch XCOM 2.")

module Argument =
    let name = Argument<string>("name", "Name of a mod.")
    let arguments       = Argument<string>("arguments", (fun () -> config.LaunchArguments), "Change default launch arguments.")
    let search          = Argument<string>("search", (fun () -> String.Empty), "Filter the listing to only show mods that match the search.")

module Option =
    let enabled  = Option<bool>("--enabled", "Only show enabled.")
    let disabled = Option<bool>("--disabled", "Only show disabled.")

list.AddArgument Argument.search
list.AddOption Option.enabled
list.AddOption Option.disabled
list.SetHandler (listMods, Argument.search, Option.enabled, Option.disabled)

enable.AddArgument Argument.name
enable.SetHandler (enableMod, Argument.name)

disable.AddArgument Argument.name
disable.SetHandler (disableMod, Argument.name)

enableAll.SetHandler enableAllMods
disableAll.SetHandler disableAllMods

run.AddArgument Argument.arguments
run.SetHandler (runXcom, Argument.arguments)

[ list
  run
  enableAll
  disableAll
  enable
  disable ]
|> Seq.iter root.AddCommand

Environment.GetCommandLineArgs() |> Array.skip 1 |> root.Invoke |> ignore
