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
| ExecuteOutcomeAsync                            | 28.96 ns | 0.189 ns | 0.271 ns | 29.03 ns |  1.00 |    0.01 |         - |          NA |
| ExecuteAsync_ResilienceContextAndState         | 55.90 ns | 0.282 ns | 0.376 ns | 55.86 ns |  1.93 |    0.02 |         - |          NA |
| ExecuteAsync_CancellationToken                 | 59.75 ns | 1.010 ns | 1.512 ns | 59.97 ns |  2.06 |    0.05 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 62.98 ns | 1.785 ns | 2.672 ns | 63.49 ns |  2.17 |    0.09 |         - |          NA |
| Execute_ResilienceContextAndState              | 44.60 ns | 0.528 ns | 0.791 ns | 44.29 ns |  1.54 |    0.03 |         - |          NA |
| Execute_CancellationToken                      | 45.80 ns | 0.330 ns | 0.494 ns | 45.54 ns |  1.58 |    0.02 |         - |          NA |
| Execute_GenericStrategy_CancellationToken      | 46.75 ns | 0.310 ns | 0.463 ns | 46.46 ns |  1.61 |    0.02 |         - |          NA |
