```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean     | Error     | StdDev    | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|----------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_Generic_V7             | 1.180 μs | 0.0082 μs | 0.0122 μs | 1.179 μs |  1.00 |    0.01 | 0.2174 |    2744 B |        1.00 |
| ExecuteStrategyPipeline_Generic_V8             | 1.082 μs | 0.0128 μs | 0.0180 μs | 1.078 μs |  0.92 |    0.02 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_GenericTelemetry_V8    | 1.566 μs | 0.0178 μs | 0.0255 μs | 1.552 μs |  1.33 |    0.03 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGeneric_V8          | 1.150 μs | 0.0052 μs | 0.0075 μs | 1.148 μs |  0.97 |    0.01 | 0.0019 |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 1.672 μs | 0.0075 μs | 0.0110 μs | 1.672 μs |  1.42 |    0.02 | 0.0019 |      40 B |        0.01 |
