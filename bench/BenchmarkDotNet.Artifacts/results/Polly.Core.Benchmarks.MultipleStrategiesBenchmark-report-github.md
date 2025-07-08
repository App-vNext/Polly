```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_Generic_V7             | 1.347 μs | 0.0219 μs | 0.0328 μs |  1.00 |    0.03 | 0.2174 |    2744 B |        1.00 |
| ExecuteStrategyPipeline_Generic_V8             | 1.179 μs | 0.0266 μs | 0.0398 μs |  0.88 |    0.04 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_GenericTelemetry_V8    | 1.653 μs | 0.0277 μs | 0.0415 μs |  1.23 |    0.04 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGeneric_V8          | 1.285 μs | 0.0184 μs | 0.0276 μs |  0.96 |    0.03 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 1.739 μs | 0.0475 μs | 0.0711 μs |  1.29 |    0.06 | 0.0019 |      40 B |        0.01 |
