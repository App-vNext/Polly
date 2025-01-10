```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                  | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| ExecuteAsync_Generic    | 14.67 ns | 0.266 ns | 0.390 ns |  1.00 |    0.04 |         - |          NA |
| ExecuteAsync_NonGeneric | 24.89 ns | 0.581 ns | 0.870 ns |  1.70 |    0.07 |         - |          NA |
