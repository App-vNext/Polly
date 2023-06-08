``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |      Mean |    Error |   StdDev |    Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------------------------------- |----------:|---------:|---------:|----------:|------:|--------:|-------:|----------:|------------:|
|                            ExecuteOutcomeAsync |  70.84 ns | 0.722 ns | 1.012 ns |  70.20 ns |  1.00 |    0.00 |      - |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 170.43 ns | 1.402 ns | 2.055 ns | 168.87 ns |  2.41 |    0.06 |      - |         - |          NA |
|                 ExecuteAsync_CancellationToken | 182.04 ns | 1.460 ns | 2.046 ns | 183.40 ns |  2.57 |    0.06 |      - |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 181.49 ns | 0.892 ns | 1.279 ns | 182.35 ns |  2.56 |    0.02 |      - |         - |          NA |
|              Execute_ResilienceContextAndState |  99.09 ns | 0.383 ns | 0.550 ns |  99.14 ns |  1.40 |    0.02 | 0.0035 |      88 B |          NA |
|                      Execute_CancellationToken |  94.22 ns | 0.503 ns | 0.752 ns |  94.24 ns |  1.33 |    0.01 | 0.0035 |      88 B |          NA |
|      Execute_GenericStrategy_CancellationToken |  94.94 ns | 0.443 ns | 0.649 ns |  95.10 ns |  1.34 |    0.02 | 0.0035 |      88 B |          NA |
