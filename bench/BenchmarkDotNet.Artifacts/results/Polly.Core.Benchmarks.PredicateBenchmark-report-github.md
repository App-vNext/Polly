```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                     | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| Predicate_SwitchExpression |  7.625 ns | 0.0609 ns | 0.0911 ns |  1.00 |    0.02 |         - |          NA |
| Predicate_PredicateBuilder | 14.278 ns | 0.2200 ns | 0.3084 ns |  1.87 |    0.05 |         - |          NA |
