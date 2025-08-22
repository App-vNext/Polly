```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method             | Components | Mean      | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------- |----------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** | **1**          |  **49.19 ns** |   **0.745 ns** |   **1.115 ns** |  **1.00** |    **0.03** | **0.0242** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 | 1          |  45.41 ns |   0.064 ns |   0.096 ns |  0.92 |    0.02 |      - |         - |        0.00 |
|                    |            |           |            |            |       |         |        |           |             |
| **ExecutePipeline_V7** | **2**          | **107.03 ns** |   **0.403 ns** |   **0.578 ns** |  **1.00** |    **0.01** | **0.0440** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 | 2          |  63.22 ns |   0.866 ns |   1.296 ns |  0.59 |    0.01 |      - |         - |        0.00 |
|                    |            |           |            |            |       |         |        |           |             |
| **ExecutePipeline_V7** | **5**          | **367.20 ns** |  **39.852 ns** |  **59.649 ns** |  **1.03** |    **0.24** | **0.1030** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 | 5          | 169.31 ns |  16.930 ns |  25.340 ns |  0.47 |    0.10 |      - |         - |        0.00 |
|                    |            |           |            |            |       |         |        |           |             |
| **ExecutePipeline_V7** | **10**         | **813.37 ns** | **106.720 ns** | **159.734 ns** |  **1.04** |    **0.31** | **0.2003** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 | 10         | 362.39 ns |  50.165 ns |  75.085 ns |  0.47 |    0.14 |      - |         - |        0.00 |
