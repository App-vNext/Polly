```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                     | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |---------:|---------:|---------:|---------:|------:|--------:|----------:|------------:|
| Predicate_SwitchExpression | 11.82 ns | 0.234 ns | 0.343 ns | 12.08 ns |  1.00 |    0.04 |         - |          NA |
| Predicate_PredicateBuilder | 21.52 ns | 0.077 ns | 0.108 ns | 21.51 ns |  1.82 |    0.05 |         - |          NA |
