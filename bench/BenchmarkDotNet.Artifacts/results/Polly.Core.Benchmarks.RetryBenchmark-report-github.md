```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method          | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRetry_V7 |  86.09 ns | 0.702 ns | 1.050 ns |  1.00 |    0.02 | 0.0408 |     512 B |        1.00 |
| ExecuteRetry_V8 | 109.46 ns | 0.152 ns | 0.213 ns |  1.27 |    0.02 |      - |         - |        0.00 |
