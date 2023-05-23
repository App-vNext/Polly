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
|         ExecuteAsync_ResilienceContextAndState |  68.89 ns | 1.419 ns | 2.034 ns |  70.42 ns |  1.00 |    0.00 |         - |          NA |
|                 ExecuteAsync_CancellationToken | 117.33 ns | 1.144 ns | 1.677 ns | 116.99 ns |  1.70 |    0.05 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 116.36 ns | 0.229 ns | 0.320 ns | 116.26 ns |  1.69 |    0.05 |         - |          NA |
