namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
[<Parallelizable(ParallelScope.Children)>]
module TestRegexMetacharacters =
    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // GlobalBuilderConfig.enterBulkUpdateMode ()
        ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    type Dummy = class end

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
    let ``Regex metacharacters in regular string`` () =
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
            snapshot """"""regex: .*+?[]{}()|^$\ in triple quotes""""""
            return 456
        }

    let regexInRegularString () =
        expect {
            snapshot @""new regex: [a-z]+(?:\d{2,4})? pattern""
            return 789
        }

    let complexRegexPattern () =
        expect {
            snapshotJson @""^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$""
            return ""IP regex""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 20 "new regex: [a-z]+(?:\\d{2,4})? pattern"
                |> String.concat "\n"
        }

    [<Test>]
    let ``IP regex`` () =
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
            snapshotJson @""new regex: [a-z]+(?:\d{2,4})? pattern""
            return ""IP regex""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 26 "new regex: [a-z]+(?:\\d{2,4})? pattern"
                |> String.concat "\n"
        }
