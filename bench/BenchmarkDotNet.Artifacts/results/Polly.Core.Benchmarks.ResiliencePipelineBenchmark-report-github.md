```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|---------:|---------:|---------:|------:|--------:|----------:|------------:|
| ExecuteOutcomeAsync                            | 35.40 ns | 0.084 ns | 0.113 ns | 35.35 ns |  1.00 |    0.00 |         - |          NA |
| ExecuteAsync_ResilienceContextAndState         | 65.38 ns | 0.426 ns | 0.569 ns | 65.77 ns |  1.85 |    0.02 |         - |          NA |
| ExecuteAsync_CancellationToken                 | 69.33 ns | 0.112 ns | 0.154 ns | 69.33 ns |  1.96 |    0.01 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 71.03 ns | 0.947 ns | 1.388 ns | 71.47 ns |  2.01 |    0.04 |         - |          NA |
| Execute_ResilienceContextAndState              | 50.61 ns | 0.136 ns | 0.190 ns | 50.69 ns |  1.43 |    0.01 |         - |          NA |
| Execute_CancellationToken                      | 55.46 ns | 0.737 ns | 1.080 ns | 54.51 ns |  1.57 |    0.03 |         - |          NA |
| Execute_GenericStrategy_CancellationToken      | 56.74 ns | 0.107 ns | 0.146 ns | 56.68 ns |  1.60 |    0.01 |         - |          NA |
