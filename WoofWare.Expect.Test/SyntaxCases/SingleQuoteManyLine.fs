namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot
                "test
with
newlines"

            return 123
        }
