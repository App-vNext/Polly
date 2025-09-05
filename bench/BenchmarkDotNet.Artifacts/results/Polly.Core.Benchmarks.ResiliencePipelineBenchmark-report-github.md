```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|---------:|---------:|---------:|------:|--------:|----------:|------------:|
| ExecuteOutcomeAsync                            | 28.16 ns | 0.739 ns | 1.083 ns | 29.14 ns |  1.00 |    0.05 |         - |          NA |
| ExecuteAsync_ResilienceContextAndState         | 54.02 ns | 0.356 ns | 0.510 ns | 54.04 ns |  1.92 |    0.08 |         - |          NA |
| ExecuteAsync_CancellationToken                 | 58.20 ns | 0.551 ns | 0.790 ns | 57.59 ns |  2.07 |    0.08 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 58.45 ns | 0.245 ns | 0.360 ns | 58.26 ns |  2.08 |    0.08 |         - |          NA |
| Execute_ResilienceContextAndState              | 31.65 ns | 0.245 ns | 0.367 ns | 31.64 ns |  1.13 |    0.04 |         - |          NA |
| Execute_CancellationToken                      | 36.59 ns | 0.382 ns | 0.560 ns | 36.69 ns |  1.30 |    0.05 |         - |          NA |
| Execute_GenericStrategy_CancellationToken      | 36.80 ns | 0.245 ns | 0.367 ns | 36.97 ns |  1.31 |    0.05 |         - |          NA |
