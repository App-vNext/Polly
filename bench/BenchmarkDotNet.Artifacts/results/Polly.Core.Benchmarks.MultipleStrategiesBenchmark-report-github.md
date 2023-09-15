```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|             ExecuteStrategyPipeline_Generic_V7 | 2.277 μs | 0.0133 μs | 0.0191 μs |  1.00 |    0.00 | 0.1106 |    2824 B |        1.00 |
|             ExecuteStrategyPipeline_Generic_V8 | 2.089 μs | 0.0105 μs | 0.0157 μs |  0.92 |    0.01 |      - |      40 B |        0.01 |
|    ExecuteStrategyPipeline_GenericTelemetry_V8 | 3.034 μs | 0.0117 μs | 0.0175 μs |  1.33 |    0.01 |      - |      40 B |        0.01 |
|          ExecuteStrategyPipeline_NonGeneric_V8 | 2.380 μs | 0.0076 μs | 0.0113 μs |  1.05 |    0.01 |      - |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 3.542 μs | 0.0178 μs | 0.0266 μs |  1.56 |    0.02 |      - |      40 B |        0.01 |
