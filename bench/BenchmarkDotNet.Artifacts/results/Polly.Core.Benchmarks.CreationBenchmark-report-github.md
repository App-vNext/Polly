```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method      | Mean        | Error     | StdDev    | Gen0   | Allocated |
|------------ |------------:|----------:|----------:|-------:|----------:|
| Fallback_V7 |    44.26 ns |  0.524 ns |  0.784 ns | 0.0306 |     384 B |
| Fallback_V8 | 1,727.58 ns | 32.536 ns | 47.691 ns | 0.4025 |    5064 B |
