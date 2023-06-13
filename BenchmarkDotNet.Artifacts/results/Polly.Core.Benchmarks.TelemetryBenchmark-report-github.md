``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|  Method | Telemetry |        Mean |      Error |     StdDev |    Median |   Gen0 | Allocated |
|-------- |---------- |------------:|-----------:|-----------:|----------:|-------:|----------:|
| **Execute** |     **False** |    **90.27 ns** |   **0.823 ns** |   **1.207 ns** |  **90.34 ns** |      **-** |         **-** |
|   Retry |     False |   435.25 ns |   8.593 ns |  12.595 ns | 433.63 ns |      - |         - |
| **Execute** |      **True** |   **311.60 ns** |   **4.107 ns** |   **5.890 ns** | **311.37 ns** |      **-** |         **-** |
|   Retry |      True | 1,004.07 ns | 106.845 ns | 159.921 ns | 922.31 ns | 0.0124 |     104 B |
