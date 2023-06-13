``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|  Method | Telemetry |      Mean |    Error |   StdDev |    Median |   Gen0 | Allocated |
|-------- |---------- |----------:|---------:|---------:|----------:|-------:|----------:|
| **Execute** |     **False** |  **71.49 ns** | **2.474 ns** | **3.627 ns** |  **69.31 ns** |      **-** |         **-** |
|   Retry |     False | 356.87 ns | 1.355 ns | 1.943 ns | 356.15 ns |      - |         - |
| **Execute** |      **True** | **280.70 ns** | **4.106 ns** | **5.756 ns** | **278.32 ns** |      **-** |         **-** |
|   Retry |      True | 847.76 ns | 3.870 ns | 5.297 ns | 848.03 ns | 0.0038 |     104 B |
