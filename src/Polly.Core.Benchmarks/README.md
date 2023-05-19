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
| ExecutePipeline_V7 |          1 |    91.11 ns |  0.628 ns |  0.939 ns |  1.00 |    0.00 | 0.0120 |     304 B |        1.00 |
| ExecutePipeline_V8 |          1 |   158.94 ns |  0.914 ns |  1.311 ns |  1.75 |    0.02 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |          2 |   180.26 ns |  1.257 ns |  1.881 ns |  1.00 |    0.00 | 0.0219 |     552 B |        1.00 |
| ExecutePipeline_V8 |          2 |   311.17 ns |  1.193 ns |  1.748 ns |  1.73 |    0.02 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |          5 |   644.61 ns |  6.817 ns | 10.203 ns |  1.00 |    0.00 | 0.0515 |    1296 B |        1.00 |
| ExecutePipeline_V8 |          5 |   639.76 ns |  2.951 ns |  4.233 ns |  0.99 |    0.02 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |         10 | 1,392.30 ns | 15.508 ns | 21.741 ns |  1.00 |    0.00 | 0.0992 |    2536 B |        1.00 |
| ExecutePipeline_V8 |         10 | 1,144.47 ns |  6.144 ns |  9.005 ns |  0.82 |    0.02 |      - |         - |        0.00 |

## TIMEOUT

|            Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------ |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 332.3 ns | 6.37 ns | 9.34 ns |  1.00 |    0.00 | 0.0286 |     728 B |        1.00 |
| ExecuteTimeout_V8 | 368.1 ns | 4.70 ns | 7.03 ns |  1.11 |    0.04 |      - |         - |        0.00 |

## RETRY

|          Method |     Mean |   Error |  StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|---------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| ExecuteRetry_V7 | 192.6 ns | 1.55 ns | 2.22 ns |  1.00 | 0.0229 |     576 B |        1.00 |
| ExecuteRetry_V8 | 232.8 ns | 0.66 ns | 0.91 ns |  1.21 |      - |         - |        0.00 |

## RATE LIMITER

|                Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRateLimiter_V7 | 195.1 ns | 1.16 ns | 1.69 ns |  1.00 |    0.00 | 0.0148 |     376 B |        1.00 |
| ExecuteRateLimiter_V8 | 308.6 ns | 3.92 ns | 5.74 ns |  1.58 |    0.03 | 0.0014 |      40 B |        0.11 |

## CIRCUIT BREAKER

|                   Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 257.2 ns | 3.45 ns | 5.16 ns |  1.00 |    0.00 | 0.0210 |     528 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 422.4 ns | 2.16 ns | 3.09 ns |  1.64 |    0.03 | 0.0010 |      32 B |        0.06 |

## STRATEGY PIPELINE (RATE LIMITER + TIMEOUT + RETRY + TIMEOUT + CIRCUIT BREAKER)

|                     Method |     Mean |     Error |    StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_V7 | 1.890 us | 0.0062 us | 0.0093 us |  1.00 | 0.1144 |    2872 B |        1.00 |
| ExecuteStrategyPipeline_V8 | 1.989 us | 0.0067 us | 0.0096 us |  1.05 | 0.0038 |      96 B |        0.03 |

## HEDGING

|                      Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|---------------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
|             Hedging_Primary | 1.182 us | 0.0203 us | 0.0297 us |  1.00 |    0.00 | 0.0019 |      - |      80 B |        1.00 |
|           Hedging_Secondary | 2.071 us | 0.0214 us | 0.0321 us |  1.75 |    0.05 | 0.0076 |      - |     280 B |        3.50 |
|   Hedging_Primary_AsyncWork | 5.843 us | 0.1018 us | 0.1460 us |  4.96 |    0.20 | 0.0687 | 0.0229 |    1778 B |       22.23 |
| Hedging_Secondary_AsyncWork | 8.526 us | 0.2354 us | 0.3376 us |  7.23 |    0.34 | 0.0763 | 0.0381 |    2025 B |       25.31 |
