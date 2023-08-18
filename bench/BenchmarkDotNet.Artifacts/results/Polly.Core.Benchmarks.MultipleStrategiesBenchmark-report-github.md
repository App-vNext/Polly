```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                                         Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|             ExecuteStrategyPipeline_Generic_V7 | 2.357 μs | 0.0277 μs | 0.0406 μs |  1.00 |    0.00 | 0.1106 |    2824 B |        1.00 |
|             ExecuteStrategyPipeline_Generic_V8 | 2.237 μs | 0.0235 μs | 0.0344 μs |  0.95 |    0.03 |      - |      40 B |        0.01 |
|    ExecuteStrategyPipeline_GenericTelemetry_V8 | 3.489 μs | 0.0342 μs | 0.0501 μs |  1.48 |    0.03 |      - |      40 B |        0.01 |
|          ExecuteStrategyPipeline_NonGeneric_V8 | 2.457 μs | 0.0137 μs | 0.0193 μs |  1.04 |    0.02 |      - |      40 B |        0.01 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 3.777 μs | 0.0361 μs | 0.0541 μs |  1.60 |    0.02 |      - |      40 B |        0.01 |
