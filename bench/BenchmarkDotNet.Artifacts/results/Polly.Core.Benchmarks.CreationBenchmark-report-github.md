```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|      Method |       Mean |    Error |   StdDev |   Gen0 | Allocated |
|------------ |-----------:|---------:|---------:|-------:|----------:|
| Fallback_V7 |   127.9 ns |  5.55 ns |  7.96 ns | 0.0191 |     480 B |
| Fallback_V8 | 3,633.8 ns | 60.79 ns | 89.11 ns | 0.2136 |    5432 B |
