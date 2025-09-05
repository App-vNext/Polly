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
| NoOpAsync              |  32.02 ns | 0.309 ns | 0.452 ns |  1.00 |    0.02 | 0.0242 |     304 B |        1.00 |
| NullResiliencePipeline | 128.75 ns | 1.035 ns | 1.550 ns |  4.02 |    0.07 | 0.0298 |     376 B |        1.24 |
