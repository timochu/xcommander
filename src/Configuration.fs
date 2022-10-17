module xcommander.Configuration
open Microsoft.Extensions.Configuration
open type System.Environment

[<CLIMutable>]
type Configuration = {
    SteamPath : string
    LaunchArguments : string
}

let configurationRoot = (new ConfigurationBuilder()).AddJsonFile("settings.json").AddCommandLine(GetCommandLineArgs()).Build();
let config = configurationRoot.Get<Configuration>()

module Paths =
    open xcommander.Utility.Path
    let ModOptionsFile = GetFolderPath SpecialFolder.MyDocuments |> join @"\My Games\XCOM2 War of the Chosen\XComGame\Config\XComModOptions.ini"
    let WorkshopContentFolder = config.SteamPath |> join @"steamapps\workshop\content\268500"
    let ExecutableFile = config.SteamPath |> join @"steamapps\common\XCOM 2\XCom2-WarOfTheChosen\Binaries\Win64\XCom2.exe"
    let WorkingDirectory = config.SteamPath |> join @"steamapps\common\XCOM 2\XCom2-WarOfTheChosen"
