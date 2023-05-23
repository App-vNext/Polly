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
|         ExecuteAsync_ResilienceContextAndState |  67.14 ns | 0.473 ns | 0.708 ns |  1.00 |    0.00 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 115.73 ns | 0.874 ns | 1.253 ns |  1.72 |    0.01 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 117.85 ns | 0.346 ns | 0.486 ns |  1.76 |    0.02 |         - |          NA |
