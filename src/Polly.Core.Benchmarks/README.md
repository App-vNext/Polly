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

|             Method | Components |        Mean |     Error |     StdDev |      Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|-----------:|------------:|------:|--------:|-------:|----------:|------------:|
| ExecutePipeline_V7 |          1 |    74.78 ns |  1.555 ns |   2.279 ns |    75.63 ns |  1.00 |    0.00 | 0.0362 |     304 B |        1.00 |
| ExecutePipeline_V8 |          1 |    85.69 ns |  0.500 ns |   0.732 ns |    85.36 ns |  1.15 |    0.04 |      - |         - |        0.00 |
|                    |            |             |           |            |             |       |         |        |           |             |
| ExecutePipeline_V7 |          2 |   165.37 ns |  1.157 ns |   1.732 ns |   165.59 ns |  1.00 |    0.00 | 0.0658 |     552 B |        1.00 |
| ExecutePipeline_V8 |          2 |   119.10 ns |  0.653 ns |   0.915 ns |   119.63 ns |  0.72 |    0.01 |      - |         - |        0.00 |
|                    |            |             |           |            |             |       |         |        |           |             |
| ExecutePipeline_V7 |          5 |   533.97 ns |  7.327 ns |  10.967 ns |   536.79 ns |  1.00 |    0.00 | 0.1545 |    1296 B |        1.00 |
| ExecutePipeline_V8 |          5 |   227.69 ns |  1.236 ns |   1.812 ns |   227.72 ns |  0.43 |    0.01 |      - |         - |        0.00 |
|                    |            |             |           |            |             |       |         |        |           |             |
| ExecutePipeline_V7 |         10 | 1,191.41 ns | 35.512 ns |  53.152 ns | 1,192.79 ns |  1.00 |    0.00 | 0.3014 |    2536 B |        1.00 |
| ExecutePipeline_V8 |         10 |   557.95 ns | 76.434 ns | 112.036 ns |   505.58 ns |  0.47 |    0.09 |      - |         - |        0.00 |

## TIMEOUT

|            Method |     Mean |   Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------ |---------:|--------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 304.9 ns | 7.53 ns | 11.27 ns |  1.00 |    0.00 | 0.0868 |     728 B |        1.00 |
| ExecuteTimeout_V8 | 266.5 ns | 5.95 ns |  8.72 ns |  0.88 |    0.04 |      - |         - |        0.00 |

## RETRY

|          Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRetry_V7 | 169.8 ns | 4.98 ns | 6.98 ns |  1.00 |    0.00 | 0.0687 |     576 B |        1.00 |
| ExecuteRetry_V8 | 144.9 ns | 2.35 ns | 3.52 ns |  0.85 |    0.04 |      - |         - |        0.00 |

## RATE LIMITER

|                Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRateLimiter_V7 | 190.8 ns | 10.01 ns | 14.98 ns |  1.00 |    0.00 | 0.0448 |     376 B |        1.00 |
| ExecuteRateLimiter_V8 | 199.6 ns |  2.54 ns |  3.64 ns |  1.05 |    0.09 | 0.0048 |      40 B |        0.11 |

## STRATEGY PIPELINE (TIMEOUT + RETRY + TIMEOUT + RATE LIMITER)

|                     Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_V7 | 1.265 us | 0.0372 us | 0.0558 us |  1.00 |    0.00 | 0.2861 |    2400 B |        1.00 |
| ExecuteStrategyPipeline_V8 | 1.032 us | 0.0165 us | 0.0236 us |  0.82 |    0.04 | 0.0076 |      64 B |        0.03 |
