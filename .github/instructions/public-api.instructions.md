---
applyTo: "src/Polly/**,src/Polly.Core/**,src/Polly.Extensions/**,src/Polly.RateLimiting/**,src/Polly.Testing/**"
---

# Public API

Public API changes are a **blocking** review gate. `AGENTS.md` states that the public API must not change unless specifically requested.

- Every library project tracks its public API with the public API analyzers in `.PublicAPI/PublicAPI.Shipped.txt` and `.PublicAPI/PublicAPI.Unshipped.txt`. New API must be added to `PublicAPI.Unshipped.txt`. `PublicAPI.Shipped.txt` must never be edited by a PR, as it is only updated as part of a release.
- NuGet package validation runs against a baseline version (`PackageValidationBaselineVersion` in `eng/Library.targets`). A new or changed entry in a `CompatibilitySuppressions.xml` file indicates a binary or source breaking change and is an error unless a maintainer has explicitly approved the break.
- For any public API change, check that:
  1. There is a linked issue where the maintainers agreed to the change. If there isn't one, the change needs human review.
  2. The change is additive. Removing or renaming members, changing parameter types or order, adding members to public interfaces or abstract members to public classes, or sealing/unsealing types are breaking changes.
  3. The new API follows the shape of the existing API. For example, a new strategy option lives on the relevant `*StrategyOptions` type with a sensible default, delegates take a single `readonly struct` arguments type (e.g. `OnRetryArguments<TResult>`) so that parameters can be added later without breaking changes, and callbacks return `ValueTask`.
  4. Generic and non-generic variants, and sync and async overloads, are consistent where the existing API provides both.
  5. The new API has XML documentation, tests, and where appropriate, documentation under `docs/`.
- Behavioral changes to existing API are breaking changes too, even when no signature changes. Examples include changing default option values, which outcomes are handled by default, the exception types thrown, or the name, severity, order or number of telemetry events.
- Internal types are visible to the test projects through `InternalsVisibleTo`. Do not make something `public` just so that it can be tested.
