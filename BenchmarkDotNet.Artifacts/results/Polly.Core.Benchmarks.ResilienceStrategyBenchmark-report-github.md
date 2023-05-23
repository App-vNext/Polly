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
|                            ExecuteOutcomeAsync |  61.80 ns | 0.537 ns | 0.770 ns |  1.00 |    0.00 |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 169.88 ns | 1.252 ns | 1.755 ns |  2.75 |    0.05 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 178.11 ns | 0.894 ns | 1.253 ns |  2.88 |    0.02 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 181.22 ns | 0.512 ns | 0.701 ns |  2.93 |    0.04 |         - |          NA |
