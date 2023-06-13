``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |      Mean |    Error |   StdDev |    Median | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |----------:|---------:|---------:|----------:|------:|--------:|----------:|------------:|
|                            ExecuteOutcomeAsync |  69.88 ns | 0.093 ns | 0.130 ns |  69.91 ns |  1.00 |    0.00 |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 172.99 ns | 0.920 ns | 1.348 ns | 172.82 ns |  2.48 |    0.02 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 185.87 ns | 1.043 ns | 1.496 ns | 186.11 ns |  2.66 |    0.02 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 187.20 ns | 4.618 ns | 6.474 ns | 192.09 ns |  2.68 |    0.09 |         - |          NA |
|              Execute_ResilienceContextAndState |  78.18 ns | 3.369 ns | 4.612 ns |  76.87 ns |  1.12 |    0.07 |         - |          NA |
|                      Execute_CancellationToken |  70.32 ns | 0.175 ns | 0.240 ns |  70.40 ns |  1.01 |    0.00 |         - |          NA |
|      Execute_GenericStrategy_CancellationToken |  71.08 ns | 0.516 ns | 0.757 ns |  71.55 ns |  1.02 |    0.01 |         - |          NA |
