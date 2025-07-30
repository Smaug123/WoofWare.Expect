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

/// The state accumulated by the `expect` builder. You should never find yourself interacting with this type.
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

type internal CompletedListSnapshotGeneric<'elt> =
    private
        {
            Expected : string list
            Format : 'elt -> string
            Caller : CallerInfo
            Actual : unit -> 'elt seq
        }

[<RequireQualifiedAccess>]
module internal CompletedListSnapshotGeneric =
    let replacement (s : CompletedListSnapshotGeneric<'T>) =
        s.Actual () |> unbox<IEnumerable> |> Seq.cast |> Seq.map s.Format |> Seq.toList

    /// Returns None if the assertion passes, or Some (expected, actual) if the assertion fails.
    let internal passesAssertion (state : CompletedListSnapshotGeneric<'T>) : (string list * string list) option =
        let actual =
            state.Actual ()
            |> unbox<IEnumerable>
            |> Seq.cast
            |> Seq.map state.Format
            |> Seq.toList

        if state.Expected <> actual then
            Some (state.Expected, actual)
        else
            None

    let make (state : ExpectStateListy<'elt>) : CompletedListSnapshotGeneric<'elt> =
        match state.Actual with
        | None -> failwith "expected an assertion, but got none"
        | Some actual ->

        match state.Snapshot with
        | None -> failwith "expected a snapshotList, but got none"
        | Some (snapshot, caller) ->

        let formatter =
            match state.Formatter with
            | Some f -> fun x -> f (fun () -> x)
            | None -> fun x -> x.ToString ()

        {
            Expected = snapshot
            Format = formatter
            Caller = caller
            Actual = actual
        }

/// Represents a snapshot test that has failed and is awaiting update or report to the user.
type internal CompletedSnapshot =
    | GuessString of CallerInfo * replacement : string
    | Known of CallerInfo * replacement : string list * SnapshotLocation

    member this.CallerInfo =
        match this with
        | CompletedSnapshot.GuessString (c, _) -> c
        | CompletedSnapshot.Known (c, _, _) -> c

[<RequireQualifiedAccess>]
module internal CompletedSnapshot =
    let makeGuess (s : CompletedSnapshotGeneric<'T>) =
        CompletedSnapshot.GuessString (s.Caller, CompletedSnapshotGeneric.replacement s)

    let makeFromAst (source : SnapshotLocation) (s : CompletedListSnapshotGeneric<'elt>) =
        CompletedSnapshot.Known (s.Caller, CompletedListSnapshotGeneric.replacement s, source)
