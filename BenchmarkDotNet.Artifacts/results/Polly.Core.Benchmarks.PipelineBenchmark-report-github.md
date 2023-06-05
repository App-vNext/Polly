``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|             Method | Components |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** |          **1** |    **93.17 ns** |  **1.232 ns** |  **1.766 ns** |  **1.00** |    **0.00** | **0.0120** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |    67.67 ns |  0.578 ns |  0.847 ns |  0.73 |    0.01 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **242.68 ns** |  **5.866 ns** |  **8.779 ns** |  **1.00** |    **0.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   104.03 ns |  0.132 ns |  0.197 ns |  0.43 |    0.02 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **871.60 ns** | **14.359 ns** | **21.492 ns** |  **1.00** |    **0.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   274.64 ns |  2.091 ns |  3.130 ns |  0.32 |    0.01 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| **ExecutePipeline_V7** |         **10** | **1,791.84 ns** | **15.425 ns** | **22.123 ns** |  **1.00** |    **0.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   527.94 ns |  2.723 ns |  3.905 ns |  0.29 |    0.00 |      - |         - |        0.00 |
