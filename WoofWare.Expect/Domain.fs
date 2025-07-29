namespace WoofWare.Expect

open System.Collections
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
    | Json of expected : string
    | Formatted of expected : string
    | ThrowsException of expected : string

type private CompletedSnapshotValue<'T> =
    | Json of expected : string * JsonSerializerOptions * JsonDocumentOptions
    | List of expected : string list * format : ((unit -> 'T seq) -> string)
    | Formatted of expected : string * format : ((unit -> 'T) -> string)

/// The state accumulated by the `expect` builder. You should never find yourself interacting with this type.
type ExpectState<'T> =
    private
        {
            Formatter : ((unit -> 'T) -> string) option
            JsonSerialiserOptions : JsonSerializerOptions option
            JsonDocOptions : JsonDocumentOptions option
            Snapshot : (SnapshotValue * CallerInfo) option
            Actual : (unit -> 'T) option
        }

type ExpectStateListy<'T> =
    private
        {
            Formatter : ((unit -> 'T) -> string) option
            Snapshot : (string list * CallerInfo) option
            Actual : (unit -> 'T seq) option
        }

/// The state accumulated by the `expect` builder. You should never find yourself interacting with this type.
type internal CompletedSnapshotGeneric<'T> =
    private
        {
            SnapshotValue : CompletedSnapshotValue<'T>
            Caller : CallerInfo
            Actual : unit -> 'T
        }

[<RequireQualifiedAccess>]
module internal CompletedSnapshotGeneric =
    let private defaultJsonSerialiserOptions : JsonSerializerOptions =
        let options = JsonFSharpOptions.Default().ToJsonSerializerOptions ()
        options.AllowTrailingCommas <- true
        options.WriteIndented <- true
        options

    let private defaultJsonDocOptions : JsonDocumentOptions =
        let options = JsonDocumentOptions (AllowTrailingCommas = true)
        options

    let make (state : ExpectState<'T>) : CompletedSnapshotGeneric<'T> =
        match state.Snapshot, state.Actual with
        | Some (snapshot, source), Some actual ->
            let snapshot =
                match snapshot with
                | SnapshotValue.Json expected ->
                    let serOpts =
                        state.JsonSerialiserOptions |> Option.defaultValue defaultJsonSerialiserOptions

                    let docOpts = state.JsonDocOptions |> Option.defaultValue defaultJsonDocOptions
                    CompletedSnapshotValue.Json (expected, serOpts, docOpts)
                | SnapshotValue.Formatted expected ->
                    let formatter =
                        match state.Formatter with
                        | None -> fun x -> x().ToString ()
                        | Some f -> f

                    CompletedSnapshotValue.Formatted (expected, formatter)

                | SnapshotValue.ThrowsException expected ->
                    CompletedSnapshotValue.Formatted (
                        expected,
                        fun x ->
                            try
                                x () |> ignore
                                "<no exception raised>"
                            with e ->
                                e.GetType().FullName + ": " + e.Message
                    )

            {
                SnapshotValue = snapshot
                Caller = source
                Actual = actual
            }
        | None, _ -> failwith "Must specify snapshot"
        | _, None -> failwith "Must specify actual value with 'return'"

    let internal replacement (s : CompletedSnapshotGeneric<'T>) =
        match s.SnapshotValue with
        | CompletedSnapshotValue.Json (_existing, options, _) ->
            JsonSerializer.Serialize (s.Actual (), options)
            |> JsonDocument.Parse
            |> _.RootElement
            |> _.ToString()
        | CompletedSnapshotValue.Formatted (_existing, f) -> f s.Actual
        | CompletedSnapshotValue.List (_existing, f) ->
            s.Actual ()
            |> unbox<IEnumerable>
            |> Seq.cast
            |> Seq.map (fun x -> f (fun () -> x))
            |> String.concat ",\n"

    /// Returns None if the assertion passes, or Some (expected, actual) if the assertion fails.
    let internal passesAssertion (state : CompletedSnapshotGeneric<'T>) : (string * string) option =
        match state.SnapshotValue with
        | CompletedSnapshotValue.Formatted (snapshot, f) ->
            let actual = f state.Actual
            if actual = snapshot then None else Some (snapshot, actual)
        | CompletedSnapshotValue.Json (snapshot, jsonSerOptions, jsonDocOptions) ->
            let canonicalSnapshot =
                try
                    JsonDocument.Parse (snapshot, jsonDocOptions) |> Some
                with _ ->
                    None

            let canonicalActual =
                JsonSerializer.Serialize (state.Actual (), jsonSerOptions) |> JsonDocument.Parse

            match canonicalSnapshot with
            | None -> Some ("[JSON failed to parse:] " + snapshot, canonicalActual.RootElement.ToString ())
            | Some canonicalSnapshot ->
                if not (JsonElement.DeepEquals (canonicalActual.RootElement, canonicalSnapshot.RootElement)) then
                    Some (canonicalSnapshot.RootElement.ToString (), canonicalActual.RootElement.ToString ())
                else
                    None
        | CompletedSnapshotValue.List (expected, f) ->
            let actual =
                state.Actual ()
                |> unbox<IEnumerable>
                |> Seq.cast
                |> Seq.map (fun x -> f (fun () -> x))
                |> Seq.toList

            if expected <> actual then
                Some (expected |> String.concat ",\n", actual |> String.concat ",\n")
            else
                None

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
