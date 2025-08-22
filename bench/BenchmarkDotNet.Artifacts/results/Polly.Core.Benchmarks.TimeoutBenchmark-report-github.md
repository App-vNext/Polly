```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method            | Mean     | Error   | StdDev  | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------:|--------:|--------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 185.4 ns | 3.07 ns | 4.60 ns | 183.0 ns |  1.00 |    0.03 | 0.0579 |     728 B |        1.00 |
| ExecuteTimeout_V8 | 169.5 ns | 0.95 ns | 1.42 ns | 169.8 ns |  0.91 |    0.02 |      - |         - |        0.00 |
