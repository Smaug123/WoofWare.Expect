namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
module SimpleTest =
    [<Test>]
    let ``JSON is resilient to whitespace changes`` () =
        expect {
            snapshotJson " 123 "
            return 123
        }

    [<Test>]
    let ``Example of a failing test`` () =
        expect {
            snapshot
                @"snapshot mismatch! snapshot at filepath.fs:99 (Example of a failing test) was:

- 123

actual was:

+ 124"

            return
                Assert
                    .Throws<ExpectException>(fun () ->
                        expectWithMockedFilePath ("filepath.fs", 99) {
                            snapshot "123"
                            return 124
                        }
                    )
                    .Message
        }

    [<Test>]
    let ``Basic example`` () =
        expect {
            snapshot @"123"
            return 123
        }

    [<Test>]
    let ``Formatting example`` () =
        expect {
            snapshot @"123"
            snapshotFormat (fun x -> string<int> (x + 1))
            return 123
        }
