```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method  | Telemetry | Enrichment | Mean      | Error    | StdDev    | Median    | Allocated |
|-------- |---------- |----------- |----------:|---------:|----------:|----------:|----------:|
| **Execute** | **False**     | **False**      |  **70.29 ns** | **1.936 ns** |  **2.651 ns** |  **69.94 ns** |         **-** |
| **Execute** | **False**     | **True**       |  **70.77 ns** | **0.214 ns** |  **0.300 ns** |  **70.68 ns** |         **-** |
| **Execute** | **True**      | **False**      | **675.67 ns** | **3.703 ns** |  **5.542 ns** | **675.58 ns** |         **-** |
| **Execute** | **True**      | **True**       | **883.36 ns** | **8.840 ns** | **12.957 ns** | **872.21 ns** |         **-** |
