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
| **ExecutePipeline_V7** |          **1** |   **112.6 ns** |  **3.90 ns** |  **5.83 ns** |  **1.00** |    **0.00** | **0.0120** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |   114.7 ns |  1.43 ns |  2.14 ns |  1.02 |    0.06 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **281.6 ns** |  **4.78 ns** |  **7.15 ns** |  **1.00** |    **0.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   155.8 ns |  1.00 ns |  1.50 ns |  0.55 |    0.02 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **921.0 ns** |  **6.42 ns** |  **9.00 ns** |  **1.00** |    **0.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   346.5 ns |  1.09 ns |  1.53 ns |  0.38 |    0.00 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |         **10** | **1,982.9 ns** | **63.37 ns** | **86.74 ns** |  **1.00** |    **0.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   610.2 ns |  3.49 ns |  5.01 ns |  0.31 |    0.01 |      - |         - |        0.00 |
