``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                Method |     Mean |   Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------- |---------:|--------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRateLimiter_V7 | 241.2 ns | 9.40 ns | 14.07 ns |  1.00 |    0.00 | 0.0148 |     376 B |        1.00 |
| ExecuteRateLimiter_V8 | 267.5 ns | 0.69 ns |  0.96 ns |  1.11 |    0.07 | 0.0014 |      40 B |        0.11 |
