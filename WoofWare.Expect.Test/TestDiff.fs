namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
module TestDiff =

    [<Test>]
    let ``Basic diff`` () =
        let textA =
            [|
                "The quick brown fox"
                "jumps over"
                "the lazy dog"
                "Some unique line here"
                "The end"
            |]

        let textB =
            [|
                "The quick brown fox"
                "Some unique line here"
                "jumps over"
                "the lazy dog"
                "Another line"
                "The end"
            |]

        let diff = Diff.patience textA textB

        expect {
            snapshot
                @"    0   0  The quick brown fox
+       1  Some unique line here
    1   2  jumps over
    2   3  the lazy dog
-   3      Some unique line here
+       4  Another line
    4   5  The end"

            withFormat (Diff.formatDiff >> String.concat "\n")
            return diff
        }
