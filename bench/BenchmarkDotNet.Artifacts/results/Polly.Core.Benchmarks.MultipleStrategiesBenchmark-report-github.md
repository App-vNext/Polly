```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_Generic_V7             |   926.0 ns | 54.27 ns | 81.23 ns |  1.01 |    0.13 | 0.2184 |    2744 B |        1.00 |
| ExecuteStrategyPipeline_Generic_V8             |   766.3 ns | 45.05 ns | 67.42 ns |  0.83 |    0.10 | 0.0029 |      40 B |        0.01 |
| ExecuteStrategyPipeline_GenericTelemetry_V8    | 1,042.7 ns | 55.46 ns | 83.01 ns |  1.13 |    0.13 | 0.0029 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGeneric_V8          |   872.1 ns | 58.58 ns | 87.67 ns |  0.95 |    0.13 | 0.0029 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 1,125.7 ns | 57.02 ns | 85.35 ns |  1.22 |    0.14 | 0.0019 |      40 B |        0.01 |
