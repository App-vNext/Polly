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
| **ExecutePipeline_V7** |          **1** |   **104.6 ns** |  **0.96 ns** |  **1.41 ns** |  **1.00** |    **0.00** | **0.0120** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |   179.6 ns |  2.04 ns |  2.86 ns |  1.72 |    0.04 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **267.0 ns** |  **0.83 ns** |  **1.22 ns** |  **1.00** |    **0.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   236.3 ns |  1.11 ns |  1.63 ns |  0.89 |    0.01 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **901.0 ns** |  **4.79 ns** |  **7.02 ns** |  **1.00** |    **0.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   456.0 ns |  4.72 ns |  6.76 ns |  0.51 |    0.01 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |         **10** | **1,942.6 ns** | **35.76 ns** | **50.14 ns** |  **1.00** |    **0.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   699.6 ns |  3.93 ns |  5.63 ns |  0.36 |    0.01 |      - |         - |        0.00 |
