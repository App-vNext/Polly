```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                 | Mean      | Error     | StdDev    | Allocated |
|----------------------- |----------:|----------:|----------:|----------:|
| GetPipeline_Ok         |  9.136 ns | 0.0515 ns | 0.0771 ns |         - |
| GetPipeline_Generic_Ok | 33.004 ns | 0.1981 ns | 0.2966 ns |         - |
