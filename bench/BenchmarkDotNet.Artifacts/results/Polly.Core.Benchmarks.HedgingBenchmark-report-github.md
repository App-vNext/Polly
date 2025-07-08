```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                      | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Hedging_Primary             |   583.6 ns |   8.73 ns |  13.06 ns |  1.00 |    0.03 |      - |         - |          NA |
| Hedging_Secondary           |   952.5 ns |  13.33 ns |  19.95 ns |  1.63 |    0.05 | 0.0191 |     240 B |          NA |
| Hedging_Primary_AsyncWork   | 4,219.6 ns |  70.46 ns | 101.05 ns |  7.23 |    0.24 | 0.1831 |    2343 B |          NA |
| Hedging_Secondary_AsyncWork | 5,214.9 ns | 225.20 ns | 322.98 ns |  8.94 |    0.59 | 0.2060 |    2587 B |          NA |
