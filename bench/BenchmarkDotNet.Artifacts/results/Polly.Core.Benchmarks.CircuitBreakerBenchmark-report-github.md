```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 111.6 ns | 2.26 ns | 3.38 ns |  1.00 |    0.04 | 0.0370 |     464 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 190.2 ns | 1.73 ns | 2.53 ns |  1.71 |    0.06 |      - |         - |        0.00 |
