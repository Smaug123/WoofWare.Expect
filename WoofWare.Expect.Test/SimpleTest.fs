namespace WoofWare.Expect.Test

open System
open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
module SimpleTest =
    [<Test>]
    let ``JSON is resilient to whitespace changes`` () =
        expect {
            snapshotJson "123  "
            return 123
        }

    [<Test>]
    let ``Example of a failing test`` () =
        expect {
            snapshot
                "snapshot mismatch! snapshot at filepath.fs:32 (Example of a failing test) was:

- 123

actual was:

+ 124"

            return
                Assert
                    .Throws<Exception>(fun () ->
                        expectWithMockedFilePath "filepath.fs" {
                            snapshot "123"
                            return 124
                        }
                    )
                    .Message
        }
