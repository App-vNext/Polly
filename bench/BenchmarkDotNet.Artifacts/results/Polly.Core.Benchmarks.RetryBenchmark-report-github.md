```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method          | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRetry_V7 | 125.4 ns | 0.84 ns | 1.20 ns |  1.00 |    0.01 | 0.0408 |     512 B |        1.00 |
| ExecuteRetry_V8 | 179.7 ns | 0.61 ns | 0.87 ns |  1.43 |    0.02 |      - |         - |        0.00 |
