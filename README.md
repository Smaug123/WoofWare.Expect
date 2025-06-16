# WoofWare.Expect

[![NuGet version](https://img.shields.io/nuget/v/WoofWare.Expect.svg?style=flat-square)](https://www.nuget.org/packages/WoofWare.Expect)
[![GitHub Actions status](https://github.com/Smaug123/WoofWare.Expect/actions/workflows/dotnet.yaml/badge.svg)](https://github.com/Smaug123/WoofWare.Expect/actions?query=branch%3Amain)
[![License file](https://img.shields.io/github/license/Smaug123/WoofWare.Expect)](./LICENSE)

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="logos/logo-dark-background.png">
  <source media="(prefers-color-scheme: light)" srcset="logos/logo-light-background.png">
  <img alt="Project logo: minimalistic face of a cartoon Shiba Inu; one eye is a camera aperture." src="logos/logo-light-background.png">
</picture>

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

## Updating an individual snapshot

If a snapshot is failing, add a `'` to the `expect` builder and rerun.
The rerun will throw, but it will update the snapshot; then remove the `'` again to put the test back into "assert snapshot" mode.

```fsharp
[<Test>]
let ``Example of automatically updating`` () =
    // This test fails...
    expect {
        snapshotJson "123"
        return 124
    }

    // so make this change:
    expect' {
        snapshotJson "123"
        return 124
    }

    // and rerunning converts the result to this:
    expect' {
        snapshotJson @"124"
        return 124
    }

    // That test will always throw, because it's not in "assertion" mode but in "update" mode;
    // so finally, remove the `'` again. This test now passes!
    expect {
        snapshotJson @"124"
        return 124
    }
```

## Bulk update of snapshots

*Warning*: when doing this, you should probably make sure your test fixture is `[<NonParallelizable>]` (or the equivalent in your test runner of choice).
Otherwise, the global state used by this mechanism may interfere with other fixtures.

You can put WoofWare.Expect into "bulk update" mode as follows:

```fsharp
open NUnit.Framework
open WoofWare.Expect

[<TestFixture>]
[<NonParallelizable>]
module BulkUpdateExample =

    [<OneTimeSetUp>]
    let ``Prepare to bulk-update tests`` () =
        // If you don't want to enter bulk-update mode, just replace this line with a no-op `()`.
        // The `updateAllSnapshots` tear-down below will simply do nothing in that case.
        GlobalBuilderConfig.enterBulkUpdateMode ()

    [<OneTimeTearDown>]
    let ``Update all tests`` () =
        GlobalBuilderConfig.updateAllSnapshots ()

    [<Test>]
    let ``Snapshot 2`` () =
        // this snapshot fails: the "expected" isn't even JSON!
        expect {
            snapshotJson ""

            return Map.ofList [ "1", "hi" ; "2", "my" ; "3", "name" ; "4", "is" ]
        }

    [<Test>]
    let ``Snapshot 1`` () =
        // this snapshot fails: the "expected" is not equal to the "actual"
        expect {
            snapshotJson @"124"
            return 123
        }
```

Observe the `OneTimeSetUp` which sets global state to enter "bulk update" mode, and the `OneTimeTearDown` which performs all the updates to rectify failures which were accumulated during this test run.

# Limitations

* The snapshot updating mechanism *requires* you to use verbatim string literals. While the test assertions will work correctly if you do `snapshot ("foo" + "bar" + f 3)`, for example, the updating code is liable to do something undefined in that case. Also do not use format strings (`$"blah"`).

# Licence

MIT.
