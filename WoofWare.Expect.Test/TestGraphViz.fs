namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
module TestGraphViz =

    [<Test>]
    let ``Basic dotfile`` () =
        let s =
            """digraph G {
  rankdir = TB
  bgcolor = transparent
    n2 [shape=box label="{{n2|Map|height=1}}" ]
    n1 [shape=box label="{{n1|Const|height=0}}" ]
  n1 -> n2
}"""

        expect {
            snapshot
                @"
┌───────────────────────┐
│ {{n1|Const|height=0}} │
└───────────────────────┘
  │
  │
  ▼
┌───────────────────────┐
│  {{n2|Map|height=1}}  │
└───────────────────────┘
"

            return GraphViz.render s
        }
