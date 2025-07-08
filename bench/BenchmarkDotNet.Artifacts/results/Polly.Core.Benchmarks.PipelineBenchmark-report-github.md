```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method             | Components | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** | **1**          |    **60.32 ns** |  **1.079 ns** |  **1.615 ns** |  **1.00** |    **0.04** | **0.0242** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 | 1          |    51.35 ns |  0.074 ns |  0.110 ns |  0.85 |    0.02 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| **ExecutePipeline_V7** | **2**          |   **129.42 ns** |  **3.163 ns** |  **4.636 ns** |  **1.00** |    **0.05** | **0.0439** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 | 2          |    72.20 ns |  0.222 ns |  0.326 ns |  0.56 |    0.02 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| **ExecutePipeline_V7** | **5**          |   **532.96 ns** | **18.679 ns** | **27.958 ns** |  **1.00** |    **0.08** | **0.1030** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 | 5          |   235.23 ns |  3.250 ns |  4.864 ns |  0.44 |    0.03 |      - |         - |        0.00 |
|                    |            |             |           |           |       |         |        |           |             |
| **ExecutePipeline_V7** | **10**         | **1,201.79 ns** | **27.474 ns** | **41.122 ns** |  **1.00** |    **0.05** | **0.2003** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 | 10         |   531.81 ns | 35.576 ns | 53.249 ns |  0.44 |    0.05 |      - |         - |        0.00 |
