namespace BigExample

open WoofWare.Expect

module MyModule =
    let emptyString () =
        expect {
            snapshot ""
            return ""
        }

    let emptyVerbatim () =
        expect {
            snapshot @""
            return ""
        }

    let emptyTripleQuote () =
        expect {
            snapshot """"""
            return ""
        }

    let onlyWhitespace () =
        expect {
            snapshot "   \t\n   "
            return "whitespace"
        }

    let quotesInQuotes () =
        expect {
            snapshot @"He said ""Hello"" and she said ""Hi"""
            return "quotes"
        }

    let tripleQuotesInContent () =
        expect {
            snapshot """This has """ """ in the middle"""
            return "triple"
        }

    let backslashesGalore () =
        expect {
            snapshot @"C:\Users\Test\Documents\file.txt"
            return "path"
        }

    let veryLongLine () =
        expect {
            snapshot
                @"This is a very long line that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and contains over 300 characters to test how the parser handles very long single-line strings"

            return "long line"
        }

    let leadingNewlines () =
        expect {
            snapshot
                """

Starts with newlines"""

            return "leading"
        }

    let trailingNewlines () =
        expect {
            snapshot
                """Ends with newlines


"""

            return "trailing"
        }
