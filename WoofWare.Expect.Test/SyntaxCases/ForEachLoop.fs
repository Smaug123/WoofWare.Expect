namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        let items = [ 1 ; 2 ; 3 ]

        expect {
            for item in items do
                snapshotList [ "original" ]
                printfn "%d" item

            return [ 1 ; 2 ; 3 ]
        }
