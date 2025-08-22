```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                      | Mean       | Error    | StdDev   | Median     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |-----------:|---------:|---------:|-----------:|------:|--------:|-------:|----------:|------------:|
| Hedging_Primary             |   480.9 ns | 19.69 ns | 29.47 ns |   496.7 ns |  1.00 |    0.09 |      - |         - |          NA |
| Hedging_Secondary           |   784.6 ns | 25.82 ns | 38.65 ns |   788.3 ns |  1.64 |    0.13 | 0.0191 |     240 B |          NA |
| Hedging_Primary_AsyncWork   | 3,119.4 ns | 35.11 ns | 51.46 ns | 3,113.1 ns |  6.51 |    0.42 | 0.1831 |    2338 B |          NA |
| Hedging_Secondary_AsyncWork | 3,581.9 ns | 47.02 ns | 64.37 ns | 3,590.9 ns |  7.48 |    0.49 | 0.2022 |    2566 B |          NA |
