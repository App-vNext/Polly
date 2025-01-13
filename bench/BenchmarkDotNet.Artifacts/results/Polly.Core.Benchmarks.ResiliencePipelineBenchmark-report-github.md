```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| ExecuteOutcomeAsync                            | 45.69 ns | 0.443 ns | 0.635 ns |  1.00 |    0.02 |         - |          NA |
| ExecuteAsync_ResilienceContextAndState         | 80.17 ns | 0.672 ns | 1.006 ns |  1.76 |    0.03 |         - |          NA |
| ExecuteAsync_CancellationToken                 | 84.26 ns | 0.285 ns | 0.381 ns |  1.84 |    0.03 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 87.55 ns | 3.675 ns | 5.030 ns |  1.92 |    0.11 |         - |          NA |
| Execute_ResilienceContextAndState              | 59.59 ns | 0.610 ns | 0.894 ns |  1.30 |    0.03 |         - |          NA |
| Execute_CancellationToken                      | 66.67 ns | 0.542 ns | 0.794 ns |  1.46 |    0.03 |         - |          NA |
| Execute_GenericStrategy_CancellationToken      | 67.05 ns | 0.632 ns | 0.906 ns |  1.47 |    0.03 |         - |          NA |
