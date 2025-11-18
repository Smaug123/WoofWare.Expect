namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework
open FsUnitTyped

[<TestFixture>]
[<Parallelizable(ParallelScope.Children)>]
module TestFindSnapshotList =
    type Dummy = class end

    [<Test>]
    let ``Snapshot inside try-with block`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TryWithBlock.fs"
            |> _.Split('\n')

        // Should successfully find the snapshot location
        let location = AstWalker.findSnapshotList "" source 10 "snapshotList"

        // Verify the keyword was found at the correct line
        location.KeywordRange.StartLine |> shouldEqual 10

    [<Test>]
    let ``Snapshot inside try-finally block`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TryFinallyBlock.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 10 "snapshotList"
        location.KeywordRange.StartLine |> shouldEqual 10

    [<Test>]
    let ``Snapshot inside for loop`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "ForLoop.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 9 "snapshotList"
        location.KeywordRange.StartLine |> shouldEqual 9

    [<Test>]
    let ``Snapshot inside foreach loop`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "ForEachLoop.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 11 "snapshotList"
        location.KeywordRange.StartLine |> shouldEqual 11

    [<Test>]
    let ``Snapshot inside while loop`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "WhileLoop.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 11 "snapshotList"
        location.KeywordRange.StartLine |> shouldEqual 11

    [<Test>]
    let ``Snapshot with yield in sequence expression`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "YieldExpression.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 10 "snapshotList"
        location.KeywordRange.StartLine |> shouldEqual 10

    // Tests for regular 'snapshot' keyword (for future when AST walker is used for all snapshots)
    // These tests verify that when the AST walker is eventually used for regular snapshots,
    // it will properly handle snapshots inside these constructs.

    [<Test>]
    let ``Regular snapshot inside try-with block`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TryWithBlock_Snapshot.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 10 "snapshot"
        location.KeywordRange.StartLine |> shouldEqual 10

    [<Test>]
    let ``Regular snapshot inside try-finally block`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TryFinallyBlock_Snapshot.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 10 "snapshot"
        location.KeywordRange.StartLine |> shouldEqual 10

    [<Test>]
    let ``Regular snapshot inside for loop`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "ForLoop_Snapshot.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 9 "snapshot"
        location.KeywordRange.StartLine |> shouldEqual 9

    [<Test>]
    let ``Regular snapshot inside foreach loop`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "ForEachLoop_Snapshot.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 11 "snapshot"
        location.KeywordRange.StartLine |> shouldEqual 11

    [<Test>]
    let ``Regular snapshot inside while loop`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "WhileLoop_Snapshot.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 11 "snapshot"
        location.KeywordRange.StartLine |> shouldEqual 11

    [<Test>]
    let ``Regular snapshot with yield in sequence expression`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "YieldExpression_Snapshot.fs"
            |> _.Split('\n')

        let location = AstWalker.findSnapshotList "" source 10 "snapshot"
        location.KeywordRange.StartLine |> shouldEqual 10
