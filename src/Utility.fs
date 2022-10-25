module xcommander.Utility

module String =
    let replace oldValue (newValue : string) (str : string) = str.Replace(oldValue, newValue)
    let trim (s : string) = s.Trim()
    let startsWith (value : string) (s : string) = s.StartsWith(value)
    let contains (value : string) (s : string) = s.Contains(value, System.StringComparison.InvariantCultureIgnoreCase)

module Process =
    let run filename workingDirectory arguments =
        let p = new System.Diagnostics.Process(
            StartInfo = new System.Diagnostics.ProcessStartInfo(
                Arguments = arguments,
                FileName = filename,
                WorkingDirectory = workingDirectory))
        p.Start() |> ignore

module File =
    let writeAllLines path contents = System.IO.File.WriteAllLines (path, contents)

module Path =
    let join (path : string) root = System.IO.Path.Join(root, path)

module Regex =
    open System.Text.RegularExpressions

    let matches pattern input : Match seq = Regex.Matches(input, pattern, RegexOptions.IgnoreCase ||| RegexOptions.Multiline)

    let getCapturedSubstring capturingGroupName (matches : Match seq) =
        matches |> Seq.collect (fun m -> m.Groups) |> Seq.find (fun g -> g.Success && g.Name = capturingGroupName) |> fun g -> g.Value

    let tryGetCapturedSubstring capturingGroupName (matches : Match seq) =
        matches |> Seq.collect (fun m -> m.Groups) |> Seq.tryFind (fun g -> g.Success && g.Name = capturingGroupName) |> Option.map (fun g -> g.Value)


module Result =
    let print = function Ok msg | Error msg -> msg |> printfn "   %s"
