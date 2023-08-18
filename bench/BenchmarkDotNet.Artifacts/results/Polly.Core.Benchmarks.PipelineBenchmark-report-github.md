```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|             Method | Components |       Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** |          **1** |   **108.5 ns** |  **2.65 ns** |  **3.96 ns** |  **1.00** |    **0.00** | **0.0120** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 |          1 |   103.9 ns |  1.96 ns |  2.68 ns |  0.96 |    0.03 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |          **2** |   **261.3 ns** |  **3.89 ns** |  **5.83 ns** |  **1.00** |    **0.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 |          2 |   140.8 ns |  3.49 ns |  5.01 ns |  0.54 |    0.03 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |          **5** |   **876.3 ns** |  **5.80 ns** |  **8.13 ns** |  **1.00** |    **0.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 |          5 |   362.9 ns |  2.66 ns |  3.99 ns |  0.41 |    0.01 |      - |         - |        0.00 |
|                    |            |            |          |          |       |         |        |           |             |
| **ExecutePipeline_V7** |         **10** | **1,897.9 ns** | **13.50 ns** | **19.37 ns** |  **1.00** |    **0.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 |         10 |   714.8 ns |  3.27 ns |  4.69 ns |  0.38 |    0.00 |      - |         - |        0.00 |
