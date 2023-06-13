``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                      Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|             Hedging_Primary |  1.045 μs | 0.0049 μs | 0.0074 μs |  1.00 |    0.00 |      - |      40 B |        1.00 |
|           Hedging_Secondary |  1.836 μs | 0.0043 μs | 0.0063 μs |  1.76 |    0.01 | 0.0076 |     224 B |        5.60 |
|   Hedging_Primary_AsyncWork | 15.291 μs | 1.7958 μs | 2.6878 μs | 14.63 |    2.55 | 0.1678 |    4327 B |      108.17 |
| Hedging_Secondary_AsyncWork | 24.883 μs | 2.1234 μs | 3.1782 μs | 23.82 |    3.09 | 0.1526 |    4553 B |      113.83 |
