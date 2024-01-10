```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                     | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| Predicate_SwitchExpression |  9.628 ns | 0.0755 ns | 0.1082 ns |  1.00 |    0.00 |         - |          NA |
| Predicate_PredicateBuilder | 27.885 ns | 0.1972 ns | 0.2890 ns |  2.90 |    0.04 |         - |          NA |
