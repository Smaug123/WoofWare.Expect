namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
[<Parallelizable(ParallelScope.Children)>]
module TestSnapshotFinding =
    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // GlobalBuilderConfig.enterBulkUpdateMode ()
        ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    type Dummy = class end

    [<Test>]
    let ``Triple-quote, one line, one-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @""replacement""
            return 123
        }
"

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``Triple-quote, one line, multi-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @""replacement
more""
            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "replacement\nmore"
                |> String.concat "\n"
        }

    [<Test>]
    let ``At-string, one line, one-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "AtStringOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @""replacement""
            return 123
        }
"

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``At-string, one line, multi-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "AtStringOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @""replacement
more""
            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "replacement\nmore"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Triple-quote, intervening comment, one-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteInterveningComment.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot (* comment *)
                @""replacement""

            return 123
        }
"

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``Triple-quote, intervening comment, multi-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteInterveningComment.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot (* comment *)
                @""replacement
more""

            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "replacement\nmore"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Single-quote, many lines, one-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "SingleQuoteManyLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot
                @""replacement""

            return 123
        }
"

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``Single-quote, many lines, multi-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "SingleQuoteManyLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot
                @""replacement
more""

            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "replacement\nmore"
                |> String.concat "\n"
        }
