```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                      | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Hedging_Primary             |   684.6 ns |  25.04 ns |  36.71 ns |  1.00 |    0.07 |      - |         - |          NA |
| Hedging_Secondary           | 1,192.5 ns |  24.41 ns |  34.21 ns |  1.75 |    0.10 | 0.0191 |     240 B |          NA |
| Hedging_Primary_AsyncWork   | 5,504.3 ns | 313.47 ns | 449.58 ns |  8.06 |    0.77 | 0.1831 |    2338 B |          NA |
| Hedging_Secondary_AsyncWork | 6,123.1 ns |  98.34 ns | 144.15 ns |  8.97 |    0.50 | 0.2060 |    2577 B |          NA |
