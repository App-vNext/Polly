``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.304
  [Host] : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|  Method | Telemetry | Enrichment |        Mean |    Error |   StdDev | Allocated |
|-------- |---------- |----------- |------------:|---------:|---------:|----------:|
| **Execute** |     **False** |      **False** |    **80.27 ns** | **1.992 ns** | **2.920 ns** |         **-** |
| **Execute** |     **False** |       **True** |    **79.68 ns** | **1.324 ns** | **1.982 ns** |         **-** |
| **Execute** |      **True** |      **False** |   **750.41 ns** | **4.875 ns** | **6.673 ns** |         **-** |
| **Execute** |      **True** |       **True** | **1,034.73 ns** | **4.941 ns** | **7.242 ns** |         **-** |
