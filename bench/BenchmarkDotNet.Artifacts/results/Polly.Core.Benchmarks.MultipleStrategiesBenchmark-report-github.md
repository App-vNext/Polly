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
|             ExecuteStrategyPipeline_Generic_V7 | 2.632 μs | 0.0337 μs | 0.0484 μs |  1.00 |    0.00 | 0.1106 |    2824 B |        1.00 |
|             ExecuteStrategyPipeline_Generic_V8 | 2.331 μs | 0.0067 μs | 0.0096 μs |  0.89 |    0.02 |      - |      72 B |        0.03 |
|    ExecuteStrategyPipeline_GenericTelemetry_V8 | 3.265 μs | 0.0104 μs | 0.0149 μs |  1.24 |    0.03 |      - |      72 B |        0.03 |
|          ExecuteStrategyPipeline_NonGeneric_V8 | 2.428 μs | 0.0126 μs | 0.0181 μs |  0.92 |    0.02 |      - |      72 B |        0.03 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 3.396 μs | 0.0220 μs | 0.0323 μs |  1.29 |    0.03 |      - |      72 B |        0.03 |
