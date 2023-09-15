```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                     Method |     Mean |    Error |   StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| Predicate_SwitchExpression | 16.69 ns | 0.035 ns | 0.051 ns |  1.00 |    0.00 |         - |          NA |
| Predicate_PredicateBuilder | 34.01 ns | 0.297 ns | 0.445 ns |  2.04 |    0.02 |         - |          NA |
