```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                 | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| NoOpAsync              |  43.26 ns | 1.954 ns | 2.925 ns |  1.00 |    0.09 | 0.0242 |     304 B |        1.00 |
| NullResiliencePipeline | 163.88 ns | 1.275 ns | 1.869 ns |  3.80 |    0.24 | 0.0298 |     376 B |        1.24 |
