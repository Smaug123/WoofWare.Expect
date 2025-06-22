namespace WoofWare.Expect.Test

open System.Collections.Generic
open System.Text.Json
open System.Text.Json.Serialization
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
                @"snapshot mismatch! snapshot at filepath.fs:99 (Example of a failing test) diff:

- 123
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
            withFormat (fun x -> x.GetType().Name)
            snapshot @"Int32"
            return 123
        }

        expect {
            snapshot @"Int32"
            withFormat (fun x -> x.GetType().Name)
            return 123
        }

    [<Test>]
    let ``Custom JSON output`` () =
        // Out of the box, comments in snapshots cause the JSON parser to throw, so the snapshot fails to match...
        expect {
            snapshot
                @"snapshot mismatch! snapshot at file.fs:99 (Custom JSON output) diff:

- [JSON failed to parse:] {
-   // a key here
+ {
    ""a"": 3
  }"

            return
                Assert.Throws<ExpectException> (fun () ->
                    expectWithMockedFilePath ("file.fs", 99) {
                        snapshotJson
                            @"{
  // a key here
  ""a"": 3
}"

                        return Map.ofList [ "a", 3 ]
                    }
                )
                |> _.Message
        }

        // but it can be made to like them!
        expect {
            snapshotJson
                @"{
                // a key here
                ""a"":3
            }"

            withJsonDocOptions (JsonDocumentOptions (CommentHandling = JsonCommentHandling.Skip))
            return Map.ofList [ "a", 3 ]
        }

    type SomeDu =
        | Something of IReadOnlyDictionary<string, string>
        | SomethingElse of string

    type MoreComplexType =
        {
            Thing : int
            SomeDu : SomeDu option
        }

    [<Test>]
    let ``JSON snapshot of complex ADT`` () =
        expect {
            snapshotJson
                @"{
  ""SomeDu"": {
    ""Case"": ""Something"",
    ""Fields"": [
      {
        ""hi"": ""bye""
      }
    ]
  },
  ""Thing"": 3,
}"

            return
                {
                    Thing = 3
                    SomeDu = Some (SomeDu.Something (Map.ofList [ "hi", "bye" ]))
                }
        }

    [<Test>]
    let ``Overriding JSON format, from docstring`` () =
        expect {
            snapshotJson @"{""a"":3}"
            withJsonSerializerOptions (JsonSerializerOptions (WriteIndented = false))
            return Map.ofList [ "a", 3 ]
        }

    [<Test>]
    let ``Overriding the JSON format`` () =
        expect {
            snapshotJson
                @"{
  ""Thing"": 3,
  ""SomeDu"": [
    ""Some"",
    [
      ""Something"",
      {
        ""hi"": ""bye""
      }
    ]
  ]
}"

            withJsonSerializerOptions (
                let options = JsonFSharpOptions.ThothLike().ToJsonSerializerOptions ()
                options.WriteIndented <- true
                options
            )

            return
                {
                    Thing = 3
                    SomeDu = Some (SomeDu.Something (Map.ofList [ "hi", "bye" ]))
                }
        }
