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
    let ``Unicode emoji in string`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "UnicodeCharacters.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emoji () =
        expect {
            snapshot @""Updated with 🚀🌟✨ more emoji!""
            return 123
        }

    let chineseCharacters () =
        expect {
            snapshot """"""Chinese: 你好世界""""""
            return ""hello""
        }

    let arabicRTL () =
        expect {
            snapshot @""Arabic RTL: مرحبا بالعالم""
            return ""rtl test""
        }

    let combiningCharacters () =
        expect {
            // Combining diacritics: e + ́ = é
            snapshot ""test with combining: e\u0301 and a\u0308""
            return ""combining""
        }

    let mixedScripts () =
        expect {
            snapshotJson @""Mixed: English, русский, 日本語, العربية, emoji 🚀""
            return [ ""multilingual"" ]
        }

    let zeroWidthChars () =
        expect {
            snapshot @""Zero​width​space​test"" // Contains U+200B
            return ""zwsp""
        }

    let mathSymbols () =
        expect {
            snapshot """"""Math: ∀x∈ℝ, ∃y: x² + y² = 1 ⟹ |x| ≤ 1""""""
            return ""math""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "Updated with 🚀🌟✨ more emoji!"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Unicode Chinese characters multi-line`` () =
        let source =
            Assembly.getEmbeddedResource typeof<Dummy>.Assembly "UnicodeCharacters.fs"
            |> _.Split('\n')

        expect {
            snapshot
                @"namespace BigExample

open WoofWare.Expect

module MyModule =
    let emoji () =
        expect {
            snapshot @""Hello 👋 World 🌍 with emoji 🎉🎊""
            return 123
        }

    let chineseCharacters () =
        expect {
            snapshot @""Chinese poem:
静夜思
床前明月光
疑是地上霜
举头望明月
低头思故乡""
            return ""hello""
        }

    let arabicRTL () =
        expect {
            snapshot @""Arabic RTL: مرحبا بالعالم""
            return ""rtl test""
        }

    let combiningCharacters () =
        expect {
            // Combining diacritics: e + ́ = é
            snapshot ""test with combining: e\u0301 and a\u0308""
            return ""combining""
        }

    let mixedScripts () =
        expect {
            snapshotJson @""Mixed: English, русский, 日本語, العربية, emoji 🚀""
            return [ ""multilingual"" ]
        }

    let zeroWidthChars () =
        expect {
            snapshot @""Zero​width​space​test"" // Contains U+200B
            return ""zwsp""
        }

    let mathSymbols () =
        expect {
            snapshot """"""Math: ∀x∈ℝ, ∃y: x² + y² = 1 ⟹ |x| ≤ 1""""""
            return ""math""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 14 "Chinese poem:\n静夜思\n床前明月光\n疑是地上霜\n举头望明月\n低头思故乡"
                |> String.concat "\n"
        }

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
            // This would have \r\n in a real Windows file
            snapshot ""Line 1\r\nLine 2\r\nLine 3""
            return ""crlf""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 8 "Replaced with\na different\nmultiline\nvalue"
                |> String.concat "\n"
        }

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
            snapshot @""He said """"Hello"""" and she said """"Hi""""""
            return ""quotes""
        }

    let backslashesGalore () =
        expect {
            snapshot @""Updated: He said """"What's up?"""" and replied """"Nothing much.""""""
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
                    34
                    "Updated: He said \"What's up?\" and replied \"Nothing much.\""
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

    [<Test>]
    let ``String with only newlines and special escapes`` () =
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
            snapshot @""

Just newlines!


""
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
                SnapshotUpdate.updateSnapshotAtLine source 28 "\n\nJust newlines!\n\n\n"
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
                @""{
  """"name"""": """"test with émojis 🎯"""",
  """"unicode"""": """"你好世界"""",
  """"rtl"""": """"مرحبا"""",
  """"math"""": """"∀x∈ℝ: x² ≥ 0""""
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
            // This would have \r\n in a real Windows file
            snapshot ""Line 1\r\nLine 2\r\nLine 3""
            return ""crlf""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine
                    source
                    55
                    "{\n  \"name\": \"test with émojis 🎯\",\n  \"unicode\": \"你好世界\",\n  \"rtl\": \"مرحبا\",\n  \"math\": \"∀x∈ℝ: x² ≥ 0\"\n}"
                |> String.concat "\n"
        }
