```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|  Method | Telemetry | Enrichment |       Mean |   Error |  StdDev |     Median | Allocated |
|-------- |---------- |----------- |-----------:|--------:|--------:|-----------:|----------:|
| **Execute** |     **False** |      **False** |   **110.6 ns** | **1.95 ns** | **2.73 ns** |   **111.6 ns** |         **-** |
| **Execute** |     **False** |       **True** |   **113.2 ns** | **1.47 ns** | **2.11 ns** |   **114.3 ns** |         **-** |
| **Execute** |      **True** |      **False** |   **988.1 ns** | **5.08 ns** | **7.28 ns** |   **987.2 ns** |         **-** |
| **Execute** |      **True** |       **True** | **1,403.0 ns** | **4.47 ns** | **6.26 ns** | **1,400.6 ns** |         **-** |
