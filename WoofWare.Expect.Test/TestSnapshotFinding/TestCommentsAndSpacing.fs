namespace WoofWare.Expect.Test

open WoofWare.Expect
open NUnit.Framework

[<TestFixture>]
[<Parallelizable(ParallelScope.Children)>]
module TestCommentsAndSpacing =
    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // GlobalBuilderConfig.enterBulkUpdateMode ()
        ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    type Dummy = class end

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
                ""updated after many comments""

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
    let ``Nested comments`` () =
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
                @""test with many comments""

            return 123
        }

    let nestedComments () =
        expect {
            snapshot (* outer (* inner *) comment *) ""updated after nested comments""
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
                SnapshotUpdate.updateSnapshotAtLine source 17 "updated after nested comments"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Comment with special chars`` () =
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
                @""test with many comments""

            return 123
        }

    let nestedComments () =
        expect {
            snapshot (* outer (* inner *) comment *) """"""nested comment test""""""
            return ""nested""
        }

    let commentWithSpecialChars () =
        expect {
            snapshot (* comment with ""quotes"" and \ backslash *) ""updated after weird comment""
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
                SnapshotUpdate.updateSnapshotAtLine source 23 "updated after weird comment"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Whitespace before`` () =
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
                @""test with many comments""

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


                ""updated after spaces""

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
                SnapshotUpdate.updateSnapshotAtLine source 29 "updated after spaces"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Mixed whitespace and comments`` () =
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
                @""test with many comments""

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
                (* comment 3 *) ""updated after comments""

            return 123
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 39 "updated after comments"
                |> String.concat "\n"
        }
