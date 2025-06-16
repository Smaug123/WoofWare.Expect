namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot """test"""
            return 123
        }
