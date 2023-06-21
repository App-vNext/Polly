``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.304
  [Host] : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                     Method |     Mean |    Error |   StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| Predicate_SwitchExpression | 17.17 ns | 0.028 ns | 0.041 ns |  1.00 |    0.00 |         - |          NA |
| Predicate_PredicateBuilder | 29.64 ns | 0.859 ns | 1.232 ns |  1.73 |    0.07 |         - |          NA |
