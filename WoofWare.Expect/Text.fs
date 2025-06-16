namespace WoofWare.Expect

[<RequireQualifiedAccess>]
module internal Text =
    let predent (c : char) (s : string) =
        s.Split '\n' |> Seq.map (sprintf "%c %s" c) |> String.concat "\n"
