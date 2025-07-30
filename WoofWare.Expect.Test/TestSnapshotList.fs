namespace WoofWare.Expect.Test

open NUnit.Framework
open WoofWare.Expect

[<TestFixture>]
module TestSnapshotList =
    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // If you don't want to enter bulk-update mode, just replace this line with a no-op `()`.
        // The `updateAllSnapshots` tear-down below will simply do nothing in that case.
        // GlobalBuilderConfig.enterBulkUpdateMode ()
        ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    [<Test>]
    let ``simple list test`` () =
        expect {
            snapshotList [ "1" ; "2" ; "3" ]
            return [ 1..3 ]
        }

    [<Test>]
    let ``list test with formatting`` () =
        expect {
            snapshotList [ "8" ; "9" ; "0" ; "1" ; "2" ]
            withFormat (fun x -> string<int> (x % 10))
            return [ 8..12 ]
        }
