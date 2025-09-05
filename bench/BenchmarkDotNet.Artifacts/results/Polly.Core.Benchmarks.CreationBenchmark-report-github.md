```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26100.4946/24H2/2024Update/HudsonValley)
13th Gen Intel Core i7-13700H 2.90GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.304
  [Host] : .NET 9.0.8 (9.0.825.36511), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method      | Mean        | Error     | StdDev    | Gen0   | Allocated |
|------------ |------------:|----------:|----------:|-------:|----------:|
| Fallback_V7 |    34.11 ns |  0.496 ns |  0.727 ns | 0.0306 |     384 B |
| Fallback_V8 | 1,350.30 ns | 15.951 ns | 23.875 ns | 0.4025 |    5064 B |
