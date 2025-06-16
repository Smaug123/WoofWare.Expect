namespace WoofWare.Expect

open System.Runtime.CompilerServices
open System.Text.Json
open System.Text.Json.Serialization

type private CallerInfo =
    {
        MemberName : string
        FilePath : string
        LineNumber : int
    }

type private SnapshotValue =
    | BareString of string
    | Json of string

/// An exception indicating that a value failed to match its snapshot.
exception ExpectException of Message : string

/// A dummy type which is here to provide better error messages when you supply
/// the `snapshot` keyword multiple times.
type YouHaveSuppliedMultipleSnapshots = private | NonConstructible

/// The state accumulated by the `expect` builder. You should never find yourself interacting with this type.
type ExpectState<'T> =
    private
        {
            Snapshot : (SnapshotValue * CallerInfo) option
            Actual : 'T option
        }

[<RequireQualifiedAccess>]
module private Text =
    let predent (c : char) (s : string) =
        s.Split '\n' |> Seq.map (sprintf "%c %s" c) |> String.concat "\n"

/// <summary>
/// The builder which powers WoofWare.Expect.
/// </summary>
/// <remarks>You're not expected to construct this explicitly; it's a computation expression, available as <c>Builder.expect</c>.</remarks>
/// <param name="sourceOverride">Override the file path and line numbers reported in snapshots, so that your tests can be fully stable even on failure. (You almost certainly don't want to set this.)</param>
type ExpectBuilder (?sourceOverride : string * int) =
    /// Combine two `ExpectState`s. The first one is the "expected" snapshot; the second is the "actual".
    member _.Bind
        (state : ExpectState<YouHaveSuppliedMultipleSnapshots>, f : unit -> ExpectState<'U>)
        : ExpectState<'U>
        =
        let actual = f ()

        match state.Actual with
        | Some _ -> failwith "somehow came in with an Actual"
        | None ->

        match actual.Snapshot with
        | Some _ -> failwith "somehow Actual came through with a Snapshot"
        | None ->

        // Pass through the state structure when there's no actual value
        {
            Snapshot = state.Snapshot
            Actual = actual.Actual
        }

    /// <summary>Express that the actual value's <c>ToString</c> should identically equal this string.</summary>
    [<CustomOperation("snapshot", MaintainsVariableSpaceUsingBind = true)>]
    member _.Snapshot
        (
            state : ExpectState<unit>,
            snapshot : string,
            [<CallerMemberName>] ?memberName : string,
            [<CallerLineNumber>] ?callerLine : int,
            [<CallerFilePath>] ?filePath : string
        )
        : ExpectState<YouHaveSuppliedMultipleSnapshots>
        =
        match state.Snapshot with
        | Some _ -> failwith "snapshot can only be specified once"
        | None ->
            let memberName = defaultArg memberName "<unknown method>"
            let filePath = defaultArg filePath "<unknown file>"
            let lineNumber = defaultArg callerLine -1

            let callerInfo =
                {
                    MemberName = memberName
                    FilePath = filePath
                    LineNumber = lineNumber
                }

            {
                Snapshot = Some (SnapshotValue.BareString snapshot, callerInfo)
                Actual = None
            }

    /// <summary>
    /// Express that the actual value, when converted to JSON, should result in a JSON document
    /// which matches the JSON document that is this string.
    /// </summary>
    /// <remarks>
    /// For example, <c>snapshot "123"</c> indicates the JSON integer 123.
    /// </remarks>
    [<CustomOperation("snapshotJson", MaintainsVariableSpaceUsingBind = true)>]
    member _.SnapshotJson
        (
            state : ExpectState<unit>,
            snapshot : string,
            [<CallerMemberName>] ?memberName : string,
            [<CallerLineNumber>] ?callerLine : int,
            [<CallerFilePath>] ?filePath : string
        )
        : ExpectState<YouHaveSuppliedMultipleSnapshots>
        =
        match state.Snapshot with
        | Some _ -> failwith "snapshot can only be specified once"
        | None ->
            let memberName = defaultArg memberName "<unknown method>"
            let filePath = defaultArg filePath "<unknown file>"
            let lineNumber = defaultArg callerLine -1

            let callerInfo =
                {
                    MemberName = memberName
                    FilePath = filePath
                    LineNumber = lineNumber
                }

            {
                Snapshot = Some (SnapshotValue.Json snapshot, callerInfo)
                Actual = None
            }

    /// MaintainsVariableSpaceUsingBind causes this to be used; it's a dummy representing "no snapshot and no assertion".
    member _.Return (() : unit) : ExpectState<'T> =
        {
            Snapshot = None
            Actual = None
        }

    /// Expresses the "actual value" component of the assertion "expected snapshot = actual value".
    member _.Return (value : 'T) : ExpectState<'T> =
        {
            Snapshot = None
            Actual = Some value
        }

    /// Computation expression `Delay`.
    member _.Delay (f : unit -> ExpectState<'T>) : unit -> ExpectState<'T> = f

    /// Computation expression `Run`, which runs a `Delay`ed snapshot assertion, throwing if the assertion fails.
    member _.Run (f : unit -> ExpectState<'T>) : unit =
        let state = f ()

        let options = JsonFSharpOptions.Default().ToJsonSerializerOptions ()

        match state.Snapshot, state.Actual with
        | Some (snapshot, source), Some actual ->
            match snapshot with
            | SnapshotValue.Json snapshot ->
                let canonicalSnapshot = JsonDocument.Parse snapshot

                let canonicalActual =
                    JsonSerializer.Serialize (actual, options) |> JsonDocument.Parse

                if not (JsonElement.DeepEquals (canonicalActual.RootElement, canonicalSnapshot.RootElement)) then
                    sprintf
                        "snapshot mismatch! snapshot at %s:%i (%s) was:\n\n%s\n\nactual was:\n\n%s"
                        (sourceOverride |> Option.map fst |> Option.defaultValue source.FilePath)
                        (sourceOverride |> Option.map snd |> Option.defaultValue source.LineNumber)
                        source.MemberName
                        (canonicalSnapshot.RootElement.ToString () |> Text.predent '-')
                        (canonicalActual.RootElement.ToString () |> Text.predent '-')
                    |> ExpectException
                    |> raise

            | SnapshotValue.BareString snapshot ->
                let actual = actual.ToString ()

                if actual <> snapshot then
                    sprintf
                        "snapshot mismatch! snapshot at %s:%i (%s) was:\n\n%s\n\nactual was:\n\n%s"
                        (sourceOverride |> Option.map fst |> Option.defaultValue source.FilePath)
                        (sourceOverride |> Option.map snd |> Option.defaultValue source.LineNumber)
                        source.MemberName
                        (snapshot |> Text.predent '-')
                        (actual |> Text.predent '+')
                    |> ExpectException
                    |> raise

        | None, _ -> failwith "Must specify snapshot"
        | _, None -> failwith "Must specify actual value with 'return'"

/// Module containing the `expect` builder.
[<AutoOpen>]
module Builder =
    /// <summary>The primary WoofWare.Expect builder.</summary>
    ///
    /// <remarks>
    /// You are expected to use this like so:
    ///
    /// <code>
    /// expect {
    ///     snapshot "123"
    ///     return 124
    /// }
    /// </code>
    ///
    /// (That example expectation will fail, because the actual value 124 does not snapshot to the expected snapshot "123".)
    /// </remarks>
    let expect = ExpectBuilder ()

    /// <summary>
    /// This is the `expect` builder, but it mocks out the filepath reported on failure.
    /// </summary>
    /// <remarks>
    /// You probably don't want to use this; use `expect` instead.
    /// The point of the mocked builder is to allow fully predictable testing of the WoofWare.Expect library itself.
    /// </remarks>
    let expectWithMockedFilePath (path : string, line : int) = ExpectBuilder ((path, line))
