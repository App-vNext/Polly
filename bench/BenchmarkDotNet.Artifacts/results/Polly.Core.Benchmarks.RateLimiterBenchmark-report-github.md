```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRateLimiter_V7 | 243.5 ns | 3.71 ns | 5.44 ns |  1.00 |    0.00 | 0.0148 |     376 B |        1.00 |
| ExecuteRateLimiter_V8 | 292.8 ns | 3.76 ns | 5.51 ns |  1.20 |    0.05 | 0.0014 |      40 B |        0.11 |
