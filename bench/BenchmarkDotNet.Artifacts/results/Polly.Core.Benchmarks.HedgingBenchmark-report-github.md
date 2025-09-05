```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                      | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Hedging_Primary             |   394.6 ns | 10.42 ns | 15.60 ns |  1.00 |    0.06 |      - |         - |          NA |
| Hedging_Secondary           |   655.0 ns |  6.08 ns |  9.10 ns |  1.66 |    0.07 | 0.0191 |     240 B |          NA |
| Hedging_Primary_AsyncWork   | 1,854.6 ns | 11.50 ns | 17.21 ns |  4.71 |    0.19 | 0.1011 |    1289 B |          NA |
| Hedging_Secondary_AsyncWork | 2,299.9 ns | 15.49 ns | 23.18 ns |  5.84 |    0.24 | 0.1221 |    1546 B |          NA |
