```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method  | Telemetry | Enrichment | Mean      | Error     | StdDev    | Allocated |
|-------- |---------- |----------- |----------:|----------:|----------:|----------:|
| **Execute** | **False**     | **False**      |  **43.26 ns** |  **0.092 ns** |  **0.138 ns** |         **-** |
| **Execute** | **False**     | **True**       |  **44.97 ns** |  **1.299 ns** |  **1.862 ns** |         **-** |
| **Execute** | **True**      | **False**      | **316.54 ns** |  **2.804 ns** |  **4.198 ns** |         **-** |
| **Execute** | **True**      | **True**       | **435.78 ns** | **12.809 ns** | **19.172 ns** |         **-** |
