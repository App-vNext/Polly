```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method  | Telemetry | Enrichment | Mean      | Error    | StdDev   | Allocated |
|-------- |---------- |----------- |----------:|---------:|---------:|----------:|
| **Execute** | **False**     | **False**      |  **60.58 ns** | **0.178 ns** | **0.256 ns** |         **-** |
| **Execute** | **False**     | **True**       |  **62.53 ns** | **1.442 ns** | **2.159 ns** |         **-** |
| **Execute** | **True**      | **False**      | **443.92 ns** | **1.736 ns** | **2.434 ns** |         **-** |
| **Execute** | **True**      | **True**       | **612.90 ns** | **2.974 ns** | **4.359 ns** |         **-** |
