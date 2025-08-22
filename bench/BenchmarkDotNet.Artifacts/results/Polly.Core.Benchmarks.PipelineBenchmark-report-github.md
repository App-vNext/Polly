```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method             | Components | Mean      | Error     | StdDev     | Median    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------- |----------:|----------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** | **1**          |  **46.43 ns** |  **0.916 ns** |   **1.313 ns** |  **46.53 ns** |  **1.00** |    **0.04** | **0.0242** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 | 1          |  43.76 ns |  0.293 ns |   0.438 ns |  43.51 ns |  0.94 |    0.03 |      - |         - |        0.00 |
|                    |            |           |           |            |           |       |         |        |           |             |
| **ExecutePipeline_V7** | **2**          | **104.95 ns** |  **0.426 ns** |   **0.625 ns** | **104.88 ns** |  **1.00** |    **0.01** | **0.0440** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 | 2          |  59.60 ns |  0.697 ns |   1.044 ns |  59.78 ns |  0.57 |    0.01 |      - |         - |        0.00 |
|                    |            |           |           |            |           |       |         |        |           |             |
| **ExecutePipeline_V7** | **5**          | **387.16 ns** | **30.254 ns** |  **45.283 ns** | **388.12 ns** |  **1.01** |    **0.17** | **0.1030** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 | 5          | 159.45 ns | 12.943 ns |  19.373 ns | 162.79 ns |  0.42 |    0.07 |      - |         - |        0.00 |
|                    |            |           |           |            |           |       |         |        |           |             |
| **ExecutePipeline_V7** | **10**         | **771.09 ns** | **85.457 ns** | **127.908 ns** | **775.33 ns** |  **1.03** |    **0.25** | **0.2012** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 | 10         | 347.47 ns | 44.705 ns |  66.912 ns | 346.01 ns |  0.46 |    0.12 |      - |         - |        0.00 |
