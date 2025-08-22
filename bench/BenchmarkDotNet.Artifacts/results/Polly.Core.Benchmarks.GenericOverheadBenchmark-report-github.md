```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                  | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| ExecuteAsync_Generic    |  8.720 ns | 0.1965 ns | 0.2755 ns |  8.934 ns |  1.00 |    0.04 |         - |          NA |
| ExecuteAsync_NonGeneric | 13.574 ns | 0.1276 ns | 0.1870 ns | 13.718 ns |  1.56 |    0.05 |         - |          NA |
