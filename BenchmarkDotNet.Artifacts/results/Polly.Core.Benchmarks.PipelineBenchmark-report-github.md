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
| **ExecutePipeline_V7** |          **1** |   **114.38 ns** |  **0.847 ns** |  **1.241 ns** |  **1.00** | **0.0119** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |    68.18 ns |  0.188 ns |  0.282 ns |  0.60 |      - |         - |        0.00 |
|                    |            |             |           |           |       |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **287.17 ns** |  **4.366 ns** |  **6.535 ns** |  **1.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   106.99 ns |  0.977 ns |  1.304 ns |  0.37 |      - |         - |        0.00 |
|                    |            |             |           |           |       |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **978.19 ns** |  **9.237 ns** | **13.247 ns** |  **1.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   279.21 ns |  2.244 ns |  3.219 ns |  0.29 |      - |         - |        0.00 |
|                    |            |             |           |           |       |        |           |             |
| **ExecutePipeline_V7** |         **10** | **2,179.96 ns** | **43.086 ns** | **63.154 ns** |  **1.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   532.87 ns |  5.880 ns |  8.433 ns |  0.24 |      - |         - |        0.00 |
