namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            for i in 1..10 do
                snapshot "original"
                printfn "%d" i

            return 123
        }
