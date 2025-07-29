namespace WoofWare.Expect.Test

open NUnit.Framework
open WoofWare.Expect

[<TestFixture>]
module TestSnapshotList =

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
