namespace WoofWare.Expect.Test

open NUnit.Framework
open WoofWare.Expect

[<TestFixture>]
module TestEdgeCases =
    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // GlobalBuilderConfig.enterBulkUpdateMode ()
        ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    type Dummy = class end

    [<Test>]
    let ``Empty string replacements`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "EdgeCases.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot @""now has content""
            return """"
        }

    let emptyVerbatim () =
        expect {
            snapshot @""""
            return """"
        }

    let emptyTripleQuote () =
        expect {
            snapshot """"""""""""
            return """"
        }

    let onlyWhitespace () =
        expect {
            snapshot ""   \t\n   ""
            return ""whitespace""
        }

    let quotesInQuotes () =
        expect {
            snapshot @""He said """"Hello"""" and she said """"Hi""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""C:\Users\Test\Documents\file.txt""
            return ""path""
        }

    let veryLongLine () =
        expect {
            snapshot
                @""This is a very long line that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and contains over 300 characters to test how the parser handles very long single-line strings""

            return ""long line""
        }

    let leadingNewlines () =
        expect {
            snapshot
                """"""

Starts with newlines""""""

            return ""leading""
        }

    let trailingNewlines () =
        expect {
            snapshot
                """"""Ends with newlines


""""""

            return ""trailing""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "now has content"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Empty string replacements, verbatim`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "EdgeCases.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot """"
            return """"
        }

    let emptyVerbatim () =
        expect {
            snapshot @""now has content""
            return """"
        }

    let emptyTripleQuote () =
        expect {
            snapshot """"""""""""
            return """"
        }

    let onlyWhitespace () =
        expect {
            snapshot ""   \t\n   ""
            return ""whitespace""
        }

    let quotesInQuotes () =
        expect {
            snapshot @""He said """"Hello"""" and she said """"Hi""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""C:\Users\Test\Documents\file.txt""
            return ""path""
        }

    let veryLongLine () =
        expect {
            snapshot
                @""This is a very long line that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and contains over 300 characters to test how the parser handles very long single-line strings""

            return ""long line""
        }

    let leadingNewlines () =
        expect {
            snapshot
                """"""

Starts with newlines""""""

            return ""leading""
        }

    let trailingNewlines () =
        expect {
            snapshot
                """"""Ends with newlines


""""""

            return ""trailing""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 14 "now has content"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Empty string replacements, triple quotes`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "EdgeCases.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot """"
            return """"
        }

    let emptyVerbatim () =
        expect {
            snapshot @""""
            return """"
        }

    let emptyTripleQuote () =
        expect {
            snapshot @""now has content""
            return """"
        }

    let onlyWhitespace () =
        expect {
            snapshot ""   \t\n   ""
            return ""whitespace""
        }

    let quotesInQuotes () =
        expect {
            snapshot @""He said """"Hello"""" and she said """"Hi""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""C:\Users\Test\Documents\file.txt""
            return ""path""
        }

    let veryLongLine () =
        expect {
            snapshot
                @""This is a very long line that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and contains over 300 characters to test how the parser handles very long single-line strings""

            return ""long line""
        }

    let leadingNewlines () =
        expect {
            snapshot
                """"""

Starts with newlines""""""

            return ""leading""
        }

    let trailingNewlines () =
        expect {
            snapshot
                """"""Ends with newlines


""""""

            return ""trailing""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 20 "now has content"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Empty string replacements, only whitespace`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "EdgeCases.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot """"
            return """"
        }

    let emptyVerbatim () =
        expect {
            snapshot @""""
            return """"
        }

    let emptyTripleQuote () =
        expect {
            snapshot """"""""""""
            return """"
        }

    let onlyWhitespace () =
        expect {
            snapshot @""now has content""
            return ""whitespace""
        }

    let quotesInQuotes () =
        expect {
            snapshot @""He said """"Hello"""" and she said """"Hi""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""C:\Users\Test\Documents\file.txt""
            return ""path""
        }

    let veryLongLine () =
        expect {
            snapshot
                @""This is a very long line that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and contains over 300 characters to test how the parser handles very long single-line strings""

            return ""long line""
        }

    let leadingNewlines () =
        expect {
            snapshot
                """"""

Starts with newlines""""""

            return ""leading""
        }

    let trailingNewlines () =
        expect {
            snapshot
                """"""Ends with newlines


""""""

            return ""trailing""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 26 "now has content"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Quotes in quotes handling`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "EdgeCases.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot """"
            return """"
        }

    let emptyVerbatim () =
        expect {
            snapshot @""""
            return """"
        }

    let emptyTripleQuote () =
        expect {
            snapshot """"""""""""
            return """"
        }

    let onlyWhitespace () =
        expect {
            snapshot ""   \t\n   ""
            return ""whitespace""
        }

    let quotesInQuotes () =
        expect {
            snapshot @""Updated: He said """"What's up?"""" and replied """"Nothing much.""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""C:\Users\Test\Documents\file.txt""
            return ""path""
        }

    let veryLongLine () =
        expect {
            snapshot
                @""This is a very long line that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and contains over 300 characters to test how the parser handles very long single-line strings""

            return ""long line""
        }

    let leadingNewlines () =
        expect {
            snapshot
                """"""

Starts with newlines""""""

            return ""leading""
        }

    let trailingNewlines () =
        expect {
            snapshot
                """"""Ends with newlines


""""""

            return ""trailing""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine
                    source
                    32
                    "Updated: He said \"What's up?\" and replied \"Nothing much.\""
                |> String.concat "\n"
        }

    [<Test>]
    let ``Backslashes galore`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "EdgeCases.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot """"
            return """"
        }

    let emptyVerbatim () =
        expect {
            snapshot @""""
            return """"
        }

    let emptyTripleQuote () =
        expect {
            snapshot """"""""""""
            return """"
        }

    let onlyWhitespace () =
        expect {
            snapshot ""   \t\n   ""
            return ""whitespace""
        }

    let quotesInQuotes () =
        expect {
            snapshot @""He said """"Hello"""" and she said """"Hi""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""prefer\these\ones""
            return ""path""
        }

    let veryLongLine () =
        expect {
            snapshot
                @""This is a very long line that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and contains over 300 characters to test how the parser handles very long single-line strings""

            return ""long line""
        }

    let leadingNewlines () =
        expect {
            snapshot
                """"""

Starts with newlines""""""

            return ""leading""
        }

    let trailingNewlines () =
        expect {
            snapshot
                """"""Ends with newlines


""""""

            return ""trailing""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 38 "prefer\\these\\ones"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Very long line`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "EdgeCases.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot """"
            return """"
        }

    let emptyVerbatim () =
        expect {
            snapshot @""""
            return """"
        }

    let emptyTripleQuote () =
        expect {
            snapshot """"""""""""
            return """"
        }

    let onlyWhitespace () =
        expect {
            snapshot ""   \t\n   ""
            return ""whitespace""
        }

    let quotesInQuotes () =
        expect {
            snapshot @""He said """"Hello"""" and she said """"Hi""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""C:\Users\Test\Documents\file.txt""
            return ""path""
        }

    let veryLongLine () =
        expect {
            snapshot
                @""this line is short though""

            return ""long line""
        }

    let leadingNewlines () =
        expect {
            snapshot
                """"""

Starts with newlines""""""

            return ""leading""
        }

    let trailingNewlines () =
        expect {
            snapshot
                """"""Ends with newlines


""""""

            return ""trailing""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 44 "this line is short though"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Leading newlines`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "EdgeCases.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot """"
            return """"
        }

    let emptyVerbatim () =
        expect {
            snapshot @""""
            return """"
        }

    let emptyTripleQuote () =
        expect {
            snapshot """"""""""""
            return """"
        }

    let onlyWhitespace () =
        expect {
            snapshot ""   \t\n   ""
            return ""whitespace""
        }

    let quotesInQuotes () =
        expect {
            snapshot @""He said """"Hello"""" and she said """"Hi""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""C:\Users\Test\Documents\file.txt""
            return ""path""
        }

    let veryLongLine () =
        expect {
            snapshot
                @""This is a very long line that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and contains over 300 characters to test how the parser handles very long single-line strings""

            return ""long line""
        }

    let leadingNewlines () =
        expect {
            snapshot
                @""

Just newlines!


""

            return ""leading""
        }

    let trailingNewlines () =
        expect {
            snapshot
                """"""Ends with newlines


""""""

            return ""trailing""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 52 "\n\nJust newlines!\n\n\n"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Trailing newlines`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "EdgeCases.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot """"
            return """"
        }

    let emptyVerbatim () =
        expect {
            snapshot @""""
            return """"
        }

    let emptyTripleQuote () =
        expect {
            snapshot """"""""""""
            return """"
        }

    let onlyWhitespace () =
        expect {
            snapshot ""   \t\n   ""
            return ""whitespace""
        }

    let quotesInQuotes () =
        expect {
            snapshot @""He said """"Hello"""" and she said """"Hi""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""C:\Users\Test\Documents\file.txt""
            return ""path""
        }

    let veryLongLine () =
        expect {
            snapshot
                @""This is a very long line that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and contains over 300 characters to test how the parser handles very long single-line strings""

            return ""long line""
        }

    let leadingNewlines () =
        expect {
            snapshot
                """"""

Starts with newlines""""""

            return ""leading""
        }

    let trailingNewlines () =
        expect {
            snapshot
                @""

Just newlines!


""

            return ""trailing""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 62 "\n\nJust newlines!\n\n\n"
                |> String.concat "\n"
        }
