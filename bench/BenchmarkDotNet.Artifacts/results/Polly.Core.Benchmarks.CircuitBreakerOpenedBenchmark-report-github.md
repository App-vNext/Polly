```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                    | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 10,446.3 ns | 39.89 ns | 59.71 ns | 22.30 |    0.82 | 0.1526 |    2056 B |       10.28 |
| ExecuteAsync_Exception_V8 |  6,791.6 ns | 42.75 ns | 61.31 ns | 14.50 |    0.54 | 0.0992 |    1312 B |        6.56 |
| ExecuteAsync_Outcome_V8   |    469.0 ns | 10.24 ns | 15.33 ns |  1.00 |    0.05 | 0.0157 |     200 B |        1.00 |
