```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRateLimiter_V7 | 161.8 ns | 2.92 ns | 4.38 ns |  1.00 |    0.04 | 0.0298 |     376 B |        1.00 |
| ExecuteRateLimiter_V8 | 159.4 ns | 0.47 ns | 0.69 ns |  0.99 |    0.03 | 0.0031 |      40 B |        0.11 |
