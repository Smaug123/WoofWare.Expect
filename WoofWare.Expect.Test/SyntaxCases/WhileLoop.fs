namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        let mutable i = 0

        expect {
            while i < 10 do
                snapshotList [ "original" ]
                i <- i + 1

            return [ 1 ; 2 ; 3 ]
        }
