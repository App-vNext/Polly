``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|  Method | Telemetry |      Mean |    Error |   StdDev |   Gen0 | Allocated |
|-------- |---------- |----------:|---------:|---------:|-------:|----------:|
| **Execute** |     **False** |  **69.25 ns** | **0.279 ns** | **0.373 ns** |      **-** |         **-** |
|   Retry |     False | 323.60 ns | 3.308 ns | 4.951 ns |      - |         - |
| **Execute** |      **True** | **348.06 ns** | **2.681 ns** | **4.013 ns** |      **-** |         **-** |
|   Retry |      True | 844.17 ns | 2.915 ns | 4.181 ns | 0.0038 |     104 B |
