``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
|                            ExecuteOutcomeAsync |  93.05 ns |  3.421 ns |  5.014 ns |  1.00 |    0.00 |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 212.89 ns | 14.518 ns | 21.730 ns |  2.30 |    0.27 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 211.75 ns |  6.933 ns | 10.376 ns |  2.28 |    0.14 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 211.68 ns |  1.738 ns |  2.547 ns |  2.28 |    0.13 |         - |          NA |
|              Execute_ResilienceContextAndState |  84.71 ns |  1.981 ns |  2.965 ns |  0.91 |    0.07 |         - |          NA |
|                      Execute_CancellationToken |  88.79 ns |  4.553 ns |  6.674 ns |  0.96 |    0.11 |         - |          NA |
|      Execute_GenericStrategy_CancellationToken |  87.09 ns |  1.332 ns |  1.994 ns |  0.94 |    0.05 |         - |          NA |
