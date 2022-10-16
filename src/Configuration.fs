module XCommander.Configuration
open Microsoft.Extensions.Configuration
open type System.Environment

let configuration = (new ConfigurationBuilder()).AddJsonFile("settings.json").AddCommandLine(GetCommandLineArgs()).Build();

module Paths =
    open XCommander.Utility.Path
    let ModOptionsFile = GetFolderPath SpecialFolder.MyDocuments |> join @"\My Games\XCOM2 War of the Chosen\XComGame\Config\XComModOptions.ini"
    let WorkshopContentFolder = configuration.Item "SteamPath" |> join @"steamapps\workshop\content\268500"
    let ExecutableFile = configuration.Item "SteamPath" |> join @"steamapps\common\XCOM 2\XCom2-WarOfTheChosen\Binaries\Win64\XCom2.exe"
    let WorkingDirectory = configuration.Item "SteamPath" |> join @"steamapps\common\XCOM 2\XCom2-WarOfTheChosen"

