```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                      | Mean       | Error    | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |-----------:|---------:|----------:|------:|--------:|-------:|----------:|------------:|
| Hedging_Primary             |   579.2 ns | 12.50 ns |  18.71 ns |  1.00 |    0.05 |      - |         - |          NA |
| Hedging_Secondary           |   983.2 ns | 28.67 ns |  42.03 ns |  1.70 |    0.09 | 0.0191 |     240 B |          NA |
| Hedging_Primary_AsyncWork   | 4,149.2 ns | 38.23 ns |  56.04 ns |  7.17 |    0.25 | 0.1831 |    2347 B |          NA |
| Hedging_Secondary_AsyncWork | 5,138.5 ns | 95.25 ns | 142.57 ns |  8.88 |    0.37 | 0.2060 |    2579 B |          NA |
