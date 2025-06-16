namespace BigExample

open WoofWare.Expect

module MyModule =
    let emoji () =
        expect {
            snapshot @"Hello 👋 World 🌍 with emoji 🎉🎊"
            return 123
        }

    let chineseCharacters () =
        expect {
            snapshot """Chinese: 你好世界"""
            return "hello"
        }

    let arabicRTL () =
        expect {
            snapshot @"Arabic RTL: مرحبا بالعالم"
            return "rtl test"
        }

    let combiningCharacters () =
        expect {
            // Combining diacritics: e + ́ = é
            snapshot "test with combining: e\u0301 and a\u0308"
            return "combining"
        }

    let mixedScripts () =
        expect {
            snapshotJson @"Mixed: English, русский, 日本語, العربية, emoji 🚀"
            return [ "multilingual" ]
        }

    let zeroWidthChars () =
        expect {
            snapshot @"Zero​width​space​test" // Contains U+200B
            return "zwsp"
        }

    let mathSymbols () =
        expect {
            snapshot """Math: ∀x∈ℝ, ∃y: x² + y² = 1 ⟹ |x| ≤ 1"""
            return "math"
        }
