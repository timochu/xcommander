module xcommander.Configuration
open Microsoft.Extensions.Configuration
open type System.Environment

// Without settings UTF8 encoding, special symbols like • won't be displayed in terminal.
System.Console.OutputEncoding <- System.Text.Encoding.UTF8

[<CLIMutable>]
type Configuration = {
    SteamPath : string
    LaunchArguments : string
}

let configurationRoot = ConfigurationBuilder().AddJsonFile("settings.json").AddCommandLine(GetCommandLineArgs()).Build()
let config = configurationRoot.Get<Configuration>()

module Paths =
    open xcommander.Utility.Path
    let ModOptionsFile = GetFolderPath SpecialFolder.MyDocuments |> join @"\My Games\XCOM2 War of the Chosen\XComGame\Config\XComModOptions.ini"
    let WorkshopContentFolder = config.SteamPath |> join @"steamapps\workshop\content\268500"
    let ExecutableFile = config.SteamPath |> join @"steamapps\common\XCOM 2\XCom2-WarOfTheChosen\Binaries\Win64\XCom2.exe"
    let WorkingDirectory = config.SteamPath |> join @"steamapps\common\XCOM 2\XCom2-WarOfTheChosen"
