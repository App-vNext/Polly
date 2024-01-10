```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_Generic_V7             | 2.626 μs | 0.4061 μs | 0.5824 μs |  1.00 |    0.00 | 0.1106 |    2824 B |        1.00 |
| ExecuteStrategyPipeline_Generic_V8             | 1.853 μs | 0.0380 μs | 0.0556 μs |  0.74 |    0.18 |      - |      40 B |        0.01 |
| ExecuteStrategyPipeline_GenericTelemetry_V8    | 2.647 μs | 0.0401 μs | 0.0600 μs |  1.06 |    0.22 |      - |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGeneric_V8          | 2.059 μs | 0.0292 μs | 0.0418 μs |  0.82 |    0.18 |      - |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 2.934 μs | 0.0150 μs | 0.0220 μs |  1.17 |    0.26 |      - |      40 B |        0.01 |
