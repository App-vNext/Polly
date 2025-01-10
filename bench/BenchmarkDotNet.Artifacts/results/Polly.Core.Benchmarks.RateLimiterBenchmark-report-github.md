```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| ExecuteRateLimiter_V7 | 178.3 ns | 0.79 ns | 1.16 ns |  1.00 | 0.0298 |     376 B |        1.00 |
| ExecuteRateLimiter_V8 | 188.2 ns | 0.89 ns | 1.34 ns |  1.06 | 0.0031 |      40 B |        0.11 |
