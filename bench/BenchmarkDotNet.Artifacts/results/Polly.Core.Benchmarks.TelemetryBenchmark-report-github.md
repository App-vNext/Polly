```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|  Method | Telemetry | Enrichment |       Mean |    Error |   StdDev |     Median | Allocated |
|-------- |---------- |----------- |-----------:|---------:|---------:|-----------:|----------:|
| **Execute** |     **False** |      **False** |   **102.2 ns** |  **0.37 ns** |  **0.54 ns** |   **101.9 ns** |         **-** |
| **Execute** |     **False** |       **True** |   **104.9 ns** |  **0.40 ns** |  **0.56 ns** |   **105.3 ns** |         **-** |
| **Execute** |      **True** |      **False** |   **838.8 ns** |  **5.01 ns** |  **7.50 ns** |   **836.5 ns** |         **-** |
| **Execute** |      **True** |       **True** | **1,279.6 ns** | **10.57 ns** | **15.82 ns** | **1,276.6 ns** |         **-** |
