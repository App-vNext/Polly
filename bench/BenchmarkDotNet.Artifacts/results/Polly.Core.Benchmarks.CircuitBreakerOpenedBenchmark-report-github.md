```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                    | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 14,295.0 ns | 418.44 ns | 600.11 ns | 27.37 |    1.33 | 0.1526 |    2056 B |       10.28 |
| ExecuteAsync_Exception_V8 |  9,178.8 ns | 114.83 ns | 171.88 ns | 17.57 |    0.55 | 0.0916 |    1312 B |        6.56 |
| ExecuteAsync_Outcome_V8   |    522.6 ns |   9.15 ns |  13.70 ns |  1.00 |    0.04 | 0.0153 |     200 B |        1.00 |
