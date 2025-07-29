namespace WoofWare.Expect

open System.Collections.Generic
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
    new (sourceOverride : string * int) = ExpectBuilder (Mode.AssertMockingSource sourceOverride)

    new (update : bool)
        =
        if update then
            ExpectBuilder Mode.Update
        else
            ExpectBuilder Mode.Assert

    /// Combine two `ExpectStateListy`s. The first one is the "expected" snapshot; the second is the "actual".
    member _.Bind<'U> (state : ExpectStateListy<'U>, f : unit -> ExpectStateListy<'U>) : ExpectStateListy<'U> =
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

        {
            Formatter = formatter
            Snapshot = state.Snapshot
            Actual = actual.Actual
        }

    /// Combine an `ExpectStateListy` with an `ExpectState`. The first one is the "expected" snapshot; the second is
    /// the "actual".
    member _.Bind<'U, 'elt when 'U :> IEnumerable<'elt>>
        (state : ExpectStateListy<'elt>, f : unit -> ExpectState<'U>)
        : ExpectStateListy<'elt>
        =
        let actual = f ()

        match state.Actual with
        | Some _ -> failwith "somehow came in with an Actual"
        | None ->

        match actual.Snapshot with
        | Some _ -> failwith "somehow Actual came through with a Snapshot"
        | None ->

        let formatter : ((unit -> 'elt) -> string) option =
            match state.Formatter, actual.Formatter with
            | None, None -> None
            | None, Some _ ->
                failwith
                    "unexpectedly had a formatter supplied before the snapshotList keyword; I thought this was impossible"
            | Some f, None -> Some f
            | Some _, Some _ -> failwith "multiple formatters supplied for a single expect!"

        {
            Formatter = formatter
            Snapshot = state.Snapshot
            Actual = actual.Actual |> Option.map (fun f () -> Seq.cast<'elt> (f ()))
        }

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
            state : ExpectStateListy<'a>,
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
                Formatter = state.Formatter
                JsonSerialiserOptions = None
                JsonDocOptions = None
                Snapshot = Some (SnapshotValue.Formatted snapshot, callerInfo)
                Actual = None
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
    member this.SnapshotJson<'a>
        (
            state : ExpectStateListy<unit>,
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
                JsonSerialiserOptions = None
                JsonDocOptions = None
                Snapshot = Some (SnapshotValue.Json snapshot, callerInfo)
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
    /// Express that the actual value, which is a sequence, should have elements which individually (in order) match
    /// this snapshot list.
    /// </summary>
    /// <remarks>
    /// For example, <c>snapshotList ["123" ; "456"]</c> indicates an exactly-two-element list <c>[123 ; 456]</c>.
    /// </remarks>
    [<CustomOperation("snapshotList", MaintainsVariableSpaceUsingBind = true)>]
    member _.SnapshotList<'a>
        (
            state : ExpectStateListy<unit>,
            snapshot : string list,
            [<CallerMemberName>] ?memberName : string,
            [<CallerLineNumber>] ?callerLine : int,
            [<CallerFilePath>] ?filePath : string
        )
        : ExpectStateListy<'a>
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
                Snapshot = Some (snapshot, callerInfo)
                Actual = None
            }

    /// <summary>
    /// Expresses that the given expression throws during evaluation.
    /// </summary>
    /// <example>
    /// <code>
    /// expect {
    ///     snapshotThrows @"System.Exception: oh no"
    ///     return! (fun () -> failwith "oh no")
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("snapshotThrows", MaintainsVariableSpaceUsingBind = true)>]
    member _.SnapshotThrows<'a>
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

            {
                Formatter = None
                JsonSerialiserOptions = state.JsonSerialiserOptions
                JsonDocOptions = state.JsonDocOptions
                Snapshot = Some (SnapshotValue.ThrowsException snapshot, callerInfo)
                Actual = None
            }

    /// <summary>
    /// Expresses that the given expression throws during evaluation.
    /// </summary>
    /// <example>
    /// <code>
    /// expect {
    ///     snapshotThrows @"System.Exception: oh no"
    ///     return! (fun () -> failwith "oh no")
    /// }
    /// </code>
    /// </example>
    [<CustomOperation("snapshotThrows", MaintainsVariableSpaceUsingBind = true)>]
    member _.SnapshotThrows<'a>
        (
            state : ExpectStateListy<'a>,
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
                JsonSerialiserOptions = None
                JsonDocOptions = None
                Snapshot = Some (SnapshotValue.ThrowsException snapshot, callerInfo)
                Actual = None
            }

    /// <summary>
    /// Express that the <c>return</c> value of this builder should be formatted using this function, before
    /// comparing to the snapshot.
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
                Formatter = Some (fun f -> f () |> formatter)
            }

    /// <summary>
    /// Express that the <c>return</c> value of this builder should be formatted using this function, before
    /// comparing to the snapshot.
    /// In the case of <c>snapshotList</c>, this applies to the elements of the sequence, not to the sequence itself.
    /// </summary>
    /// <remarks>
    /// For example, <c>withFormat (fun x -> x.ToString ()) "123"</c> is equivalent to <c>snapshot "123"</c>.
    /// </remarks>
    [<CustomOperation("withFormat", MaintainsVariableSpaceUsingBind = true)>]
    member _.WithFormat<'T> (state : ExpectStateListy<'T>, formatter : 'T -> string) =
        match state.Formatter with
        | Some _ -> failwith "Please don't supply withFormat more than once"
        | None ->
            { state with
                Formatter = Some (fun f -> f () |> formatter)
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
    member _.WithJsonSerializerOptions<'T> (state : ExpectStateListy<'T>, jsonOptions : JsonSerializerOptions) =
        match state.Snapshot with
        | Some _ -> failwith "TODO describe this failure mode"
        | None ->

        match state.Actual with
        | Some _ -> failwith "TODO describe this failure mode"
        | None ->

        {
            Formatter = state.Formatter
            JsonSerialiserOptions = Some jsonOptions
            JsonDocOptions = None
            Snapshot = None
            Actual = None
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
    member _.Return (() : unit) : ExpectStateListy<'T> =
        {
            Formatter = None
            Snapshot = None
            Actual = None
        }

    /// Expresses the "actual value" component of the assertion "expected snapshot = actual value".
    member _.Return<'T> (value : 'T) : ExpectState<'T> =
        {
            Snapshot = None
            Formatter = None
            Actual = Some (fun () -> value)
            JsonSerialiserOptions = None
            JsonDocOptions = None
        }

    /// Expresses the "actual value" component of the assertion "expected snapshot = actual value", but delayed behind
    /// a function (by contrast with `Return`).
    member _.ReturnFrom (value : unit -> 'T) : ExpectState<'T> =
        {
            Snapshot = None
            Formatter = None
            JsonDocOptions = None
            JsonSerialiserOptions = None
            Actual = Some value
        }

    /// Computation expression `Delay`.
    member _.Delay (f : unit -> ExpectState<'T>) : unit -> ExpectState<'T> = f

    /// Computation expression `Delay`.
    member _.Delay (f : unit -> ExpectStateListy<'T>) : unit -> ExpectStateListy<'T> = f

    /// Computation expression `Run`, which runs a `Delay`ed snapshot assertion, throwing if the assertion fails.
    member _.Run (f : unit -> ExpectStateListy<'T>) : unit =
        let state = f ()

        match state.Actual with
        | None -> failwith "expected an assertion, but got none"
        | Some actual ->

        let actual = actual () |> Seq.toList

        match state.Snapshot with
        | None -> failwith "expected a snapshotList, but got none"
        | Some (snapshot, caller) ->

        let formatter =
            match state.Formatter with
            | Some f -> fun x -> f (fun () -> x)
            | None -> fun x -> x.ToString ()

        let actual = actual |> List.map formatter

        if snapshot <> actual then
            let diff =
                Diff.patienceLines (Array.ofList snapshot) (Array.ofList actual) |> Diff.format

            match mode with
            | Mode.Assert ->
                if GlobalBuilderConfig.isBulkUpdateMode () then
                    GlobalBuilderConfig.registerTest state
                else
                    $"snapshot mismatch! snapshot at %s{caller.FilePath}:%i{caller.LineNumber} (%s{caller.MemberName}) diff:\n%s{diff}"
                    |> ExpectException
                    |> raise
            | Mode.AssertMockingSource (mockSource, line) ->
                $"snapshot mismatch! snapshot at %s{mockSource}:%i{line} (%s{caller.MemberName}) diff:\n%s{diff}"
                |> ExpectException
                |> raise
            | Mode.Update ->
                let lines = File.ReadAllLines caller.FilePath
                let oldContents = String.concat "\n" lines

                let listSource =
                    AstWalker.findSnapshotList caller.FilePath lines caller.LineNumber caller.MemberName

                let indent = String.replicate listSource.KeywordRange.StartColumn " "

                let result =
                    [|
                        // Range's lines are one-indexed!
                        lines.[0 .. listSource.KeywordRange.EndLine - 2]
                        [|
                            lines.[listSource.KeywordRange.EndLine - 1].Substring (0, listSource.KeywordRange.EndColumn)
                            + " ["
                        |]
                        actual
                        |> Seq.map (fun s -> indent + "    " + SnapshotUpdate.stringLiteral s)
                        |> Array.ofSeq
                        [|
                            indent
                            + "]"
                            + lines.[listSource.ListRange.EndLine - 1].Substring listSource.ListRange.EndColumn
                        |]
                        lines.[listSource.ListRange.EndLine ..]
                    |]
                    |> Array.concat

                File.writeAllLines result caller.FilePath
                failwith ("Snapshot successfully updated. Previous contents:\n" + oldContents)

    /// Computation expression `Run`, which runs a `Delay`ed snapshot assertion, throwing if the assertion fails.
    member _.Run (f : unit -> ExpectState<'T>) : unit =
        let state = f () |> CompletedSnapshotGeneric.make

        let raiseError (snapshot : string) (actual : string) : unit =
            match mode with
            | Mode.AssertMockingSource (mockSource, line) ->
                let diff = Diff.patience snapshot actual

                sprintf
                    "snapshot mismatch! snapshot at %s:%i (%s) diff:\n\n%s"
                    mockSource
                    line
                    state.Caller.MemberName
                    (Diff.format diff)
                |> ExpectException
                |> raise
            | Mode.Assert ->
                if GlobalBuilderConfig.isBulkUpdateMode () then
                    GlobalBuilderConfig.registerTest state
                else
                    let diff = Diff.patience snapshot actual

                    sprintf
                        "snapshot mismatch! snapshot at %s:%i (%s) diff:\n\n%s"
                        state.Caller.FilePath
                        state.Caller.LineNumber
                        state.Caller.MemberName
                        (Diff.format diff)
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
