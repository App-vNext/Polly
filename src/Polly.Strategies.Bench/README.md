# Benchmark results

```text
BenchmarkDotNet=v0.13.3, OS=Windows 11 (10.0.22621.963)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.101
  [Host] : .NET 7.0.1 (7.0.122.56804), X64 RyuJIT AVX2
```

## PIPELINES

|          Method | Components | PollyVersion |        Mean |     Error |    StdDev |   Gen0 | Allocated |
|---------------- |----------- |------------- |------------:|----------:|----------:|-------:|----------:|
| ExecutePipeline |          2 |           V7 |   195.94 ns |  1.682 ns |  2.466 ns | 0.0467 |     392 B |
| ExecutePipeline |          2 |           V8 |    67.65 ns |  0.282 ns |  0.404 ns |      - |         - |
| ExecutePipeline |          5 |           V7 |   599.27 ns |  6.370 ns |  9.337 ns | 0.1354 |    1136 B |
| ExecutePipeline |          5 |           V8 |   103.66 ns |  0.319 ns |  0.447 ns |      - |         - |
| ExecutePipeline |         10 |           V7 | 1,251.22 ns | 14.295 ns | 20.954 ns | 0.2823 |    2376 B |
| ExecutePipeline |         10 |           V8 |   126.08 ns |  0.765 ns |  1.098 ns |      - |         - |

## RETRIES

|       Method | PollyVersion |     Mean |   Error |  StdDev |   Gen0 | Allocated |
|------------- |------------- |---------:|--------:|--------:|-------:|----------:|
| ExecuteRetry |           V7 | 179.0 ns | 1.26 ns | 1.80 ns | 0.0496 |     416 B |
| ExecuteRetry |           V8 | 169.3 ns | 0.50 ns | 0.71 ns |      - |         - |

## TIMEOUT

|         Method | PollyVersion |     Mean |   Error |   StdDev |   Gen0 | Allocated |
|--------------- |------------- |---------:|--------:|---------:|-------:|----------:|
| ExecuteTimeout |           V7 | 457.1 ns | 8.17 ns | 11.72 ns | 0.0973 |     816 B |
| ExecuteTimeout |           V8 | 191.5 ns | 0.68 ns |  0.96 ns |      - |         - |

## SIMPLE PIPELINE (Outer Timeout - Retries - Inner Timeout)

|                Method | PollyVersion |       Mean |    Error |    StdDev |   Gen0 | Allocated |
|---------------------- |------------- |-----------:|---------:|----------:|-------:|----------:|
| ExecuteSimplePipeline |           V7 | 1,690.5 ns | 81.64 ns | 117.09 ns | 0.2880 |    2416 B |
| ExecuteSimplePipeline |           V8 |   577.9 ns | 11.26 ns |  16.85 ns |      - |         - |
