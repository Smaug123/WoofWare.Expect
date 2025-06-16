namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            snapshot @"test ""quotes"" here"
            return 123
        }
