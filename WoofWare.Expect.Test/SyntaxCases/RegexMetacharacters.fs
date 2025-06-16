namespace BigExample

open WoofWare.Expect

module MyModule =
    let regexChars () =
        expect {
            snapshot @"test with regex chars: .*+?[]{}()|^$\ and more"
            return 123
        }

    let regexInTripleQuote () =
        expect {
            snapshot """regex: .*+?[]{}()|^$\ in triple quotes"""
            return 456
        }

    let regexInRegularString () =
        expect {
            snapshot "escaped regex: \\.\\*\\+\\?\\[\\]\\{\\}\\(\\)\\|\\^\\$\\\\"
            return 789
        }

    let complexRegexPattern () =
        expect {
            snapshotJson @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"
            return "IP regex"
        }
