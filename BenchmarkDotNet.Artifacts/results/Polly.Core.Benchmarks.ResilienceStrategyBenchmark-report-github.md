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
|                            ExecuteOutcomeAsync |  68.96 ns | 0.111 ns | 0.163 ns |  68.91 ns |  1.00 |    0.00 |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 200.33 ns | 1.497 ns | 2.148 ns | 199.59 ns |  2.90 |    0.03 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 184.24 ns | 1.331 ns | 1.993 ns | 184.89 ns |  2.67 |    0.03 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 178.89 ns | 1.096 ns | 1.571 ns | 177.62 ns |  2.59 |    0.02 |         - |          NA |
