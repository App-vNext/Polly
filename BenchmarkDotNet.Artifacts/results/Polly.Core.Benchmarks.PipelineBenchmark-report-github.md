``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|             Method | Components |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** |          **1** |   **104.35 ns** |  **4.445 ns** |  **6.515 ns** |  **1.00** |    **0.00** | **0.0362** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |    80.16 ns |  2.307 ns |  3.309 ns |  0.77 |    0.05 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **274.03 ns** |  **6.359 ns** |  **9.120 ns** |  **1.00** |    **0.00** | **0.0658** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   125.66 ns |  4.803 ns |  6.574 ns |  0.46 |    0.03 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **838.28 ns** | **57.934 ns** | **86.713 ns** |  **1.00** |    **0.00** | **0.1545** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   257.46 ns |  3.722 ns |  5.570 ns |  0.31 |    0.03 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| **ExecutePipeline_V7** |         **10** | **1,652.98 ns** | **53.504 ns** | **75.005 ns** |  **1.00** |    **0.00** | **0.3014** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   477.94 ns | 16.290 ns | 23.878 ns |  0.29 |    0.02 |      - |         - |        0.00 |
