``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.304
  [Host] : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                               Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|           ExecuteStrategyPipeline_V7 | 2.220 μs | 0.0164 μs | 0.0236 μs |  1.00 |    0.00 | 0.1106 |    2824 B |        1.00 |
|           ExecuteStrategyPipeline_V8 | 1.901 μs | 0.0089 μs | 0.0127 μs |  0.86 |    0.01 |      - |      40 B |        0.01 |
| ExecuteStrategyPipeline_Telemetry_V8 | 2.947 μs | 0.0077 μs | 0.0115 μs |  1.33 |    0.02 |      - |      40 B |        0.01 |
