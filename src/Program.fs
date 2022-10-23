open System
open System.CommandLine
open xcommander.Commands
open xcommander.Configuration

let rootCommand         = RootCommand "XCOM 2 Mod Manager."
let listCommand         = Command("list", "List all mods.")
let listEnabledCommand  = Command("enabled", "List enabled mods.")
let listDisabledCommand = Command("disabled", "List disabled mods.")
let enableCommand       = Command("enable", "Enables a specified mod.")
let disableCommand      = Command("disable", "Disables a specified mod.")
let runCommand          = Command("run", "Launch XCOM 2.")

let nameArgument        = Argument<string>("name", "Name of mod.")
let launchArguments     = Argument<string>("arguments", (fun () -> config.LaunchArguments), "Change default launch arguments.")
let filterOption        = Option<string>("--filter", (fun () -> String.Empty),"Filter listing by mod name.")

enableCommand.AddArgument nameArgument
disableCommand.AddArgument nameArgument
runCommand.AddArgument launchArguments

listCommand.AddOption filterOption
listEnabledCommand.AddOption filterOption
listDisabledCommand.AddOption filterOption

rootCommand.AddCommand listCommand
rootCommand.AddCommand listEnabledCommand
rootCommand.AddCommand listDisabledCommand
rootCommand.AddCommand runCommand
rootCommand.AddCommand enableCommand
rootCommand.AddCommand disableCommand

listEnabledCommand.SetHandler (listEnabled, filterOption)
listDisabledCommand.SetHandler (listDisabled, filterOption)
listCommand.SetHandler (listAll, filterOption)
runCommand.SetHandler (runXcom, launchArguments)
enableCommand.SetHandler(enableMod, nameArgument)
disableCommand.SetHandler(disableMod, nameArgument)


Environment.GetCommandLineArgs() |> Array.skip 1 |> rootCommand.Invoke |> ignore