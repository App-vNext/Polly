```

BenchmarkDotNet v0.13.6, Windows 11 (10.0.22621.1992/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.306
  [Host] : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|             ExecuteStrategyPipeline_Generic_V7 | 2.523 μs | 0.0207 μs | 0.0303 μs |  1.00 |    0.00 | 0.1106 |    2824 B |        1.00 |
|             ExecuteStrategyPipeline_Generic_V8 | 1.997 μs | 0.0079 μs | 0.0110 μs |  0.79 |    0.01 |      - |      40 B |        0.01 |
|    ExecuteStrategyPipeline_GenericTelemetry_V8 | 2.909 μs | 0.0111 μs | 0.0166 μs |  1.15 |    0.01 |      - |      40 B |        0.01 |
|          ExecuteStrategyPipeline_NonGeneric_V8 | 2.286 μs | 0.0101 μs | 0.0152 μs |  0.91 |    0.01 |      - |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 3.223 μs | 0.0146 μs | 0.0215 μs |  1.28 |    0.02 |      - |      40 B |        0.01 |
