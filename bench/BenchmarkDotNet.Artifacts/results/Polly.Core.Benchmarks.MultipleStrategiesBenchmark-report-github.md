```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_Generic_V7             | 1.372 μs | 0.0189 μs | 0.0282 μs |  1.00 |    0.03 | 0.2174 |    2744 B |        1.00 |
| ExecuteStrategyPipeline_Generic_V8             | 1.189 μs | 0.0271 μs | 0.0406 μs |  0.87 |    0.03 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_GenericTelemetry_V8    | 1.647 μs | 0.0147 μs | 0.0221 μs |  1.20 |    0.03 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGeneric_V8          | 1.266 μs | 0.0436 μs | 0.0652 μs |  0.92 |    0.05 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 1.756 μs | 0.0171 μs | 0.0257 μs |  1.28 |    0.03 | 0.0019 |      40 B |        0.01 |
