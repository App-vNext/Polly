```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|            Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------ |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 437.9 ns | 5.98 ns | 8.58 ns |  1.00 |    0.00 | 0.0286 |     728 B |        1.00 |
| ExecuteTimeout_V8 | 349.0 ns | 3.97 ns | 5.56 ns |  0.80 |    0.02 |      - |         - |        0.00 |
