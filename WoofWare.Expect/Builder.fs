namespace WoofWare.Expect

open System.IO
open System.Runtime.CompilerServices
open System.Text.Json

/// An exception indicating that a value failed to match its snapshot.
exception ExpectException of Message : string

/// <summary>Specify how the Expect computation expression treats failures.</summary>
/// <remarks>You probably don't want to use this directly; use the computation expression definitions
/// like <c>expect</c> in the <c>Builder</c> module instead.</remarks>
type Mode =
    private
    | Assert
    | Update
    | AssertMockingSource of (string * int)

/// <summary>
/// The builder which powers WoofWare.Expect.
/// </summary>
/// <remarks>You're not expected to construct this explicitly; it's a computation expression, available as <c>Builder.expect</c>.</remarks>
/// <param name="applyChanges">When running the tests, instead of throwing an exception on failure, update the snapshot.</param>
/// <param name="sourceOverride">Override the file path and line numbers reported in snapshots, so that your tests can be fully stable even on failure. (You almost certainly don't want to set this.)</param>
type ExpectBuilder (mode : Mode) =
    member private this.Mode = Unchecked.defaultof<Mode>

    new (sourceOverride : string * int) = ExpectBuilder (Mode.AssertMockingSource sourceOverride)

    new (update : bool)
        =
        if update then
            ExpectBuilder Mode.Update
        else
            ExpectBuilder Mode.Assert

    /// Combine two `ExpectState`s. The first one is the "expected" snapshot; the second is the "actual".
    member _.Bind<'U> (state : ExpectState<'U>, f : unit -> ExpectState<'U>) : ExpectState<'U> =
        let actual = f ()

        match state.Actual with
        | Some _ -> failwith "somehow came in with an Actual"
        | None ->

        match actual.Snapshot with
        | Some _ -> failwith "somehow Actual came through with a Snapshot"
        | None ->

        let formatter =
            match state.Formatter, actual.Formatter with
            | None, f -> f
            | Some f, None -> Some f
            | Some _, Some _ -> failwith "multiple formatters supplied for a single expect!"

        let jsonSerOptions =
            match state.JsonSerialiserOptions, actual.JsonSerialiserOptions with
            | None, f -> f
            | Some f, None -> Some f
            | Some _, Some _ -> failwith "multiple JSON serialiser options supplied for a single expect!"

        let jsonDocOptions =
            match state.JsonDocOptions, actual.JsonDocOptions with
            | None, f -> f
            | Some f, None -> Some f
            | Some _, Some _ -> failwith "multiple JSON document options supplied for a single expect!"

        // Pass through the state structure when there's no actual value
        {
            Formatter = formatter
            Snapshot = state.Snapshot
            Actual = actual.Actual
            JsonSerialiserOptions = jsonSerOptions
            JsonDocOptions = jsonDocOptions
        }

    /// <summary>Express that the actual value's <c>ToString</c> should identically equal this string.</summary>
    [<CustomOperation("snapshot", MaintainsVariableSpaceUsingBind = true)>]
    member _.Snapshot<'a>
        (
            state : ExpectState<'a>,
            snapshot : string,
            [<CallerMemberName>] ?memberName : string,
            [<CallerLineNumber>] ?callerLine : int,
            [<CallerFilePath>] ?filePath : string
        )
        : ExpectState<'a>
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

            { state with
                Snapshot = Some (SnapshotValue.Formatted snapshot, callerInfo)
                Actual = None
            }

    /// <summary>
    /// Express that the actual value, when converted to JSON, should result in a JSON document
    /// which matches the JSON document that is this string.
    /// </summary>
    /// <remarks>
    /// For example, <c>snapshotJson "123"</c> indicates the JSON integer 123.
    /// </remarks>
    [<CustomOperation("snapshotJson", MaintainsVariableSpaceUsingBind = true)>]
    member _.SnapshotJson<'a>
        (
            state : ExpectState<unit>,
            snapshot : string,
            [<CallerMemberName>] ?memberName : string,
            [<CallerLineNumber>] ?callerLine : int,
            [<CallerFilePath>] ?filePath : string
        )
        : ExpectState<'a>
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
                Formatter = None
                JsonSerialiserOptions = state.JsonSerialiserOptions
                JsonDocOptions = state.JsonDocOptions
                Snapshot = Some (SnapshotValue.Json snapshot, callerInfo)
                Actual = None
            }

    /// <summary>
    /// Express that the <c>return</c> value of this builder should be formatted using this function, before
    /// comparing to the snapshot.
    /// this value.
    /// </summary>
    /// <remarks>
    /// For example, <c>withFormat (fun x -> x.ToString ()) "123"</c> is equivalent to <c>snapshot "123"</c>.
    /// </remarks>
    [<CustomOperation("withFormat", MaintainsVariableSpaceUsingBind = true)>]
    member _.WithFormat<'T> (state : ExpectState<'T>, formatter : 'T -> string) =
        match state.Formatter with
        | Some _ -> failwith "Please don't supply withFormat more than once"
        | None ->
            { state with
                Formatter = Some formatter
            }

    /// <summary>
    /// Express that these JsonSerializerOptions should be used to construct the JSON object to which the snapshot
    /// is to be compared (or, in write-out-the-snapshot mode, to construct the JSON object to be written out).
    /// </summary>
    /// <example>
    /// If you want your snapshots to be written out compactly, rather than the default indenting:
    /// <code>
    /// expect {
    ///     snapshotJson @"{""a"":3}"
    ///     withJsonSerializerOptions (JsonSerializerOptions (WriteIndented = false))
    ///     return Map.ofList ["a", 3]
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("withJsonSerializerOptions", MaintainsVariableSpaceUsingBind = true)>]
    member _.WithJsonSerializerOptions<'T> (state : ExpectState<'T>, jsonOptions : JsonSerializerOptions) =
        match state.JsonSerialiserOptions with
        | Some _ -> failwith "Please don't supply withJsonSerializerOptions more than once"
        | None ->
            { state with
                JsonSerialiserOptions = Some jsonOptions
            }

    /// <summary>
    /// Express that these JsonDocumentOptions should be used when parsing the snapshot string into a JSON object.
    /// </summary>
    /// <remarks>
    /// For example, you might use this if you want your snapshot to contain comments;
    /// the default JSON document parser will instead throw on comments, causing the snapshot instantly to fail to match.
    /// </remarks>
    /// <example>
    /// <code>
    /// expect {
    ///     snapshotJson
    ///         @"{
    ///         // a key here
    ///         ""a"":3
    ///     }"
    ///
    ///     withJsonDocOptions (JsonDocumentOptions (CommentHandling = JsonCommentHandling.Skip))
    ///     return Map.ofList [ "a", 3 ]
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("withJsonDocOptions", MaintainsVariableSpaceUsingBind = true)>]
    member _.WithJsonDocOptions<'T> (state : ExpectState<'T>, jsonOptions : JsonDocumentOptions) =
        match state.JsonDocOptions with
        | Some _ -> failwith "Please don't supply withJsonDocOptions more than once"
        | None ->
            { state with
                JsonDocOptions = Some jsonOptions
            }

    /// MaintainsVariableSpaceUsingBind causes this to be used; it's a dummy representing "no snapshot and no assertion".
    member _.Return (() : unit) : ExpectState<'T> =
        {
            Formatter = None
            JsonSerialiserOptions = None
            JsonDocOptions = None
            Snapshot = None
            Actual = None
        }

    /// Expresses the "actual value" component of the assertion "expected snapshot = actual value".
    member _.Return (value : 'T) : ExpectState<'T> =
        {
            Snapshot = None
            Formatter = None
            JsonDocOptions = None
            JsonSerialiserOptions = None
            Actual = Some value
        }

    /// Computation expression `Delay`.
    member _.Delay (f : unit -> ExpectState<'T>) : unit -> ExpectState<'T> = f

    /// Computation expression `Run`, which runs a `Delay`ed snapshot assertion, throwing if the assertion fails.
    member _.Run (f : unit -> ExpectState<'T>) : unit =
        let state = f () |> CompletedSnapshotGeneric.make

        let raiseError (snapshot : string) (actual : string) : unit =
            match mode with
            | Mode.AssertMockingSource (mockSource, line) ->
                sprintf
                    "snapshot mismatch! snapshot at %s:%i (%s) was:\n\n%s\n\nactual was:\n\n%s"
                    mockSource
                    line
                    state.Caller.MemberName
                    (snapshot |> Text.predent '-')
                    (actual |> Text.predent '+')
                |> ExpectException
                |> raise
            | Mode.Assert ->
                if GlobalBuilderConfig.isBulkUpdateMode () then
                    GlobalBuilderConfig.registerTest state
                else
                    sprintf
                        "snapshot mismatch! snapshot at %s:%i (%s) was:\n\n%s\n\nactual was:\n\n%s"
                        state.Caller.FilePath
                        state.Caller.LineNumber
                        state.Caller.MemberName
                        (snapshot |> Text.predent '-')
                        (actual |> Text.predent '+')
                    |> ExpectException
                    |> raise
            | Mode.Update ->
                let lines = File.ReadAllLines state.Caller.FilePath
                let oldContents = String.concat "\n" lines
                let lines = SnapshotUpdate.updateSnapshotAtLine lines state.Caller.LineNumber actual
                File.writeAllLines lines state.Caller.FilePath
                failwith ("Snapshot successfully updated. Previous contents:\n" + oldContents)

        match CompletedSnapshotGeneric.passesAssertion state with
        | None ->
            match mode, GlobalBuilderConfig.isBulkUpdateMode () with
            | Mode.Update, _
            | _, true ->
                failwith
                    "Snapshot assertion passed, but we are in snapshot-updating mode. Use the `expect` builder instead of `expect'` to assert the contents of a single snapshot; disable `GlobalBuilderConfig.bulkUpdate` to move back to assertion-checking mode."
            | _ -> ()
        | Some (expected, actual) -> raiseError expected actual

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
    let expect = ExpectBuilder false

    /// <summary>The WoofWare.Expect builder, but in "replace snapshot on failure" mode.</summary>
    ///
    /// <remarks>
    /// Take an existing failing snapshot test:
    ///
    /// <code>
    /// expect {
    ///     snapshot "123"
    ///     return 124
    /// }
    /// </code>
    ///
    /// Add the <c>'</c> marker to the <c>expect</c> builder:
    /// <code>
    /// expect' {
    ///     snapshot "123"
    ///     return 124
    /// }
    /// </code>
    ///
    /// Rerun, and observe that the snapshot becomes updated.
    /// This rerun will throw an exception, to help make sure you don't commit the snapshot builder while it's flipped to "update" mode.
    /// </remarks>
    let expect' = ExpectBuilder true

    /// <summary>
    /// This is the `expect` builder, but it mocks out the filepath reported on failure.
    /// </summary>
    /// <remarks>
    /// You probably don't want to use this; use `expect` instead.
    /// The point of the mocked builder is to allow fully predictable testing of the WoofWare.Expect library itself.
    /// </remarks>
    let expectWithMockedFilePath (path : string, line : int) = ExpectBuilder ((path, line))
