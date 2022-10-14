module XCommander.String

let replace oldValue (newValue : string) (str : string) = str.Replace(oldValue, newValue)

let trim (s : string) = s.Trim()

let startsWith (value : string) (s : string) = s.StartsWith(value)

let contains (value : string) (s : string) = s.Contains(value)