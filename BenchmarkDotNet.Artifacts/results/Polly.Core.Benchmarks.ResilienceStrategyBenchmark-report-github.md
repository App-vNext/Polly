``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |      Mean |    Error |   StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
|                            ExecuteOutcomeAsync |  69.47 ns | 0.660 ns | 0.947 ns |  1.00 |    0.00 |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 179.56 ns | 4.533 ns | 6.500 ns |  2.59 |    0.12 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 190.38 ns | 3.343 ns | 4.900 ns |  2.74 |    0.08 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 182.27 ns | 1.452 ns | 2.083 ns |  2.62 |    0.05 |         - |          NA |
