```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method             | Components | Mean        | Error     | StdDev    | Median      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** | **1**          |    **59.65 ns** |  **2.963 ns** |  **4.250 ns** |    **58.39 ns** |  **1.00** |    **0.10** | **0.0242** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 | 1          |    52.51 ns |  0.093 ns |  0.133 ns |    52.49 ns |  0.88 |    0.06 |      - |         - |        0.00 |
|                    |            |             |           |           |             |       |         |        |           |             |
| **ExecutePipeline_V7** | **2**          |   **125.93 ns** |  **0.674 ns** |  **0.988 ns** |   **125.59 ns** |  **1.00** |    **0.01** | **0.0439** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 | 2          |    74.07 ns |  0.207 ns |  0.303 ns |    73.91 ns |  0.59 |    0.01 |      - |         - |        0.00 |
|                    |            |             |           |           |             |       |         |        |           |             |
| **ExecutePipeline_V7** | **5**          |   **517.61 ns** | **20.857 ns** | **31.218 ns** |   **525.89 ns** |  **1.00** |    **0.10** | **0.1030** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 | 5          |   229.02 ns |  8.340 ns | 12.483 ns |   232.35 ns |  0.44 |    0.04 |      - |         - |        0.00 |
|                    |            |             |           |           |             |       |         |        |           |             |
| **ExecutePipeline_V7** | **10**         | **1,175.56 ns** | **56.978 ns** | **85.282 ns** | **1,206.15 ns** |  **1.01** |    **0.11** | **0.2003** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 | 10         |   550.37 ns | 12.148 ns | 17.806 ns |   558.74 ns |  0.47 |    0.04 |      - |         - |        0.00 |
