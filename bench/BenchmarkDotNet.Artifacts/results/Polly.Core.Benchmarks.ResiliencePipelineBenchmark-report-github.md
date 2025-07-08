```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|---------:|---------:|---------:|------:|--------:|----------:|------------:|
| ExecuteOutcomeAsync                            | 35.78 ns | 0.023 ns | 0.034 ns | 35.77 ns |  1.00 |    0.00 |         - |          NA |
| ExecuteAsync_ResilienceContextAndState         | 65.19 ns | 0.027 ns | 0.038 ns | 65.19 ns |  1.82 |    0.00 |         - |          NA |
| ExecuteAsync_CancellationToken                 | 69.55 ns | 0.826 ns | 1.185 ns | 68.91 ns |  1.94 |    0.03 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 68.90 ns | 0.160 ns | 0.230 ns | 68.81 ns |  1.93 |    0.01 |         - |          NA |
| Execute_ResilienceContextAndState              | 51.70 ns | 0.353 ns | 0.528 ns | 51.69 ns |  1.44 |    0.01 |         - |          NA |
| Execute_CancellationToken                      | 56.06 ns | 0.041 ns | 0.056 ns | 56.06 ns |  1.57 |    0.00 |         - |          NA |
| Execute_GenericStrategy_CancellationToken      | 56.52 ns | 0.469 ns | 0.672 ns | 57.06 ns |  1.58 |    0.02 |         - |          NA |
