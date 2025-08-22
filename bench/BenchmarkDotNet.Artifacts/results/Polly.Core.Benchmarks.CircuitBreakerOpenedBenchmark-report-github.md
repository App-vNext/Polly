```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 8,496.4 ns | 148.94 ns | 208.79 ns | 24.75 |    2.17 | 0.1526 |    2056 B |       10.28 |
| ExecuteAsync_Exception_V8 | 5,645.8 ns |  52.64 ns |  73.79 ns | 16.45 |    1.40 | 0.0992 |    1312 B |        6.56 |
| ExecuteAsync_Outcome_V8   |   345.8 ns |  20.24 ns |  30.30 ns |  1.01 |    0.12 | 0.0157 |     200 B |        1.00 |
