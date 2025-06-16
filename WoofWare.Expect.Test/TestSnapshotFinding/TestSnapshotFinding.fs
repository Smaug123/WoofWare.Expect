namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
[<Parallelizable(ParallelScope.Children)>]
module TestSnapshotFinding =
    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // GlobalBuilderConfig.enterBulkUpdateMode ()
        ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    type Dummy = class end

    [<Test>]
    let ``Triple-quote, one line, one-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @""replacement""
            return 123
        }
"

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``Triple-quote, one line, multi-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @""replacement
more""
            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "replacement\nmore"
                |> String.concat "\n"
        }

    [<Test>]
    let ``At-string, one line, one-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "AtStringOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @""replacement""
            return 123
        }
"

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``At-string, one line, multi-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "AtStringOneLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @""replacement
more""
            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "replacement\nmore"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Triple-quote, intervening comment, one-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteInterveningComment.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot (* comment *)
                @""replacement""

            return 123
        }
"

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``Triple-quote, intervening comment, multi-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "TripleQuoteInterveningComment.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot (* comment *)
                @""replacement
more""

            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "replacement\nmore"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Single-quote, many lines, one-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "SingleQuoteManyLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot
                @""replacement""

            return 123
        }
"

            return SnapshotUpdate.updateSnapshotAtLine source 8 "replacement" |> String.concat "\n"
        }

    [<Test>]
    let ``Single-quote, many lines, multi-line replacement`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "SingleQuoteManyLine.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot
                @""replacement
more""

            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "replacement\nmore"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Regex metacharacters in verbatim string`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "RegexMetacharacters.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let regexChars () =
        expect {
            snapshot @""replacement with .*+?[]{}()|^$\ chars""
            return 123
        }

    let regexInTripleQuote () =
        expect {
            snapshot """"""regex: .*+?[]{}()|^$\ in triple quotes""""""
            return 456
        }

    let regexInRegularString () =
        expect {
            snapshot ""escaped regex: \\.\\*\\+\\?\\[\\]\\{\\}\\(\\)\\|\\^\\$\\\\""
            return 789
        }

    let complexRegexPattern () =
        expect {
            snapshotJson @""^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$""
            return ""IP regex""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "replacement with .*+?[]{}()|^$\\ chars"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Regex metacharacters in triple quote`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "RegexMetacharacters.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let regexChars () =
        expect {
            snapshot @""test with regex chars: .*+?[]{}()|^$\ and more""
            return 123
        }

    let regexInTripleQuote () =
        expect {
            snapshot @""new regex: [a-z]+(?:\d{2,4})? pattern""
            return 456
        }

    let regexInRegularString () =
        expect {
            snapshot ""escaped regex: \\.\\*\\+\\?\\[\\]\\{\\}\\(\\)\\|\\^\\$\\\\""
            return 789
        }

    let complexRegexPattern () =
        expect {
            snapshotJson @""^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$""
            return ""IP regex""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 14 "new regex: [a-z]+(?:\\d{2,4})? pattern"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Multiple comments between snapshot and string`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "CommentsAndSpacing.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let multipleComments () =
        expect {
            snapshot (* first comment *) (* second comment *)
                (* third comment on new line *)
                @""updated after many comments""

            return 123
        }

    let nestedComments () =
        expect {
            snapshot (* outer (* inner *) comment *) """"""nested comment test""""""
            return ""nested""
        }

    let commentWithSpecialChars () =
        expect {
            snapshot (* comment with ""quotes"" and \ backslash *) ""regular string""
            return ""special""
        }

    let lotsOfWhitespace () =
        expect {
            snapshot


                ""string after whitespace""

            return ""whitespace""
        }

    let mixedWhitespaceAndComments () =
        expect {
            snapshotJson (* comment 1 *)
                (* comment 2 *)
                (* comment 3 *) @""123""

            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "updated after many comments"
                |> String.concat "\n"
        }
