```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22621.3007/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.101
  [Host] : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                   | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 262.4 ns | 3.35 ns | 4.91 ns |  1.00 |    0.00 | 0.0601 |     504 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 407.2 ns | 4.49 ns | 6.45 ns |  1.55 |    0.04 |      - |         - |        0.00 |
