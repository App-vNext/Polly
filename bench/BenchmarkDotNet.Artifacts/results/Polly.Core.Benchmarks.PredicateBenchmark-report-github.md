```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                     | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| Predicate_SwitchExpression |  9.687 ns | 0.1908 ns | 0.2797 ns |  9.940 ns |  1.00 |    0.04 |         - |          NA |
| Predicate_PredicateBuilder | 17.365 ns | 0.0545 ns | 0.0816 ns | 17.369 ns |  1.79 |    0.05 |         - |          NA |
