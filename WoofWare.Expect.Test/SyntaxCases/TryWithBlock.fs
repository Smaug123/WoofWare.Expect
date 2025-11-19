namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            return
                try
                    snapshotList [ "original" ]
                    [ 1 ; 2 ; 3 ]
                with _ ->
                    [ 4 ; 5 ; 6 ]
        }
