```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method             | Components | Mean        | Error    | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|---------:|----------:|------:|-------:|----------:|------------:|
| **ExecutePipeline_V7** | **1**          |    **90.13 ns** | **0.834 ns** |  **1.249 ns** |  **1.00** | **0.0120** |     **304 B** |        **1.00** |
| ExecutePipeline_V8 | 1          |    73.47 ns | 0.254 ns |  0.373 ns |  0.82 |      - |         - |        0.00 |
|                    |            |             |          |           |       |        |           |             |
| **ExecutePipeline_V7** | **2**          |   **233.68 ns** | **1.506 ns** |  **2.208 ns** |  **1.00** | **0.0219** |     **552 B** |        **1.00** |
| ExecutePipeline_V8 | 2          |   114.89 ns | 0.212 ns |  0.291 ns |  0.49 |      - |         - |        0.00 |
|                    |            |             |          |           |       |        |           |             |
| **ExecutePipeline_V7** | **5**          |   **778.86 ns** | **4.387 ns** |  **6.566 ns** |  **1.00** | **0.0515** |    **1296 B** |        **1.00** |
| ExecutePipeline_V8 | 5          |   374.64 ns | 0.452 ns |  0.619 ns |  0.48 |      - |         - |        0.00 |
|                    |            |             |          |           |       |        |           |             |
| **ExecutePipeline_V7** | **10**         | **1,706.53 ns** | **8.257 ns** | **12.359 ns** |  **1.00** | **0.0992** |    **2536 B** |        **1.00** |
| ExecutePipeline_V8 | 10         |   775.52 ns | 3.341 ns |  4.792 ns |  0.45 |      - |         - |        0.00 |
