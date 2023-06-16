``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.304
  [Host] : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|  Method | Telemetry | Enrichment |        Mean |    Error |    StdDev |   Gen0 | Allocated |
|-------- |---------- |----------- |------------:|---------:|----------:|-------:|----------:|
| **Execute** |     **False** |      **False** |    **80.13 ns** | **0.324 ns** |  **0.486 ns** |      **-** |         **-** |
| **Execute** |     **False** |       **True** |    **74.33 ns** | **0.286 ns** |  **0.392 ns** |      **-** |         **-** |
| **Execute** |      **True** |      **False** |   **494.33 ns** | **3.973 ns** |  **5.698 ns** | **0.0029** |      **72 B** |
| **Execute** |      **True** |       **True** | **1,157.27 ns** | **7.130 ns** | **10.450 ns** | **0.0286** |     **728 B** |
