```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method  | Telemetry | Enrichment | Mean      | Error    | StdDev    | Median    | Allocated |
|-------- |---------- |----------- |----------:|---------:|----------:|----------:|----------:|
| **Execute** | **False**     | **False**      |  **50.75 ns** | **1.532 ns** |  **2.246 ns** |  **49.73 ns** |         **-** |
| **Execute** | **False**     | **True**       |  **49.42 ns** | **0.135 ns** |  **0.189 ns** |  **49.36 ns** |         **-** |
| **Execute** | **True**      | **False**      | **382.94 ns** | **9.059 ns** | **12.992 ns** | **394.03 ns** |         **-** |
| **Execute** | **True**      | **True**       | **526.66 ns** | **1.216 ns** |  **1.783 ns** | **526.85 ns** |         **-** |
