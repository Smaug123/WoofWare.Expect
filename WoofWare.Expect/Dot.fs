namespace WoofWare.Expect

open System
open System.Diagnostics
open System.IO

/// Methods for rendering dot files (specifications of graphs).
[<RequireQualifiedAccess>]
module Dot =
    /// A mock for System.Diagnostics.Process.
    type IProcess<'Process when 'Process :> IDisposable> =
        /// Equivalent to Process.Create
        abstract Create : exe : string -> args : string -> 'Process
        /// Equivalent to Process.Start
        abstract Start : 'Process -> bool
        /// Equivalent to Process.WaitForExit
        abstract WaitForExit : 'Process -> unit
        /// Equivalent to Process.StandardOutput.ReadToEnd
        abstract ReadStandardOutput : 'Process -> string

    /// The real Process interface, in a form that can be passed to `render'`.
    let process' =
        { new IProcess<Process> with
            member _.Create exe args =
                let psi = ProcessStartInfo exe
                psi.RedirectStandardOutput <- true
                psi.Arguments <- args
                let result = new Process ()
                result.StartInfo <- psi
                result

            member _.Start p = p.Start ()
            member _.WaitForExit p = p.WaitForExit ()
            member _.ReadStandardOutput p = p.StandardOutput.ReadToEnd ()
        }

    /// A mock for System.IO
    type IFileSystem =
        /// Equivalent to Path.GetTempFileName
        abstract GetTempFileName : unit -> string
        /// Equivalent to File.Delete
        abstract DeleteFile : string -> unit
        /// Equivalent to File.WriteAllText (curried)
        abstract WriteFile : path : string -> contents : string -> unit

    /// The real filesystem, in a form that can be passed to `render'`.
    let fileSystem =
        { new IFileSystem with
            member _.GetTempFileName () = Path.GetTempFileName ()
            member _.DeleteFile f = File.Delete f
            member _.WriteFile path contents = File.WriteAllText (path, contents)
        }

    /// writeFile takes the filepath first and the contents second.
    /// Due to the impoverished nature of the .NET Standard APIs, you are in charge of making sure the output of
    /// fs.GetTempFileName is suitable for interpolation into a command line.
    let render'<'Process when 'Process :> IDisposable>
        (pr : IProcess<'Process>)
        (fs : IFileSystem)
        (graphEasyExecutable : string)
        (dotFileContents : string)
        : string
        =
        let tempFile = fs.GetTempFileName ()

        try
            fs.WriteFile tempFile dotFileContents

            use p = pr.Create graphEasyExecutable ("--as=boxart --from=dot " + tempFile)
            pr.Start p |> ignore<bool>
            pr.WaitForExit p

            "\n" + pr.ReadStandardOutput p
        finally
            try
                fs.DeleteFile tempFile
            with _ ->
                ()

    /// Call `graph-easy` to render the dotfile as ASCII art.
    /// This is fully mockable, but you must use `render'` to do so.
    let render = render' process' fileSystem "graph-easy"
