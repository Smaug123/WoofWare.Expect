# WoofWare.Expect

An [expect-testing](https://blog.janestreet.com/the-joy-of-expect-tests/) library for F#.
(Also known as "snapshot testing".)

# Current status

Basic mechanism works, but I haven't yet decided how the ergonomic updating of the input text will work.
Ideally it would edit the input AST, but I don't yet know if that's viable.

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

# Licence

MIT.
