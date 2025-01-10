```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                              | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| DelegatingComponent_ExecuteCore_Jit | 29.17 ns | 0.613 ns | 0.899 ns |  1.00 |    0.04 |      - |         - |          NA |
| DelegatingComponent_ExecuteCore_Aot | 43.48 ns | 0.584 ns | 0.873 ns |  1.49 |    0.05 | 0.0019 |      24 B |          NA |
