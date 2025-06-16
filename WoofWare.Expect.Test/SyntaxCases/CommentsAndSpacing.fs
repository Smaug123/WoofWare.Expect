namespace BigExample

open WoofWare.Expect

module MyModule =
    let multipleComments () =
        expect {
            snapshot (* first comment *) (* second comment *)
                (* third comment on new line *)
                @"test with many comments"

            return 123
        }

    let nestedComments () =
        expect {
            snapshot (* outer (* inner *) comment *) """nested comment test"""
            return "nested"
        }

    let commentWithSpecialChars () =
        expect {
            snapshot (* comment with "quotes" and \ backslash *) "regular string"
            return "special"
        }

    let lotsOfWhitespace () =
        expect {
            snapshot


                "string after whitespace"

            return "whitespace"
        }

    let mixedWhitespaceAndComments () =
        expect {
            snapshotJson (* comment 1 *)
                (* comment 2 *)
                (* comment 3 *) @"123"

            return 123
        }
