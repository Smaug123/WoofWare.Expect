namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            return
                try
                    snapshot "original"
                    123
                with _ ->
                    456
        }
