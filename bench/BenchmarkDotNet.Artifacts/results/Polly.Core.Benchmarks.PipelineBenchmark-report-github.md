```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|             Method | Components |        Mean |     Error |    StdDev |     Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** |          **1** |   **121.39 ns** |  **2.173 ns** |  **3.253 ns** |   **122.0 ns** |  **1.00** |    **0.00** | **0.0120** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |    98.02 ns |  2.387 ns |  3.499 ns |   101.3 ns |  0.81 |    0.03 |      - |         - |        0.00 |
|                    |            |             |           |           |            |       |         |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **278.13 ns** |  **3.703 ns** |  **5.428 ns** |   **279.0 ns** |  **1.00** |    **0.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   130.69 ns |  1.098 ns |  1.610 ns |   129.4 ns |  0.47 |    0.01 |      - |         - |        0.00 |
|                    |            |             |           |           |            |       |         |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **916.09 ns** |  **7.794 ns** | **11.666 ns** |   **917.8 ns** |  **1.00** |    **0.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   324.87 ns |  1.751 ns |  2.621 ns |   324.5 ns |  0.35 |    0.01 |      - |         - |        0.00 |
|                    |            |             |           |           |            |       |         |        |           |             |
| **ExecutePipeline_V7** |         **10** | **1,863.84 ns** | **10.049 ns** | **15.041 ns** | **1,865.4 ns** |  **1.00** |    **0.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   701.09 ns |  0.939 ns |  1.346 ns |   700.7 ns |  0.38 |    0.00 |      - |         - |        0.00 |
