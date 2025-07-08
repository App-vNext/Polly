```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                   | Mean     | Error   | StdDev  | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|--------:|--------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 145.0 ns | 4.56 ns | 6.83 ns | 141.5 ns |  1.00 |    0.06 | 0.0370 |     464 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 230.9 ns | 0.20 ns | 0.30 ns | 230.9 ns |  1.60 |    0.07 |      - |         - |        0.00 |
