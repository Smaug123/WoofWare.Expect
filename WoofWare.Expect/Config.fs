namespace WoofWare.Expect

open System.Threading

/// Module holding global mutable state controlling the behaviour of WoofWare.Expect
/// when running in bulk-update mode.
[<RequireQualifiedAccess>]
module GlobalBuilderConfig =
    let internal bulkUpdate = ref 0

    /// <summary>
    /// Call this to make the <c>expect</c> builder register all tests for bulk update as it runs.
    /// </summary>
    /// <remarks>
    /// We *strongly* recommend making test fixtures <c>Parallelizable(ParallelScope.Children)</c> or less parallelizable (for NUnit) if you're running in bulk update mode.
    /// The implied global mutable state is liable to interfere with other expect builders in other fixtures otherwise.
    /// </remarks>
    let enterBulkUpdateMode () =
        if Interlocked.Increment bulkUpdate <> 1 then
            failwith
                "WoofWare.Expect requires bulk updates to happen serially: for example, make the test fixture `[<NonParallelizable>]` if you're using NUnit."

    let private allTests : ResizeArray<CompletedSnapshot> = ResizeArray ()

    /// <summary>
    /// Clear the set of failing tests registered by any previous bulk-update runs.
    /// </summary>
    ///
    /// <remarks>
    /// You probably don't need to do this, because your test runner is probably tearing down
    /// anyway after the tests have failed; this is mainly here for WoofWare.Expect's own internal testing.
    /// </remarks>
    let clearTests () = lock allTests allTests.Clear

    let internal registerTest (s : CompletedSnapshotGeneric<'T>) : unit =
        let toAdd = s |> CompletedSnapshot.make
        lock allTests (fun () -> allTests.Add toAdd)

    /// <summary>
    /// For all tests whose failures have already been registered,
    /// transform the files on disk so that the failing snapshots now pass.
    /// </summary>
    let updateAllSnapshots () =
        let bulkUpdate' = Interlocked.Decrement bulkUpdate

        try
            if bulkUpdate' = 0 then
                let allTests = lock allTests (fun () -> Seq.toArray allTests)
                SnapshotUpdate.updateAll allTests

        finally
            clearTests ()
