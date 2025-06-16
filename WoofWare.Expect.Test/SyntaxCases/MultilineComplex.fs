namespace BigExample

open WoofWare.Expect

module MyModule =
    let veryLongMultiline () =
        expect {
            snapshot
                """Line 1
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
Line 15"""

            return "long"
        }

    let multilineWithEmptyLines () =
        expect {
            snapshot
                @"First line

Third line


Sixth line"

            return "empty lines"
        }

    let multilineWithSpecialChars () =
        expect {
            snapshot
                """Special chars:
    Tab:	here
    Quotes: "double" and 'single'
    Backslash: \ and \\
    Unicode: 🎯
    Regex: .*+?[]"""

            return "special"
        }

    let multilineJson () =
        expect {
            snapshotJson
                @"{
  ""name"": ""test"",
  ""values"": [
    1,
    2,
    3
  ],
  ""nested"": {
    ""deep"": true
  }
}"

            return
                {
                    name = "test"
                    values = [ 1 ; 2 ; 3 ]
                    nested =
                        {|
                            deep = true
                        |}
                }
        }

    let windowsLineEndings () =
        expect {
            // This would have \r\n in a real Windows file
            snapshot "Line 1\r\nLine 2\r\nLine 3"
            return "crlf"
        }
