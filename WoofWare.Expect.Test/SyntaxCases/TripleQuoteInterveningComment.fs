namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot (* comment *)
                """test
"""

            return 123
        }
