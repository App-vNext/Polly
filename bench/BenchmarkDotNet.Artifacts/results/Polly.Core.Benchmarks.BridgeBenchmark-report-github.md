```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
12th Gen Intel Core i7-1280P, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                 | Mean      | Error    | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------:|---------:|----------:|------:|--------:|-------:|----------:|------------:|
| NoOpAsync              |  84.05 ns | 4.660 ns |  6.830 ns |  1.01 |    0.11 | 0.0242 |     304 B |        1.00 |
| NullResiliencePipeline | 241.69 ns | 8.178 ns | 12.240 ns |  2.89 |    0.27 | 0.0296 |     376 B |        1.24 |
