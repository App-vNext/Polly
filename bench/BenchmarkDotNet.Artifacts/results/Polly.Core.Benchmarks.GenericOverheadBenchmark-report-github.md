```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                  Method |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |---------:|---------:|---------:|---------:|------:|--------:|----------:|------------:|
|    ExecuteAsync_Generic | 30.07 ns | 0.242 ns | 0.363 ns | 30.07 ns |  1.00 |    0.00 |         - |          NA |
| ExecuteAsync_NonGeneric | 36.89 ns | 5.655 ns | 8.289 ns | 32.49 ns |  1.23 |    0.27 |         - |          NA |
