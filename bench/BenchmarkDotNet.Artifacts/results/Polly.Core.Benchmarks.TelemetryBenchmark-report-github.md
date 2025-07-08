```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method  | Telemetry | Enrichment | Mean      | Error    | StdDev   | Median    | Allocated |
|-------- |---------- |----------- |----------:|---------:|---------:|----------:|----------:|
| **Execute** | **False**     | **False**      |  **54.95 ns** | **4.280 ns** | **6.000 ns** |  **60.09 ns** |         **-** |
| **Execute** | **False**     | **True**       |  **49.31 ns** | **0.741 ns** | **1.062 ns** |  **49.71 ns** |         **-** |
| **Execute** | **True**      | **False**      | **386.56 ns** | **1.374 ns** | **2.015 ns** | **385.19 ns** |         **-** |
| **Execute** | **True**      | **True**       | **544.12 ns** | **1.343 ns** | **2.010 ns** | **544.67 ns** |         **-** |
