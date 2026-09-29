---
name: performance-benchmark
description: Write and run BenchmarkDotNet benchmarks to validate the performance and allocation impact of code changes in App-vNext/Polly. Use this when asked to benchmark, profile, or validate the performance impact of a code change, or when reviewing a PR that claims a performance improvement.
---

# Performance Benchmarking

Polly's execution paths are designed to be fast and allocation-free in the common case (see [`docs/advanced/performance.md`](../../../docs/advanced/performance.md)). When you need to validate the performance impact of a code change, follow this process to write a BenchmarkDotNet benchmark and compare a baseline build against the changed build.

The benchmarks live in [`bench/`](../../../bench/README.md):

- `bench/Polly.Core.Benchmarks` - benchmarks for the v8 API (`Polly.Core`, `Polly.Extensions` and `Polly.RateLimiting`). Most benchmarks compare the v8 implementation against the equivalent v7 policy. **Use this project for almost all changes.**
- `bench/Polly.Benchmarks` - benchmarks for the legacy v7 `Polly` API. Only use this for changes to `src/Polly`.

## Step 1: Write the Benchmark

First check whether an existing benchmark in `bench/Polly.Core.Benchmarks` already covers the code being changed (e.g. `RetryBenchmark`, `CircuitBreakerBenchmark`, `HedgingBenchmark`, `PipelineBenchmark`, `TelemetryBenchmark`). Prefer running an existing benchmark over writing a new one.

If a new benchmark is needed, add a class to `bench/Polly.Core.Benchmarks` following the existing conventions:

- The project's `Program.cs` already configures BenchmarkDotNet with the `MediumRun` job, the in-process emit toolchain and the memory diagnoser. Do not add another entry point or per-class `[Config]`/`[MemoryDiagnoser]` attributes.
- `BenchmarkDotNet.Attributes` and the other BenchmarkDotNet namespaces are already imported as global usings.
- Put reusable pipeline construction in the `Helper` partial class under `Utils/` (e.g. `Helper.Retry.cs`).

```csharp
namespace Polly.Core.Benchmarks;

public class MyStrategyBenchmark
{
    private ResiliencePipeline<string>? _pipeline;

    [GlobalSetup]
    public void Setup()
    {
        _pipeline = new ResiliencePipelineBuilder<string>()
            .AddRetry(new() { ShouldHandle = _ => PredicateResult.False() })
            .Build();
    }

    [Benchmark]
    public ValueTask<string> Execute() =>
        _pipeline!.ExecuteAsync(static _ => new ValueTask<string>("result"));
}
```

### Best Practices

