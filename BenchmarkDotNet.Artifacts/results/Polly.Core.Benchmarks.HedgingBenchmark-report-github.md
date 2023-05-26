``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                      Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|---------------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
|             Hedging_Primary | 1.049 μs | 0.0074 μs | 0.0108 μs |  1.00 |    0.00 |      - |      - |      40 B |        1.00 |
|           Hedging_Secondary | 1.788 μs | 0.0093 μs | 0.0139 μs |  1.71 |    0.02 | 0.0076 |      - |     200 B |        5.00 |
|   Hedging_Primary_AsyncWork | 5.306 μs | 0.1484 μs | 0.2222 μs |  5.07 |    0.24 | 0.0534 | 0.0229 |    1444 B |       36.10 |
| Hedging_Secondary_AsyncWork | 7.518 μs | 0.0869 μs | 0.1246 μs |  7.17 |    0.12 | 0.0610 | 0.0534 |    1684 B |       42.10 |
