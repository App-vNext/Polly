```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                     | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| Predicate_SwitchExpression |  9.503 ns | 0.1032 ns | 0.1544 ns |  1.00 |    0.02 |         - |          NA |
| Predicate_PredicateBuilder | 18.068 ns | 0.0956 ns | 0.1401 ns |  1.90 |    0.03 |         - |          NA |
