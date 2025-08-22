```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                | Mean     | Error   | StdDev  | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |---------:|--------:|--------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRateLimiter_V7 | 122.9 ns | 1.06 ns | 1.59 ns | 123.3 ns |  1.00 |    0.02 | 0.0298 |     376 B |        1.00 |
| ExecuteRateLimiter_V8 | 128.4 ns | 1.84 ns | 2.70 ns | 130.6 ns |  1.04 |    0.03 | 0.0031 |      40 B |        0.11 |
