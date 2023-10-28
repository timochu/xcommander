open System
open System.CommandLine
open xcommander.Commands
open xcommander.Configuration
open xcommander
open xcommander.Steam

let root    = RootCommand "XCOM 2 Mod Manager."
let details = Command("details", "Detailed list of all mods.")
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

details.SetHandler (Mod.details)

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

[ details
  list
  run
  enable
  disable ]
|> Seq.iter root.AddCommand

Environment.GetCommandLineArgs() |> Array.skip 1 |> root.Invoke |> ignore

// let mods = [
//     667104300UL
//     934236622UL
//     1122974240UL
//     1124284135UL
//     1127539414UL
//     1128263618UL
//     1134256495UL
//     1149493976UL
//     1289686596UL
//     1384631824UL
//     2534737016UL
//     2550561145UL
//     2567229533UL
//     2663990965UL]
// let foo = fetchModDetails(mods) |> Async.RunSynchronously
// match foo with
// | Ok details -> details |> List.iter (fun details -> printfn "Mod details: %A" details.m_rgchTitle)
// | Error error -> printfn "%s" error


