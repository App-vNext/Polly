``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                   Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 299.9 ns | 11.26 ns | 16.86 ns |  1.00 |    0.00 | 0.0601 |     504 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 436.2 ns |  6.25 ns |  9.36 ns |  1.46 |    0.07 |      - |         - |        0.00 |
