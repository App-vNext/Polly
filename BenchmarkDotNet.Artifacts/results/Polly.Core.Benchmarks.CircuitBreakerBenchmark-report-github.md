``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                   Method |     Mean |   Error |  StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| ExecuteCircuitBreaker_V7 | 309.0 ns | 3.53 ns | 5.18 ns |  1.00 | 0.0200 |     504 B |        1.00 |
| ExecuteCircuitBreaker_V8 | 369.5 ns | 3.27 ns | 4.68 ns |  1.19 | 0.0010 |      32 B |        0.06 |
