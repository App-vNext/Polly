```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |      Mean |    Error |   StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
|                            ExecuteOutcomeAsync |  77.62 ns | 0.101 ns | 0.141 ns |  1.00 |    0.00 |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 182.77 ns | 0.330 ns | 0.473 ns |  2.35 |    0.01 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 194.84 ns | 0.935 ns | 1.370 ns |  2.51 |    0.02 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 186.25 ns | 0.724 ns | 1.039 ns |  2.40 |    0.02 |         - |          NA |
|              Execute_ResilienceContextAndState |  87.34 ns | 1.062 ns | 1.557 ns |  1.13 |    0.02 |         - |          NA |
|                      Execute_CancellationToken |  81.51 ns | 0.834 ns | 1.248 ns |  1.05 |    0.02 |         - |          NA |
|      Execute_GenericStrategy_CancellationToken |  80.56 ns | 0.626 ns | 0.937 ns |  1.04 |    0.01 |         - |          NA |
