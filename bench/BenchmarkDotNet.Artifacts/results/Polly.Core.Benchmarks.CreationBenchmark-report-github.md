```

BenchmarkDotNet v0.13.6, Windows 11 (10.0.22621.1992/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.306
  [Host] : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|      Method |       Mean |    Error |   StdDev |   Gen0 | Allocated |
|------------ |-----------:|---------:|---------:|-------:|----------:|
| Fallback_V7 |   114.8 ns |  1.84 ns |  2.70 ns | 0.0191 |     480 B |
| Fallback_V8 | 4,324.7 ns | 21.92 ns | 31.43 ns | 0.2518 |    6504 B |
