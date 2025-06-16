namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
[<NonParallelizable>]
module BulkUpdateExample =

    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // Uncomment the `enterBulkUpdateMode` to cause all failing tests to accumulate their results
        // into a global mutable collection.
        // At the end of the test run, you should then call `updateAllSnapshots ()`
        // to commit these accumulated failures to the source files.
        //
        // When in bulk update mode, all tests will fail, to remind you to exit bulk update mode afterwards.
        //
        // We *strongly* recommend making these test fixtures `[<NonParallelizable>]`.

        // GlobalBuilderConfig.enterBulkUpdateMode ()
        ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    [<Test>]
    let ``Snapshot 2`` () =
        expect {
            snapshotJson
                @"{
  ""1"": ""hi"",
  ""2"": ""my"",
  ""3"": ""name"",
  ""4"": ""is""
}"

            return Map.ofList [ "1", "hi" ; "2", "my" ; "3", "name" ; "4", "is" ]
        }

    [<Test>]
    let ``Snapshot 1`` () =
        expect {
            snapshotJson @"123"
            return 123
        }
