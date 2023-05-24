``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                     Method |     Mean |     Error |    StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|--------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| ExecuteStrategyPipeline_V7 | 2.178 μs | 0.0167 μs | 0.0249 μs |  1.00 | 0.1106 |    2824 B |        1.00 |
| ExecuteStrategyPipeline_V8 | 1.687 μs | 0.0028 μs | 0.0041 μs |  0.77 | 0.0019 |      72 B |        0.03 |
