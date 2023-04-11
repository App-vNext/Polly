# Benchmark results

```text
BenchmarkDotNet=v0.13.3, OS=Windows 11 (10.0.22621.1413)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.202
  [Host] : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15
LaunchCount=2  WarmupCount=10
```

## PIPELINES

|             Method | Components |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecutePipeline_V7 |          1 |    71.29 ns |  1.309 ns |  1.959 ns |  1.00 |    0.00 | 0.0362 |     304 B |        1.00 |
| ExecutePipeline_V8 |          1 |    92.93 ns |  1.398 ns |  2.049 ns |  1.30 |    0.05 | 0.0181 |     152 B |        0.50 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |          2 |   164.39 ns |  5.294 ns |  7.592 ns |  1.00 |    0.00 | 0.0658 |     552 B |        1.00 |
| ExecutePipeline_V8 |          2 |   126.74 ns |  1.198 ns |  1.755 ns |  0.77 |    0.04 | 0.0181 |     152 B |        0.28 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |          5 |   540.07 ns | 16.941 ns | 24.832 ns |  1.00 |    0.00 | 0.1545 |    1296 B |        1.00 |
| ExecutePipeline_V8 |          5 |   257.13 ns |  2.748 ns |  4.114 ns |  0.48 |    0.03 | 0.0181 |     152 B |        0.12 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |         10 | 1,111.72 ns | 16.405 ns | 23.527 ns |  1.00 |    0.00 | 0.3014 |    2536 B |        1.00 |
| ExecutePipeline_V8 |         10 |   467.93 ns |  6.546 ns |  9.388 ns |  0.42 |    0.01 | 0.0181 |     152 B |        0.06 |

## TIMEOUT

|            Method |     Mean |   Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------ |---------:|--------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 287.1 ns | 9.20 ns | 12.59 ns |  1.00 |    0.00 | 0.0868 |     728 B |        1.00 |
| ExecuteTimeout_V8 | 272.9 ns | 3.16 ns |  4.54 ns |  0.95 |    0.04 | 0.0439 |     368 B |        0.51 |

## RETRY

|          Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRetry_V7 | 162.5 ns | 2.33 ns | 3.34 ns |  1.00 |    0.00 | 0.0687 |     576 B |        1.00 |
| ExecuteRetry_V8 | 152.3 ns | 1.31 ns | 1.93 ns |  0.94 |    0.02 | 0.0181 |     152 B |        0.26 |

## RATE LIMITER

|                Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRateLimiter_V7 | 173.8 ns | 2.33 ns | 3.48 ns |  1.00 |    0.00 | 0.0448 |     376 B |        1.00 |
| ExecuteRateLimiter_V8 | 207.9 ns | 2.06 ns | 2.89 ns |  1.19 |    0.03 | 0.0229 |     192 B |        0.51 |

## STRATEGY PIPELINE (TIMEOUT + RETRY + TIMEOUT + RATE LIMITER)

|                     Method |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_V7 | 1.207 us | 0.0201 us | 0.0295 us | 1.202 us |  1.00 |    0.00 | 0.2861 |    2400 B |        1.00 |
| ExecuteStrategyPipeline_V8 | 1.117 us | 0.0297 us | 0.0407 us | 1.118 us |  0.93 |    0.05 | 0.0935 |     792 B |        0.33 |