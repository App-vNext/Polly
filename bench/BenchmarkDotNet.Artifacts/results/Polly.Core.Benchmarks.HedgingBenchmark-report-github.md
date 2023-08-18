```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                      Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|             Hedging_Primary | 1.578 μs | 0.0409 μs | 0.0600 μs |  1.00 |    0.00 |      - |      40 B |        1.00 |
|           Hedging_Secondary | 2.593 μs | 0.0139 μs | 0.0200 μs |  1.64 |    0.06 | 0.0076 |     232 B |        5.80 |
|   Hedging_Primary_AsyncWork | 4.177 μs | 0.1108 μs | 0.1589 μs |  2.65 |    0.17 | 0.0534 |    1516 B |       37.90 |
| Hedging_Secondary_AsyncWork | 5.804 μs | 0.1582 μs | 0.2319 μs |  3.68 |    0.20 | 0.0687 |    1888 B |       47.20 |
