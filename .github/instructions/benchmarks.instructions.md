---
applyTo: "bench/**"
---

# Benchmarks

- `bench/Polly.Core.Benchmarks` benchmarks the v8 API and `bench/Polly.Benchmarks` benchmarks the legacy v7 API. Most v8 benchmarks compare against the equivalent v7 policy using `PollyVersion`.
- `Program.cs` already configures the BenchmarkDotNet job, toolchain and memory diagnoser. Benchmark classes should not add their own `[Config]` or diagnoser attributes.
- Reusable pipeline construction belongs in the `Helper` partial class under `Utils/`.
- Benchmarks should build pipelines in `[GlobalSetup]`, return the `ValueTask`/result, avoid manual loops and real delays, and use static lambdas so that the reported allocations come from Polly rather than the benchmark code.
- Benchmark classes must be `public`, non-`sealed`, non-`static` classes.
- Regenerated reports under `BenchmarkDotNet.Artifacts/results` should only be committed when that is the purpose of the PR.
- See the `performance-benchmark` skill in `.github/skills` for how to run and compare benchmarks.
