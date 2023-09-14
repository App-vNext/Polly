```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                      Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|             Hedging_Primary | 1.432 μs | 0.0042 μs | 0.0061 μs |  1.00 |    0.00 |      - |      40 B |        1.00 |
|           Hedging_Secondary | 2.253 μs | 0.0051 μs | 0.0075 μs |  1.57 |    0.01 | 0.0038 |     184 B |        4.60 |
|   Hedging_Primary_AsyncWork | 3.903 μs | 0.0260 μs | 0.0381 μs |  2.73 |    0.03 | 0.0610 |    1636 B |       40.90 |
| Hedging_Secondary_AsyncWork | 4.936 μs | 0.0424 μs | 0.0595 μs |  3.45 |    0.05 | 0.0687 |    1838 B |       45.95 |
