```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                 | Mean      | Error     | StdDev    | Allocated |
|----------------------- |----------:|----------:|----------:|----------:|
| GetPipeline_Ok         |  9.383 ns | 0.1542 ns | 0.2308 ns |         - |
| GetPipeline_Generic_Ok | 31.460 ns | 0.1564 ns | 0.2293 ns |         - |
