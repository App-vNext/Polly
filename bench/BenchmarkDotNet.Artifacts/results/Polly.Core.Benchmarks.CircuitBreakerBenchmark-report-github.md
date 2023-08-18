```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                   Method |     Mean |    Error |    StdDev |   Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |---------:|---------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 306.1 ns |  2.26 ns |   3.31 ns | 305.9 ns |  1.00 |    0.00 | 0.0200 |     504 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 676.4 ns | 79.03 ns | 115.84 ns | 610.7 ns |  2.21 |    0.36 |      - |         - |        0.00 |
