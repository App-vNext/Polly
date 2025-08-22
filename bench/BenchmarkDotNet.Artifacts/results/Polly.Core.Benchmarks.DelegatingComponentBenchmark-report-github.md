```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                              | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |---------:|---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| DelegatingComponent_ExecuteCore_Jit | 17.73 ns | 0.372 ns | 0.557 ns | 17.84 ns |  1.00 |    0.04 |      - |         - |          NA |
| DelegatingComponent_ExecuteCore_Aot | 28.07 ns | 0.221 ns | 0.323 ns | 28.25 ns |  1.58 |    0.05 | 0.0019 |      24 B |          NA |
