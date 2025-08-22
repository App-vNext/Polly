```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method  | Telemetry | Enrichment | Mean      | Error    | StdDev   | Allocated |
|-------- |---------- |----------- |----------:|---------:|---------:|----------:|
| **Execute** | **False**     | **False**      |  **39.50 ns** | **0.864 ns** | **1.212 ns** |         **-** |
| **Execute** | **False**     | **True**       |  **41.71 ns** | **0.242 ns** | **0.354 ns** |         **-** |
| **Execute** | **True**      | **False**      | **271.68 ns** | **3.107 ns** | **4.650 ns** |         **-** |
| **Execute** | **True**      | **True**       | **323.94 ns** | **2.002 ns** | **2.996 ns** |         **-** |
