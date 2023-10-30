```

BenchmarkDotNet v0.13.9+228a464e8be6c580ad9408e98f18813f6407fb5a, Windows 11 (10.0.22621.2428/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.403
  [Host] : .NET 7.0.13 (7.0.1323.51816), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Hedging_Primary             |  1.284 μs | 0.0047 μs | 0.0066 μs |  1.00 |    0.00 |      - |      40 B |        1.00 |
| Hedging_Secondary           |  2.007 μs | 0.0043 μs | 0.0064 μs |  1.56 |    0.01 | 0.0038 |     184 B |        4.60 |
| Hedging_Primary_AsyncWork   | 33.379 μs | 1.9491 μs | 2.9173 μs | 26.43 |    1.90 | 0.6104 |   15299 B |      382.48 |
| Hedging_Secondary_AsyncWork | 33.735 μs | 0.2915 μs | 0.4273 μs | 26.28 |    0.43 | 0.6104 |   15431 B |      385.77 |
