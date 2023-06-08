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
|                            ExecuteOutcomeAsync |  72.19 ns | 1.272 ns | 1.741 ns |  73.65 ns |  1.00 |    0.00 |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 171.25 ns | 1.890 ns | 2.770 ns | 171.35 ns |  2.37 |    0.03 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 182.99 ns | 0.598 ns | 0.838 ns | 182.59 ns |  2.54 |    0.05 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 186.52 ns | 1.310 ns | 1.837 ns | 186.61 ns |  2.59 |    0.08 |         - |          NA |
|              Execute_ResilienceContextAndState |  74.44 ns | 0.114 ns | 0.155 ns |  74.44 ns |  1.03 |    0.02 |         - |          NA |
|                      Execute_CancellationToken |  77.50 ns | 0.247 ns | 0.362 ns |  77.50 ns |  1.07 |    0.03 |         - |          NA |
|      Execute_GenericStrategy_CancellationToken |  77.63 ns | 0.281 ns | 0.412 ns |  77.57 ns |  1.08 |    0.03 |         - |          NA |
