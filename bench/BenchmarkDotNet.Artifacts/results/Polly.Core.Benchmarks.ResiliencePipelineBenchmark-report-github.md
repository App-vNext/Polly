```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |      Mean |    Error |   StdDev |    Median | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |----------:|---------:|---------:|----------:|------:|--------:|----------:|------------:|
|                            ExecuteOutcomeAsync |  78.00 ns | 0.300 ns | 0.439 ns |  77.95 ns |  1.00 |    0.00 |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 183.18 ns | 1.886 ns | 2.705 ns | 183.73 ns |  2.35 |    0.04 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 190.95 ns | 2.795 ns | 4.008 ns | 194.12 ns |  2.45 |    0.05 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 193.67 ns | 0.283 ns | 0.397 ns | 193.65 ns |  2.48 |    0.01 |         - |          NA |
|              Execute_ResilienceContextAndState |  90.29 ns | 1.422 ns | 2.128 ns |  89.71 ns |  1.16 |    0.03 |         - |          NA |
|                      Execute_CancellationToken |  83.25 ns | 0.948 ns | 1.418 ns |  83.02 ns |  1.07 |    0.02 |         - |          NA |
|      Execute_GenericStrategy_CancellationToken |  78.66 ns | 0.446 ns | 0.625 ns |  78.53 ns |  1.01 |    0.01 |         - |          NA |
