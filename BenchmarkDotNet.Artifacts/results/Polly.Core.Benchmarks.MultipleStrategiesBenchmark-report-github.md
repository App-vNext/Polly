``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                     Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_V7 | 2.027 μs | 0.0282 μs | 0.0413 μs |  1.00 |    0.00 | 0.3357 |    2824 B |        1.00 |
| ExecuteStrategyPipeline_V8 | 1.783 μs | 0.0254 μs | 0.0372 μs |  0.88 |    0.03 | 0.0038 |      40 B |        0.01 |