For comprehensive guidance, see the [Microbenchmark Design Guidelines](https://github.com/dotnet/performance/blob/main/docs/microbenchmark-design-guidelines.md). Key principles:

- **Move initialization to `[GlobalSetup]`**: Build pipelines, options and test data in setup so that only execution is measured - unless building the pipeline is what the change affects (see `CreationBenchmark`).
- **Return values** (including `ValueTask`/`ValueTask<T>`) from benchmark methods to prevent dead code elimination.
- **Avoid loops**: BenchmarkDotNet invokes the benchmark many times automatically; manual loops distort the measurements.
- **Avoid allocations in the benchmark itself**: Use static lambdas and the state-passing overloads of `Execute`/`ExecuteAsync` so that allocations reported by the memory diagnoser come from Polly, not from the benchmark code.
- **No side effects**: Benchmarks should produce consistent results between invocations. Avoid real delays - configure strategies so that the measured path does not wait (e.g. zero or very small delays, or outcomes that are not handled).
- **Focus on the common case**: Benchmark the hot path and typical usage (e.g. a successful execution through a pipeline), then add scenarios for the specific path being changed (e.g. a handled outcome that triggers a retry).
- **Cover the variants the change affects**: Generic vs. non-generic pipelines, sync vs. async execution, and a single strategy vs. a pipeline with multiple strategies.
- **Benchmark class requirements**: Must be `public`, not `sealed`, not `static`, and must be a `class` (not a struct).
- **Processor affinity**: If the benchmark environment's processors include both performance and efficiency cores (e.g., on Apple M1/M2 or Intel hybrid architectures), consider pinning the benchmark to the performance cores to reduce noise. This can be achieved using the `--affinity` option in BenchmarkDotNet to specify an affinity mask to set for the benchmark process.

## Step 2: Prepare the Baseline and Changed Code

Benchmarks run in-process against project references, so the baseline and the changed code need to be in separate working directories.

1. Make sure the intended changes are safely saved (committed, or at least left untouched in the working tree). Do not stash, reset or revert unrelated changes.
2. Create a separate worktree for the baseline at the commit the change is based on (usually the merge base with `main`):

   ```powershell
   git worktree add ../polly-baseline $(git merge-base HEAD main)
   ```

3. If you wrote a new benchmark, or modified an existing one, copy the benchmark files (including any `Utils/Helper.*.cs` changes) into the same location in the baseline worktree so that both sides run identical benchmark code. If the benchmark uses new API that only exists in the changed code, adapt the baseline copy to the closest equivalent and state that clearly when reporting results.

## Step 3: Run the Benchmarks

Run the same benchmark, with the same filter, in the baseline worktree first and then in the changed working tree:

```powershell
dotnet run --configuration Release --framework net10.0 --project ./bench/Polly.Core.Benchmarks/Polly.Core.Benchmarks.csproj -- --filter "*MyStrategyBenchmark*"
```

- Always use a filter to run only the relevant benchmarks. Running the entire suite (`./benchmarks.ps1` from the `bench` directory, as described in `bench/README.md`) takes a long time and is rarely necessary.
- Always use the `Release` configuration and the same target framework for both runs.
- Run both sides on the same machine, one after the other, with as little other activity on the machine as possible.

The results are written as Markdown to `bench/BenchmarkDotNet.Artifacts/results/` in each working directory.

## Step 4: Interpret and Report the Results

- We do not use fixed hardware, so absolute numbers vary between machines and runs. The important columns are `Ratio` and `Alloc Ratio` (within a run), and `Mean` and `Allocated` compared between the baseline and changed runs on the same machine.
- Allocation results are deterministic. Any increase in `Allocated` on an execution path is significant and must be explained, even if the time is unchanged.
- Small differences in `Mean` (within the reported `Error`/`StdDev`) are noise. If a result is surprising, re-run both sides before drawing conclusions.
- Investigate and explain any regression, even if other scenarios improve.

When reporting, include the baseline and changed result tables, the environment summary emitted by BenchmarkDotNet (OS, CPU, .NET version), the benchmark code if it is not already in the repository, and a short summary of the conclusions.

> 📝 **AI-generated content disclosure:** When posting benchmark results to GitHub under a user's credentials - i.e. the account is **not** a dedicated bot account or app (e.g. `github-actions[bot]`, `copilot`) - you **MUST** include a concise, visible note (e.g. a `> [!NOTE]` alert) at the bottom of the content indicating the content was AI-generated and which agent generated it. Skip this only if the user explicitly asks you to omit it.

## Step 5: Clean Up

- Remove the baseline worktree when you are finished (`git worktree remove ../polly-baseline`).
- The Markdown reports under `bench/BenchmarkDotNet.Artifacts/results/` are checked into the repository. Do not commit updated reports as part of a change unless you are asked to.
- Only keep a new benchmark in the PR if it provides lasting value for monitoring performance. Ad hoc benchmarks written only to validate a change should not be committed unless requested.

## Additional Resources

- [`bench/README.md`](../../../bench/README.md) - how to run the Polly benchmarks
- [Microbenchmark Design Guidelines](https://github.com/dotnet/performance/blob/main/docs/microbenchmark-design-guidelines.md)
- [BenchmarkDotNet CLI Arguments](https://github.com/dotnet/BenchmarkDotNet/blob/master/docs/articles/guides/console-args.md)
