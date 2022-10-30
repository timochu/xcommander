open System
open System.CommandLine
open xcommander.Commands
open xcommander.Configuration
open xcommander

let root    = RootCommand "XCOM 2 Mod Manager."
let list    = Command("list", "List all mods.")
let enable  = Command("enable", "Enables a specified mod.")
let disable = Command("disable", "Disables a specified mod.")
let run     = Command("run", "Launch XCOM 2.")

module Argument =
    let id        = Argument<string>("id", "Identifier of a mod. You can use '*' in place of <id> to enable/disable all mods.")
    let arguments = Argument<string>("arguments", (fun () -> config.LaunchArguments), "Change default launch arguments.")
    let search    = Argument<string>("search", (fun () -> String.Empty), "Filter the listing to only show mods that match the search.")

module Option =
    let enabled  = Option<bool>("--enabled", "Only show enabled.")
    let disabled = Option<bool>("--disabled", "Only show disabled.")

list.AddArgument Argument.search
list.AddOption Option.enabled
list.AddOption Option.disabled
list.SetHandler (Mod.list, Argument.search, Option.enabled, Option.disabled)

enable.AddArgument Argument.id
enable.SetHandler (Mod.enable, Argument.id)

disable.AddArgument Argument.id
disable.SetHandler (Mod.disable, Argument.id)

run.AddArgument Argument.arguments
run.SetHandler (runXcom, Argument.arguments)

[ list
  run
  enable
  disable ]
|> Seq.iter root.AddCommand

Environment.GetCommandLineArgs() |> Array.skip 1 |> root.Invoke |> ignore
