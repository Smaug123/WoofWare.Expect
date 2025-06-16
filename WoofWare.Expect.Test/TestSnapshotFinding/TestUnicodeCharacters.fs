namespace WoofWare.Expect.Test

open NUnit.Framework
open WoofWare.Expect

[<TestFixture>]
module TestUnicodeCharacters =
    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // GlobalBuilderConfig.enterBulkUpdateMode ()
        ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    type Dummy = class end

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
    let ``Arabic RTL`` () =
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
            snapshot """"""Chinese: 你好世界""""""
            return ""hello""
        }

    let arabicRTL () =
        expect {
            snapshot @""Updated Arabic: مرحبا بالعالم""
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
                SnapshotUpdate.updateSnapshotAtLine source 20 "Updated Arabic: مرحبا بالعالم"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Combining characters`` () =
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
            snapshot @""updated test with combining: é and ä!""
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
                SnapshotUpdate.updateSnapshotAtLine source 27 "updated test with combining: e\u0301 and a\u0308!"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Mixed scripts`` () =
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
            snapshotJson @""Updated mixed: English, русский, 日本語, العربية, emoji 🚀""
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
                SnapshotUpdate.updateSnapshotAtLine source 33 "Updated mixed: English, русский, 日本語, العربية, emoji 🚀"
                |> String.concat "\n"
        }

    [<Test>]
    let ``ZWBS character`` () =
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
            snapshot @""Updated: Zero​width​space​test"" // Contains U+200B
            return ""zwsp""
        }

    let mathSymbols () =
        expect {
            snapshot """"""Math: ∀x∈ℝ, ∃y: x² + y² = 1 ⟹ |x| ≤ 1""""""
            return ""math""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 39 "Updated: Zero​width​space​test"
                |> String.concat "\n"
        }

    [<Test>]
    let ``Maths`` () =
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
            snapshot @""Pretty vacuous, huh: ∀x∈ℝ, ∃y: x² + y² = 1 ⟹ |x| ≤ 1""
            return ""math""
        }
"

            return
                SnapshotUpdate.updateSnapshotAtLine source 45 "Pretty vacuous, huh: ∀x∈ℝ, ∃y: x² + y² = 1 ⟹ |x| ≤ 1"
                |> String.concat "\n"
        }
