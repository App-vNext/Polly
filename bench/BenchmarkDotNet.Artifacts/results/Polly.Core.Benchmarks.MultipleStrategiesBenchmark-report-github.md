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
| ExecuteStrategyPipeline_Generic_V7             |   994.4 ns | 82.26 ns | 123.1 ns |  1.02 |    0.18 | 0.2184 |    2744 B |        1.00 |
| ExecuteStrategyPipeline_Generic_V8             |   900.6 ns | 74.47 ns | 111.5 ns |  0.92 |    0.16 | 0.0029 |      40 B |        0.01 |
| ExecuteStrategyPipeline_GenericTelemetry_V8    | 1,277.0 ns | 87.67 ns | 131.2 ns |  1.30 |    0.21 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGeneric_V8          |   953.5 ns | 87.32 ns | 130.7 ns |  0.97 |    0.18 | 0.0029 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 1,297.5 ns | 85.24 ns | 127.6 ns |  1.32 |    0.21 | 0.0019 |      40 B |        0.01 |
