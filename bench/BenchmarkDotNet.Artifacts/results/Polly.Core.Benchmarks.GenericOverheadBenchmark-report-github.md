```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                  | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| ExecuteAsync_Generic    |  9.788 ns | 0.1653 ns | 0.2317 ns |  1.00 |    0.03 |         - |          NA |
| ExecuteAsync_NonGeneric | 14.824 ns | 0.0915 ns | 0.1370 ns |  1.52 |    0.04 |         - |          NA |
