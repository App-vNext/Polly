```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4351)
13th Gen Intel Core i7-13700H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.301
  [Host] : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method          | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| ExecuteRetry_V7 |  99.64 ns | 0.373 ns | 0.510 ns |  1.00 | 0.0408 |     512 B |        1.00 |
| ExecuteRetry_V8 | 149.93 ns | 0.600 ns | 0.821 ns |  1.50 |      - |         - |        0.00 |
