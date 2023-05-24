``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|             Method | Components |        Mean |    Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|---------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** |          **1** |   **104.86 ns** | **3.128 ns** |  **4.683 ns** |  **1.00** |    **0.00** | **0.0120** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |    67.86 ns | 1.087 ns |  1.489 ns |  0.65 |    0.02 |      - |         - |        0.00 |
|                    |            |             |          |           |       |         |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **248.17 ns** | **2.472 ns** |  **3.700 ns** |  **1.00** |    **0.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   111.45 ns | 2.249 ns |  3.297 ns |  0.45 |    0.01 |      - |         - |        0.00 |
|                    |            |             |          |           |       |         |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **869.30 ns** | **7.395 ns** | **10.605 ns** |  **1.00** |    **0.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   248.32 ns | 1.181 ns |  1.731 ns |  0.29 |    0.00 |      - |         - |        0.00 |
|                    |            |             |          |           |       |         |        |           |             |
| **ExecutePipeline_V7** |         **10** | **1,879.12 ns** | **7.587 ns** | **11.356 ns** |  **1.00** |    **0.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   521.97 ns | 2.568 ns |  3.844 ns |  0.28 |    0.00 |      - |         - |        0.00 |
