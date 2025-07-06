namespace WoofWare.Expect.Test

open NUnit.Framework
open WoofWare.Expect

[<TestFixture>]
module TestExceptionThrowing =

    [<Test>]
    let ``Can throw an exception`` () =
        expect {
            snapshotThrows @"System.Exception: oh no"
            return! (fun () -> failwith<int> "oh no")
        }
