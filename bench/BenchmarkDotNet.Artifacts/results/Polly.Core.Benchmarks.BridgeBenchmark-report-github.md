```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                 | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| NoOpAsync              |  36.72 ns | 4.431 ns | 6.212 ns |  1.02 |    0.22 | 0.0242 |     304 B |        1.00 |
| NullResiliencePipeline | 129.25 ns | 2.554 ns | 3.822 ns |  3.60 |    0.51 | 0.0298 |     376 B |        1.24 |
