namespace BigExample

open WoofWare.Expect

module MyModule =
    let foo () =
        expect {
            return
                seq {
                    snapshotList [ "original" ]
                    yield 1
                    yield 2
                    yield 3
                }
        }
