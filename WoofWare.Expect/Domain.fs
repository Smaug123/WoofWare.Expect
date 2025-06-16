namespace WoofWare.Expect

open System.Text.Json
open System.Text.Json.Serialization

/// <summary>
/// Information about where in source code a specific snapshot is located.
/// </summary>
type CallerInfo =
    internal
        {
            MemberName : string
            FilePath : string
            LineNumber : int
        }

type private SnapshotValue =
    | BareString of string
    | Json of string

/// The state accumulated by the `expect` builder. You should never find yourself interacting with this type.
type ExpectState<'T> =
    private
        {
            Snapshot : (SnapshotValue * CallerInfo) option
            Actual : 'T option
        }

/// The state accumulated by the `expect` builder. You should never find yourself interacting with this type.
type internal CompletedSnapshotGeneric<'T> =
    private
        {
            SnapshotValue : SnapshotValue
            Caller : CallerInfo
            Actual : 'T
        }

[<RequireQualifiedAccess>]
module internal CompletedSnapshotGeneric =
    let make (state : ExpectState<'T>) : CompletedSnapshotGeneric<'T> =
        match state.Snapshot, state.Actual with
        | Some (snapshot, source), Some actual ->
            {
                SnapshotValue = snapshot
                Caller = source
                Actual = actual
            }
        | None, _ -> failwith "Must specify snapshot"
        | _, None -> failwith "Must specify actual value with 'return'"

    let private jsonOptions =
        let options = JsonFSharpOptions.Default().ToJsonSerializerOptions ()
        options.AllowTrailingCommas <- true
        options.WriteIndented <- true
        options

    let internal replacement (s : CompletedSnapshotGeneric<'T>) =
        match s.SnapshotValue with
        | SnapshotValue.BareString _existing -> s.Actual.ToString ()
        | SnapshotValue.Json _existing ->
            JsonSerializer.Serialize (s.Actual, jsonOptions)
            |> JsonDocument.Parse
            |> _.RootElement
            |> _.ToString()

    /// Returns None if the assertion passes, or Some (expected, actual) if the assertion fails.
    let internal passesAssertion (state : CompletedSnapshotGeneric<'T>) : (string * string) option =
        match state.SnapshotValue with
        | SnapshotValue.Json snapshot ->
            let canonicalSnapshot =
                try
                    JsonDocument.Parse snapshot |> Some
                with _ ->
                    None

            let canonicalActual =
                JsonSerializer.Serialize (state.Actual, jsonOptions) |> JsonDocument.Parse

            match canonicalSnapshot with
            | None -> Some (snapshot, canonicalActual.RootElement.ToString ())
            | Some canonicalSnapshot ->
                if not (JsonElement.DeepEquals (canonicalActual.RootElement, canonicalSnapshot.RootElement)) then
                    Some (canonicalSnapshot.RootElement.ToString (), canonicalActual.RootElement.ToString ())
                else
                    None

        | SnapshotValue.BareString snapshot ->
            let actual = state.Actual.ToString ()

            if actual = snapshot then None else Some (snapshot, actual)

/// Represents a snapshot test that has failed and is awaiting update or report to the user.
type CompletedSnapshot =
    internal
        {
            CallerInfo : CallerInfo
            Replacement : string
        }

[<RequireQualifiedAccess>]
module internal CompletedSnapshot =
    let make (s : CompletedSnapshotGeneric<'T>) =
        {
            CallerInfo = s.Caller
            Replacement = CompletedSnapshotGeneric.replacement s
        }
