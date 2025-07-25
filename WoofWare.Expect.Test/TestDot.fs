namespace WoofWare.Expect.Test

#nowarn 0044 // This construct is deprecated

open System
open FsUnitTyped
open WoofWare.Expect
open NUnit.Framework
open System.IO.Abstractions
open System.IO.Abstractions.TestingHelpers

[<TestFixture>]
module TestDot =
    let toFs (fs : IFileSystem) : Dot.IFileSystem =
        { new Dot.IFileSystem with
            member _.DeleteFile s = fs.File.Delete s
            member _.WriteFile path contents = fs.File.WriteAllText (path, contents)
            member _.GetTempFileName () = fs.Path.GetTempFileName ()
        }

    [<Test ; Explicit "requires graph-easy dependency">]
    let ``Basic dotfile, real graph-easy`` () =
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

            return Dot.render s
        }

    [<Test>]
    let ``Basic dotfile`` () =
        let fs = MockFileSystem ()

        let contents =
            """digraph G {
  rankdir = TB
  bgcolor = transparent
    n2 [shape=box label="{{n2|Map|height=1}}" ]
    n1 [shape=box label="{{n1|Const|height=0}}" ]
  n1 -> n2
}"""

        let mutable started = false
        let mutable waited = false
        let mutable disposed = false

        let expected =
            "┌───────────────────────┐
│ {{n1|Const|height=0}} │
└───────────────────────┘
  │
  │
  ▼
┌───────────────────────┐
│  {{n2|Map|height=1}}  │
└───────────────────────┘
"

        let pr =
            { new Dot.IProcess<IDisposable> with
                member _.Start _ =
                    started <- true
                    true

                member _.Create exe args =
                    exe |> shouldEqual "graph-easy"

                    args.StartsWith ("--as=boxarg --from=dot ", StringComparison.Ordinal)
                    |> shouldEqual true

                    { new IDisposable with
                        member _.Dispose () = disposed <- true
                    }

                member _.WaitForExit p = waited <- true
                member _.ReadStandardOutput _ = expected
            }

        Dot.render' pr (toFs fs) "graph-easy" contents
        |> _.TrimStart()
        |> shouldEqual expected

        started |> shouldEqual true
        waited |> shouldEqual true
        disposed |> shouldEqual true
