```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                              | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| DelegatingComponent_ExecuteCore_Jit | 16.79 ns | 0.270 ns | 0.405 ns |  1.00 |    0.03 |      - |         - |          NA |
| DelegatingComponent_ExecuteCore_Aot | 25.07 ns | 0.365 ns | 0.546 ns |  1.49 |    0.05 | 0.0019 |      24 B |          NA |
