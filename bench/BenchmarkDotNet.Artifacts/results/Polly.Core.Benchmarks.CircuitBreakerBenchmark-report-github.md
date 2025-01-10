```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 207.1 ns | 3.68 ns | 5.50 ns |  1.00 |    0.04 | 0.0370 |     464 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 335.3 ns | 6.30 ns | 9.43 ns |  1.62 |    0.06 |      - |         - |        0.00 |
