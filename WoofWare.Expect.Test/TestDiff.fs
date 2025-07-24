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

        let diff = Diff.patienceLines textA textB

        expect {
            snapshot
                @"    0   0  The quick brown fox
+       1  Some unique line here
    1   2  jumps over
    2   3  the lazy dog
-   3      Some unique line here
+       4  Another line
    4   5  The end"

            withFormat Diff.formatWithLineNumbers
            return diff
        }

        expect {
            snapshot
                @"  The quick brown fox
+ Some unique line here
  jumps over
  the lazy dog
- Some unique line here
+ Another line
  The end"

            withFormat Diff.format
            return diff
        }

    [<Test>]
    let ``An example from Incremental`` () =
        let textA =
            """digraph G {
  rankdir = TB
  bgcolor = transparent
    n4 [shape=Mrecord label="{{n4|BindMain|height=2}}"  "fontname"="Sans Serif"]
    n3 [shape=Mrecord label="{{n3|BindLhsChange|height=1}}"  "fontname"="Sans Serif"]
    n1 [shape=Mrecord label="{{n1|Const|height=0}}"  "fontname"="Sans Serif"]
    n2 [shape=Mrecord label="{{n2|Const|height=0}}"  "fontname"="Sans Serif"]
  n3 -> n4
  n2 -> n4
  n1 -> n3
}"""

        let textB =
            """digraph G {
  rankdir = TB
  bgcolor = transparent
    n4 [shape=box label="{{n4|BindMain|height=2}}" ]
    n3 [shape=box label="{{n3|BindLhsChange|height=1}}" ]
    n1 [shape=box label="{{n1|Const|height=0}}" ]
    n2 [shape=box label="{{n2|Const|height=0}}" ]
  n3 -> n4
  n2 -> n4
  n1 -> n3
}"""

        let diff = Diff.patience textA textB

        expect {
            snapshot
                @"  digraph G {
    rankdir = TB
    bgcolor = transparent
-     n4 [shape=Mrecord label=""{{n4|BindMain|height=2}}""  ""fontname""=""Sans Serif""]
-     n3 [shape=Mrecord label=""{{n3|BindLhsChange|height=1}}""  ""fontname""=""Sans Serif""]
-     n1 [shape=Mrecord label=""{{n1|Const|height=0}}""  ""fontname""=""Sans Serif""]
-     n2 [shape=Mrecord label=""{{n2|Const|height=0}}""  ""fontname""=""Sans Serif""]
+     n4 [shape=box label=""{{n4|BindMain|height=2}}"" ]
+     n3 [shape=box label=""{{n3|BindLhsChange|height=1}}"" ]
+     n1 [shape=box label=""{{n1|Const|height=0}}"" ]
+     n2 [shape=box label=""{{n2|Const|height=0}}"" ]
    n3 -> n4
    n2 -> n4
    n1 -> n3
  }"

            withFormat Diff.format
            return diff
        }
