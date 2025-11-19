namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            return
                try
                    snapshotList [ "original" ]
                    [ 1 ; 2 ; 3 ]
                finally
                    printfn "cleanup"
        }
