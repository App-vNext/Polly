```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                              | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| DelegatingComponent_ExecuteCore_Jit | 20.63 ns | 0.036 ns | 0.053 ns |  1.00 |      - |         - |          NA |
| DelegatingComponent_ExecuteCore_Aot | 30.36 ns | 0.063 ns | 0.091 ns |  1.47 | 0.0019 |      24 B |          NA |
