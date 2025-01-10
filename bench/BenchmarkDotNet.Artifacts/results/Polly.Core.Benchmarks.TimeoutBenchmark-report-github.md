```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method            | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 259.3 ns | 4.25 ns | 6.24 ns |  1.00 |    0.03 | 0.0577 |     728 B |        1.00 |
| ExecuteTimeout_V8 | 241.1 ns | 1.48 ns | 2.17 ns |  0.93 |    0.02 |      - |         - |        0.00 |
