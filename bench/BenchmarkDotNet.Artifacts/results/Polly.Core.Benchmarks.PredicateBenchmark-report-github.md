```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                     | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| Predicate_SwitchExpression |  7.809 ns | 0.2146 ns | 0.3145 ns |  7.590 ns |  1.00 |    0.06 |         - |          NA |
| Predicate_PredicateBuilder | 14.890 ns | 0.1196 ns | 0.1754 ns | 14.798 ns |  1.91 |    0.08 |         - |          NA |
