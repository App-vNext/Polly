---
applyTo: "src/Polly/**/*.cs,src/Polly.Core/**/*.cs,src/Polly.Extensions/**/*.cs,src/Polly.RateLimiting/**/*.cs,src/Polly.Testing/**/*.cs"
---

# Compatibility

- **Target frameworks.** Library code must compile and behave correctly on every target framework, including .NET Framework and .NET Standard 2.0. Watch for APIs that only exist in newer frameworks and are used without an `#if` guard or polyfill. Polyfills for attributes live in `src/LegacySupport`.
- **Native AoT and trimming.** `Polly.Core`, `Polly.Extensions` and `Polly.RateLimiting` are AoT compatible. Flag reflection, `dynamic`, and other trimming-unsafe patterns.
- **Legacy `Polly` package.** `src/Polly` is the pre-v8 API kept for backwards compatibility. New features belong in the v8 packages. Changes to `src/Polly` should be limited to bug fixes and v7/v8 interoperability (e.g. `AsAsyncPolicy()`/`AsSyncPolicy()`), and must not break existing v7 behavior.
