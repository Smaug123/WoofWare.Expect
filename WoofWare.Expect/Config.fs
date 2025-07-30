namespace WoofWare.Expect

/// Module holding global mutable state controlling the behaviour of WoofWare.Expect
/// when running in bulk-update mode.
[<RequireQualifiedAccess>]
module GlobalBuilderConfig =
    /// All access to the global mutable state locks on this.
    let private locker = obj ()

    // Global mutable state ensuring there is at most one `enterBulkUpdateMode`/`updateAllSnapshots` pair running at once.
    let private bulkUpdate = ref 0

    let private allTests : ResizeArray<CompletedSnapshot> = ResizeArray ()

    let internal isBulkUpdateMode () : bool =
        lock locker (fun () -> bulkUpdate.Value > 0)

    /// <summary>
    /// Call this to make the <c>expect</c> builder register all tests for bulk update as it runs.
    /// </summary>
    /// <remarks>
    /// We *strongly* recommend making test fixtures <c>Parallelizable(ParallelScope.Children)</c> or less parallelizable (for NUnit) if you're running in bulk update mode.
    /// The implied global mutable state is liable to interfere with other expect builders in other fixtures otherwise.
    /// </remarks>
    let enterBulkUpdateMode () =
        lock
            locker
            (fun () ->
                if bulkUpdate.Value <> 0 then
                    failwith
                        "WoofWare.Expect requires bulk updates to happen serially: for example, make the test fixture `[<NonParallelizable>]` if you're using NUnit."

                bulkUpdate.Value <- bulkUpdate.Value + 1
            )

    /// <summary>
    /// Clear the set of failing tests registered by any previous bulk-update runs.
    /// </summary>
    ///
    /// <remarks>
    /// You probably don't need to do this, because your test runner is probably tearing down
    /// anyway after the tests have failed; this is mainly here for WoofWare.Expect's own internal testing.
    /// </remarks>
    let clearTests () = lock locker allTests.Clear

    let internal registerTest (toAdd : CompletedSnapshot) : unit =
        lock locker (fun () -> allTests.Add toAdd)

    /// <summary>
    /// For all tests whose failures have already been registered,
    /// transform the files on disk so that the failing snapshots now pass.
    /// </summary>
    let updateAllSnapshots () =
        // It's OK for this to be called when `enterBulkUpdateMode` has not been called, i.e. when `bulkUpdate` has
        // value 0. That just means we aren't in bulk-update mode, so we expect the following simply to do nothing.
        // (This is an expected workflow: we expect users to run `updateAllSnapshots` unconditionally in a
        // one-time tear-down of the test suite, and they use the one-time setup to control whether any work is actually
        // performed here.)
        lock
            locker
            (fun () ->
                let allTests = Seq.toArray allTests

                try
                    SnapshotUpdate.updateAll allTests
                finally
                    // double acquiring of reentrant lock is OK, we're not switching threads
                    clearTests ()
                    bulkUpdate.Value <- 0
            )
