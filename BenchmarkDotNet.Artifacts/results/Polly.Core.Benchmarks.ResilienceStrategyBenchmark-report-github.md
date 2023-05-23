``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |      Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
|                            ExecuteOutcomeAsync |  60.27 ns | 0.288 ns | 0.413 ns |  1.00 |    0.00 |      - |         - |          NA |
|         ExecuteAsync_ResilienceContextAndState | 187.70 ns | 1.954 ns | 2.803 ns |  3.11 |    0.04 | 0.0012 |      32 B |          NA |
|                 ExecuteAsync_CancellationToken | 178.10 ns | 0.834 ns | 1.142 ns |  2.96 |    0.03 |      - |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 181.65 ns | 1.889 ns | 2.710 ns |  3.01 |    0.04 |      - |         - |          NA |
