namespace WoofWare.Expect.Test

open NUnit.Framework
open WoofWare.Expect

[<TestFixture>]
module TestMultilineComplex =
    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // GlobalBuilderConfig.enterBulkUpdateMode ()
        ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    type Dummy = class end

    [<Test>]
    let ``Very long multiline string`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "MultilineComplex.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let veryLongMultiline () =
        expect {
            snapshot
                @""Replaced with
a different
multiline
value""

            return ""long""
        }

    let multilineWithEmptyLines () =
        expect {
            snapshot
                @""First line

Third line


Sixth line""

            return ""empty lines""
        }

    let multilineWithSpecialChars () =
        expect {
            snapshot
                """"""Special chars:
    Tab:	here
    Quotes: ""double"" and 'single'
    Backslash: \ and \\
    Unicode: 🎯
    Regex: .*+?[]""""""

            return ""special""
        }

    let multilineJson () =
        expect {
            snapshotJson
                @""{
  """"name"""": """"test"""",
  """"values"""": [
    1,
    2,
    3
  ],
  """"nested"""": {
    """"deep"""": true
  }
}""

            return
                {
                    name = ""test""
                    values = [ 1 ; 2 ; 3 ]
                    nested =
                        {|
                            deep = true
                        |}
                }
        }

    let windowsLineEndings () =
        expect {
            snapshot ""Line 1\r\nLine 2\r\nLine 3""
            return ""crlf""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "Replaced with\na different\nmultiline\nvalue"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Multiline with empty lines`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "MultilineComplex.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let veryLongMultiline () =
        expect {
            snapshot
                """"""Line 1
Line 2
Line 3
Line 4
Line 5
Line 6
Line 7
Line 8
Line 9
Line 10
    Indented line 11
        More indented line 12
Line 13
Line 14
Line 15""""""

            return ""long""
        }

    let multilineWithEmptyLines () =
        expect {
            snapshot
                @""Replaced with

a different
multiline
value""

            return ""empty lines""
        }

    let multilineWithSpecialChars () =
        expect {
            snapshot
                """"""Special chars:
    Tab:	here
    Quotes: ""double"" and 'single'
    Backslash: \ and \\
    Unicode: 🎯
    Regex: .*+?[]""""""

            return ""special""
        }

    let multilineJson () =
        expect {
            snapshotJson
                @""{
  """"name"""": """"test"""",
  """"values"""": [
    1,
    2,
    3
  ],
  """"nested"""": {
    """"deep"""": true
  }
}""

            return
                {
                    name = ""test""
                    values = [ 1 ; 2 ; 3 ]
                    nested =
                        {|
                            deep = true
                        |}
                }
        }

    let windowsLineEndings () =
        expect {
            snapshot ""Line 1\r\nLine 2\r\nLine 3""
            return ""crlf""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 30 "Replaced with\n\na different\nmultiline\nvalue"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Multiline with special chars`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "MultilineComplex.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let veryLongMultiline () =
        expect {
            snapshot
                """"""Line 1
Line 2
Line 3
Line 4
Line 5
Line 6
Line 7
Line 8
Line 9
Line 10
    Indented line 11
        More indented line 12
Line 13
Line 14
Line 15""""""

            return ""long""
        }

    let multilineWithEmptyLines () =
        expect {
            snapshot
                @""First line

Third line


Sixth line""

            return ""empty lines""
        }

    let multilineWithSpecialChars () =
        expect {
            snapshot
                @""get rid of it all""

            return ""special""
        }

    let multilineJson () =
        expect {
            snapshotJson
                @""{
  """"name"""": """"test"""",
  """"values"""": [
    1,
    2,
    3
  ],
  """"nested"""": {
    """"deep"""": true
  }
}""

            return
                {
                    name = ""test""
                    values = [ 1 ; 2 ; 3 ]
                    nested =
                        {|
                            deep = true
                        |}
                }
        }

    let windowsLineEndings () =
        expect {
            snapshot ""Line 1\r\nLine 2\r\nLine 3""
            return ""crlf""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 43 "get rid of it all"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Complex nested JSON with Unicode`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "MultilineComplex.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let veryLongMultiline () =
        expect {
            snapshot
                """"""Line 1
Line 2
Line 3
Line 4
Line 5
Line 6
Line 7
Line 8
Line 9
Line 10
    Indented line 11
        More indented line 12
Line 13
Line 14
Line 15""""""

            return ""long""
        }

    let multilineWithEmptyLines () =
        expect {
            snapshot
                @""First line

Third line


Sixth line""

            return ""empty lines""
        }

    let multilineWithSpecialChars () =
        expect {
            snapshot
                """"""Special chars:
    Tab:	here
    Quotes: ""double"" and 'single'
    Backslash: \ and \\
    Unicode: 🎯
    Regex: .*+?[]""""""

            return ""special""
        }

    let multilineJson () =
        expect {
            snapshotJson
                @""wheeeee""

            return
                {
                    name = ""test""
                    values = [ 1 ; 2 ; 3 ]
                    nested =
                        {|
                            deep = true
                        |}
                }
        }

    let windowsLineEndings () =
        expect {
            snapshot ""Line 1\r\nLine 2\r\nLine 3""
            return ""crlf""
        }
"

            return SnapshotUpdate.updateSnapshotAtLine source 56 "wheeeee" |> String.concat "\n"
        }

    [<Test>]
    let ``Windows line endings`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "MultilineComplex.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let veryLongMultiline () =
        expect {
            snapshot
                """"""Line 1
Line 2
Line 3
Line 4
Line 5
Line 6
Line 7
Line 8
Line 9
Line 10
    Indented line 11
        More indented line 12
Line 13
Line 14
Line 15""""""

            return ""long""
        }

    let multilineWithEmptyLines () =
        expect {
            snapshot
                @""First line

Third line


Sixth line""

            return ""empty lines""
        }

    let multilineWithSpecialChars () =
        expect {
            snapshot
                """"""Special chars:
    Tab:	here
    Quotes: ""double"" and 'single'
    Backslash: \ and \\
    Unicode: 🎯
    Regex: .*+?[]""""""

            return ""special""
        }

    let multilineJson () =
        expect {
            snapshotJson
                @""{
  """"name"""": """"test"""",
  """"values"""": [
    1,
    2,
    3
  ],
  """"nested"""": {
    """"deep"""": true
  }
}""

            return
                {
                    name = ""test""
                    values = [ 1 ; 2 ; 3 ]
                    nested =
                        {|
                            deep = true
                        |}
                }
        }

    let windowsLineEndings () =
        expect {
            snapshot ""down with line endings""
            return ""crlf""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 82 "down with line endings"
                |> String.concat "\n"
        }
