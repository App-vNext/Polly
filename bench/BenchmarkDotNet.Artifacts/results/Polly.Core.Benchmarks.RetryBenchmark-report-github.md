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
| ExecuteRetry_V7 |  85.19 ns | 1.539 ns | 2.256 ns |  1.00 |    0.04 | 0.0408 |     512 B |        1.00 |
| ExecuteRetry_V8 | 130.07 ns | 0.583 ns | 0.872 ns |  1.53 |    0.04 |      - |         - |        0.00 |
