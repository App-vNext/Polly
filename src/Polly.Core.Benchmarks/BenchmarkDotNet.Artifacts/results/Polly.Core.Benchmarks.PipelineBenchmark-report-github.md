``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|             Method | Components |       Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** |          **1** |   **100.3 ns** |  **1.64 ns** |  **2.46 ns** |  **1.00** |    **0.00** | **0.0120** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |   118.7 ns |  2.10 ns |  2.88 ns |  1.18 |    0.03 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **242.1 ns** |  **1.94 ns** |  **2.79 ns** |  **1.00** |    **0.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   155.6 ns |  0.38 ns |  0.54 ns |  0.64 |    0.01 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **846.3 ns** |  **6.82 ns** | **10.21 ns** |  **1.00** |    **0.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   316.1 ns |  1.33 ns |  1.99 ns |  0.37 |    0.01 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |         **10** | **1,846.1 ns** | **14.05 ns** | **21.04 ns** |  **1.00** |    **0.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   600.9 ns |  4.50 ns |  6.74 ns |  0.33 |    0.01 |      - |         - |        0.00 |
