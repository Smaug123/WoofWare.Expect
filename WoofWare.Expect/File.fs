namespace WoofWare.Expect

open System
open System.IO

[<RequireQualifiedAccess>]
module internal File =

    /// Standard attempt at an atomic file write.
    /// It may fail to be atomic if the working directory somehow spans multiple volumes,
    /// and of course with external network storage all bets are off.
    let writeAllLines (lines : string[]) (path : string) : unit =
        let file = FileInfo path

        let tempFile =
            Path.Combine (file.Directory.FullName, file.Name + "." + Guid.NewGuid().ToString () + ".tmp")

        try
            File.WriteAllLines (tempFile, lines)
            // Atomicity guarantees are undocumented, but on Unix this is an atomic `rename` call
            // https://github.com/dotnet/runtime/blob/9a4be5b56d81aa04c7ea687c02b3f4e64c83761b/src/libraries/System.Private.CoreLib/src/System/IO/FileSystem.Unix.cs#L181
            // and on Windows this is an atomic ReplaceFile:
            // https://github.com/dotnet/runtime/blob/9a4be5b56d81aa04c7ea687c02b3f4e64c83761b/src/libraries/System.Private.CoreLib/src/System/IO/FileSystem.Windows.cs#L92
            // calls https://github.com/dotnet/runtime/blob/9a4be5b56d81aa04c7ea687c02b3f4e64c83761b/src/libraries/Common/src/Interop/Windows/Kernel32/Interop.ReplaceFile.cs#L12
            // which calls ReplaceFileW, whose atomicity guarantees are again apparently undocumented,
            // but 4o-turbo, Opus 4, and Gemini 2.5 Flash all think it's atomic.
            File.Replace (tempFile, path, null)
        finally
            try
                File.Delete tempFile
            with _ ->
                ()
