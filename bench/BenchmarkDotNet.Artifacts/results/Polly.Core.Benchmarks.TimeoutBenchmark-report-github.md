```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method            | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 222.8 ns | 1.58 ns | 2.26 ns |  1.00 | 0.0579 |     728 B |        1.00 |
| ExecuteTimeout_V8 | 191.3 ns | 0.37 ns | 0.49 ns |  0.86 |      - |         - |        0.00 |
