``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|             Method | Components |        Mean |     Error |    StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** |          **1** |   **118.55 ns** |  **0.892 ns** |  **1.279 ns** |  **1.00** | **0.0119** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |    68.62 ns |  0.190 ns |  0.278 ns |  0.58 |      - |         - |        0.00 |
|                    |            |             |           |           |       |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **279.96 ns** |  **1.832 ns** |  **2.686 ns** |  **1.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   104.30 ns |  0.756 ns |  1.060 ns |  0.37 |      - |         - |        0.00 |
|                    |            |             |           |           |       |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **977.73 ns** |  **9.234 ns** | **13.244 ns** |  **1.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   264.40 ns |  1.834 ns |  2.745 ns |  0.27 |      - |         - |        0.00 |
|                    |            |             |           |           |       |        |           |             |
| **ExecutePipeline_V7** |         **10** | **2,107.56 ns** | **13.311 ns** | **19.923 ns** |  **1.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   525.97 ns |  1.514 ns |  2.266 ns |  0.25 |      - |         - |        0.00 |
