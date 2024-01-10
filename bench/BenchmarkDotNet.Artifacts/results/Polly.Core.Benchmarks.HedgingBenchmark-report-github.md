```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Hedging_Primary             |  1.139 μs | 0.0058 μs | 0.0087 μs |  1.00 |    0.00 |      - |      40 B |        1.00 |
| Hedging_Secondary           |  1.789 μs | 0.0262 μs | 0.0393 μs |  1.57 |    0.04 | 0.0095 |     280 B |        7.00 |
| Hedging_Primary_AsyncWork   |  6.274 μs | 0.4927 μs | 0.7374 μs |  5.51 |    0.65 | 0.0916 |    2325 B |       58.12 |
| Hedging_Secondary_AsyncWork | 12.488 μs | 0.7955 μs | 1.1907 μs | 10.97 |    1.07 | 0.0916 |    2606 B |       65.15 |
