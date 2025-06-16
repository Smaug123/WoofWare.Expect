namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
module TestSnapshotFinding =

    type Dummy = class end

    [<Test>]
    let ``Triple-quote, one line`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                """
namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @"replacement"
            return 123
        }

"""

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``At-string, one line`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "AtStringOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                """
namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @"replacement"
            return 123
        }

"""

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``Single-quote, many lines`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "SingleQuoteManyLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                """
namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @"replacement"
            return 123
        }

"""

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``Triple-quote, intervening comment`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteInterveningComment.fs"
            |> _.Split('\n')

        expect {
            snapshot
                """
namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot (* comment *) @"replacement"
            return 123
        }

"""

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }
