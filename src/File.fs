module XCommander.File
open System.IO

let writeAllLines path contents = File.WriteAllLines (path, contents)