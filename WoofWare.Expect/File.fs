namespace WoofWare.Expect

open System
open System.IO

[<RequireQualifiedAccess>]
module internal File =

    /// Standard attempt at an atomic file write
    let writeAllLines (lines : string[]) (path : string) : unit =
        let file = FileInfo path
        let tempFile = Path.Combine (file.Directory.FullName, Guid.NewGuid().ToString ())

        try
            File.WriteAllLines (tempFile, lines)
            File.Move (tempFile, path)
        finally
            try
                File.Delete tempFile
            with _ ->
                ()
