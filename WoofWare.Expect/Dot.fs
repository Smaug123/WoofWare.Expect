namespace WoofWare.Expect

open System.Diagnostics
open System.IO

/// Methods for rendering dot files (specifications of graphs).
[<RequireQualifiedAccess>]
module Dot =

    /// writeFile takes the filepath first and the contents second.
    let render'
        (startProcess : Process -> bool)
        (waitForExit : Process -> unit)
        (getTempFile : unit -> string)
        (deleteFile : string -> unit)
        (writeFile : string -> string -> unit)
        (graphEasyExecutable : string)
        (dotFileContents : string)
        : string
        =
        let tempFile = getTempFile ()

        try
            writeFile tempFile dotFileContents
            let psi = ProcessStartInfo graphEasyExecutable
            psi.Arguments <- "--as=boxart --from=dot " + tempFile
            psi.RedirectStandardOutput <- true

            use p = new Process ()
            p.StartInfo <- psi

            startProcess p |> ignore<bool>

            waitForExit p
            "\n" + p.StandardOutput.ReadToEnd ()
        finally
            try
                deleteFile tempFile
            with _ ->
                ()

    /// Call `graph-easy` to render the dotfile as ASCII art.
    /// This is fully mockable, but you must use `render'` to do so.
    let render =
        render'
            (fun p -> p.Start ())
            (fun p -> p.WaitForExit ())
            Path.GetTempFileName
            File.Delete
            (fun path text -> File.WriteAllText (path, text))
            "graph-easy"
