```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                   Method |     Mean |   Error |  StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 338.5 ns | 1.52 ns | 2.22 ns |  1.00 | 0.0200 |     504 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 557.9 ns | 1.74 ns | 2.49 ns |  1.65 |      - |         - |        0.00 |
