# WoofWare.Expect

An [expect-testing](https://blog.janestreet.com/the-joy-of-expect-tests/) library for F#.
(Also known as "snapshot testing".)

# Current status

The basic mechanism works.
Snapshot updating is vibe-coded with Opus 4 and is purely text-based; I didn't want to use the F# compiler services because that's a pretty heavyweight dependency which should be confined to a separate test runner entity.
It's not very well tested, and I expect it to be kind of brittle.

# How to use

See [the tests](./WoofWare.Expect.Test/SimpleTest.fs).

```fsharp
[<Test>]
let ``This test fails: JSON documents are not equal`` () =
    expect {
        snapshotJson "123"
        return 124
    }

[<Test>]
let ``This test passes: JSON documents are equal`` () =
    expect {
        snapshotJson "  123  "
        return 123
    }

[<Test>]
let ``This test fails: plain text comparison of ToString`` () =
    expect {
        snapshot "  123  "
        return 123
    }
```

# Limitations

* The snapshot updating mechanism *requires* you to use verbatim string literals. While the test assertions will work correctly if you do `snapshot ("foo" + "bar" + f 3)`, for example, the updating code is liable to do something undefined in that case.

# Licence

MIT.
