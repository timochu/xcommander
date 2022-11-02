module xcommander.Utility
open System

module String =
    let replace oldValue (newValue : string) (str : string) = str.Replace(oldValue, newValue)
    let trim (s : string) = s.Trim()
    let startsWith (value : string) (s : string) = s.StartsWith(value, StringComparison.OrdinalIgnoreCase)
    let endsWith (value : string) (s : string) = s.EndsWith(value, StringComparison.OrdinalIgnoreCase)
    let contains (value : string) (s : string) = s.Contains(value, StringComparison.OrdinalIgnoreCase)

module Process =
    let run filename workingDirectory arguments =
        let p = new Diagnostics.Process(
            StartInfo = new Diagnostics.ProcessStartInfo(
                Arguments = arguments,
                FileName = filename,
                WorkingDirectory = workingDirectory))
        p.Start() |> ignore

module File =
    let writeAllLines path (contents : string seq) = IO.File.WriteAllLines (path, contents)

module Path =
    let join (path : string) root = IO.Path.Join(root, path)

module Regex =
    open System.Text.RegularExpressions

    let matches pattern input : Match seq = Regex.Matches(input, pattern, RegexOptions.IgnoreCase ||| RegexOptions.Multiline)

    let getCapturedSubstring capturingGroupName (matches : Match seq) =
        matches |> Seq.collect (fun m -> m.Groups) |> Seq.find (fun g -> g.Success && g.Name = capturingGroupName) |> fun g -> g.Value

    let tryGetCapturedSubstring capturingGroupName (matches : Match seq) =
        matches |> Seq.collect (fun m -> m.Groups) |> Seq.tryFind (fun g -> g.Success && g.Name = capturingGroupName) |> Option.map (fun g -> g.Value)
