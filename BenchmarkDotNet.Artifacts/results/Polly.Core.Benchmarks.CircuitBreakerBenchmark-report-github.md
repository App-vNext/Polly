``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                   Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 285.3 ns | 2.77 ns | 3.98 ns |  1.00 |    0.00 | 0.0200 |     504 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 357.0 ns | 1.26 ns | 1.89 ns |  1.25 |    0.02 | 0.0010 |      32 B |        0.06 |
