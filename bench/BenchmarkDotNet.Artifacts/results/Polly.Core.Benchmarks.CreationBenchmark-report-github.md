```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method      | Mean        | Error    | StdDev    | Gen0   | Allocated |
|------------ |------------:|---------:|----------:|-------:|----------:|
| Fallback_V7 |    45.45 ns | 2.165 ns |  3.241 ns | 0.0306 |     384 B |
| Fallback_V8 | 1,705.23 ns | 8.115 ns | 12.146 ns | 0.4025 |    5064 B |
