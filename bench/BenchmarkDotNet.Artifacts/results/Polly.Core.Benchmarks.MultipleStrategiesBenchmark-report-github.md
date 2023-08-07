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
|             ExecuteStrategyPipeline_Generic_V7 | 2.318 μs | 0.0253 μs | 0.0355 μs |  1.00 |    0.00 | 0.1106 |    2824 B |        1.00 |
|             ExecuteStrategyPipeline_Generic_V8 | 2.044 μs | 0.0095 μs | 0.0140 μs |  0.88 |    0.02 |      - |      72 B |        0.03 |
|    ExecuteStrategyPipeline_GenericTelemetry_V8 | 2.950 μs | 0.0059 μs | 0.0089 μs |  1.27 |    0.02 |      - |      72 B |        0.03 |
|          ExecuteStrategyPipeline_NonGeneric_V8 | 2.295 μs | 0.0176 μs | 0.0264 μs |  0.99 |    0.01 |      - |      72 B |        0.03 |
| ExecuteStrategyPipeline_NonGenericTelemetry_V8 | 3.153 μs | 0.0058 μs | 0.0085 μs |  1.36 |    0.02 |      - |      72 B |        0.03 |
