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
|                            ExecuteOutcomeAsync |  69.04 ns | 0.656 ns | 0.898 ns |  69.13 ns |  1.00 |    0.00 |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 169.91 ns | 2.823 ns | 4.049 ns | 167.61 ns |  2.47 |    0.09 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 179.82 ns | 1.461 ns | 2.142 ns | 179.23 ns |  2.61 |    0.04 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 179.50 ns | 1.440 ns | 2.065 ns | 179.23 ns |  2.60 |    0.05 |         - |          NA |
