```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                  | Mean     | Error    | StdDev   | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |---------:|---------:|---------:|----------:|------:|--------:|----------:|------------:|
| ExecuteAsync_Generic    | 12.53 ns | 2.430 ns | 3.244 ns |  9.500 ns |  1.07 |    0.39 |         - |          NA |
| ExecuteAsync_NonGeneric | 15.28 ns | 0.272 ns | 0.407 ns | 15.133 ns |  1.30 |    0.33 |         - |          NA |
