``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|             Method | Components |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** |          **1** |   **110.61 ns** |  **4.817 ns** |  **7.060 ns** |   **107.53 ns** |  **1.00** |    **0.00** | **0.0120** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |    68.63 ns |  0.583 ns |  0.778 ns |    68.32 ns |  0.62 |    0.04 |      - |         - |        0.00 |
|                    |            |             |           |           |             |       |         |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **267.80 ns** |  **3.493 ns** |  **5.228 ns** |   **269.12 ns** |  **1.00** |    **0.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   118.35 ns |  5.882 ns |  8.246 ns |   124.97 ns |  0.44 |    0.04 |      - |         - |        0.00 |
|                    |            |             |           |           |             |       |         |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **880.79 ns** |  **9.145 ns** | **13.405 ns** |   **877.94 ns** |  **1.00** |    **0.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   255.76 ns |  1.435 ns |  2.149 ns |   255.36 ns |  0.29 |    0.00 |      - |         - |        0.00 |
|                    |            |             |           |           |             |       |         |        |           |             |
| **ExecutePipeline_V7** |         **10** | **1,882.96 ns** | **15.883 ns** | **23.773 ns** | **1,885.28 ns** |  **1.00** |    **0.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   524.46 ns |  1.388 ns |  2.034 ns |   524.45 ns |  0.28 |    0.00 |      - |         - |        0.00 |
