# Benchmark results

```text
BenchmarkDotNet=v0.13.3, OS=Windows 11 (10.0.22621.963)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.101
  [Host] : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2
```

## PIPELINES

|             Method | Components |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecutePipeline_V7 |          2 |   187.95 ns |  9.763 ns | 13.687 ns |  1.00 |    0.00 | 0.0467 |     392 B |        1.00 |
| ExecutePipeline_V8 |          2 |    65.52 ns |  0.867 ns |  1.271 ns |  0.35 |    0.03 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |          5 |   531.91 ns |  3.662 ns |  5.481 ns |  1.00 |    0.00 | 0.1354 |    1136 B |        1.00 |
| ExecutePipeline_V8 |          5 |    94.90 ns |  0.823 ns |  1.232 ns |  0.18 |    0.00 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |         10 | 1,131.76 ns | 23.483 ns | 34.421 ns |  1.00 |    0.00 | 0.2823 |    2376 B |        1.00 |
| ExecutePipeline_V8 |         10 |   119.36 ns |  0.791 ns |  1.159 ns |  0.11 |    0.00 |      - |         - |        0.00 |

## RETRIES

|          Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRetry_V7 | 179.5 ns | 2.06 ns | 2.89 ns |  1.00 |    0.00 | 0.0496 |     416 B |        1.00 |
| ExecuteRetry_V8 | 165.3 ns | 3.80 ns | 5.69 ns |  0.93 |    0.03 |      - |         - |        0.00 |

## TIMEOUT

|            Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 389.8 ns |  2.62 ns |  3.92 ns |  1.00 |    0.00 | 0.0973 |     816 B |        1.00 |
| ExecuteTimeout_V8 | 191.0 ns | 14.29 ns | 20.94 ns |  0.49 |    0.05 |      - |         - |        0.00 |

## SIMPLE PIPELINE (Outer Timeout - Retries - Inner Timeout)

|                   Method |       Mean |    Error |   StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
| ExecuteSimplePipeline_V7 | 1,227.8 ns | 17.63 ns | 25.83 ns |  1.00 | 0.2880 |    2416 B |        1.00 |
| ExecuteSimplePipeline_V8 |   465.3 ns |  4.73 ns |  7.09 ns |  0.38 |      - |         - |        0.00 |