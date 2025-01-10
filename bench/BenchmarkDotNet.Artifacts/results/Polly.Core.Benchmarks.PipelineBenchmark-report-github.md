```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method             | Components | Mean      | Error    | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------- |----------:|---------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** | **1**          |  **69.79 ns** | **0.588 ns** |  **0.880 ns** |  **1.00** |    **0.02** | **0.0242** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 | 1          |  69.75 ns | 0.290 ns |  0.406 ns |  1.00 |    0.01 |      - |         - |        0.00 |
|                    |            |           |          |           |       |         |        |           |             |
| **ExecutePipeline_V7** | **2**          | **151.17 ns** | **1.092 ns** |  **1.566 ns** |  **1.00** |    **0.01** | **0.0439** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 | 2          |  99.85 ns | 0.617 ns |  0.884 ns |  0.66 |    0.01 |      - |         - |        0.00 |
|                    |            |           |          |           |       |         |        |           |             |
| **ExecutePipeline_V7** | **5**          | **409.27 ns** | **3.791 ns** |  **5.314 ns** |  **1.00** |    **0.02** | **0.1030** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 | 5          | 208.08 ns | 0.874 ns |  1.280 ns |  0.51 |    0.01 |      - |         - |        0.00 |
|                    |            |           |          |           |       |         |        |           |             |
| **ExecutePipeline_V7** | **10**         | **822.12 ns** | **6.901 ns** | **10.115 ns** |  **1.00** |    **0.02** | **0.2012** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 | 10         | 399.00 ns | 1.668 ns |  2.393 ns |  0.49 |    0.01 |      - |         - |        0.00 |
