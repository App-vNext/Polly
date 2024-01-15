```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                  | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |---------:|---------:|---------:|---------:|------:|--------:|----------:|------------:|
| ExecuteAsync_Generic    | 11.58 ns | 0.220 ns | 0.323 ns | 11.75 ns |  1.00 |    0.00 |         - |          NA |
| ExecuteAsync_NonGeneric | 19.25 ns | 0.026 ns | 0.038 ns | 19.25 ns |  1.66 |    0.05 |         - |          NA |
