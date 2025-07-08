```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                    | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 10,428.8 ns | 25.21 ns | 36.95 ns | 22.36 |    0.48 | 0.1526 |    2056 B |       10.28 |
| ExecuteAsync_Exception_V8 |  6,823.1 ns | 16.33 ns | 22.89 ns | 14.63 |    0.32 | 0.0992 |    1312 B |        6.56 |
| ExecuteAsync_Outcome_V8   |    466.7 ns |  6.57 ns |  9.83 ns |  1.00 |    0.03 | 0.0157 |     200 B |        1.00 |
