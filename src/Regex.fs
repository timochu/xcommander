module XCommander.Regex

open System.Text.RegularExpressions

let regexMatches pattern input : Match seq =
    Regex.Matches(input, pattern, RegexOptions.IgnoreCase ||| RegexOptions.Multiline)

let getRegexMatchCapturedSubstring capturingGroupName (matches : Match seq) =
    matches |> Seq.collect (fun m -> m.Groups) |> Seq.find (fun g -> g.Name = capturingGroupName && g.Success) |> fun g -> g.Value

let getOptionalRegexMatchCapturedSubstring capturingGroupName (matches : Match seq) =
    matches |> Seq.collect (fun m -> m.Groups) |> Seq.tryFind (fun g -> g.Name = capturingGroupName && g.Success) |> Option.map (fun g -> g.Value)