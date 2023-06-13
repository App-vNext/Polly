``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                      Method |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|             Hedging_Primary | 1.075 μs | 0.0162 μs | 0.0227 μs |  1.00 |    0.00 | 0.0038 |      40 B |        1.00 |
|           Hedging_Secondary | 1.859 μs | 0.0288 μs | 0.0431 μs |  1.73 |    0.04 | 0.0267 |     224 B |        5.60 |
|   Hedging_Primary_AsyncWork | 3.277 μs | 0.0630 μs | 0.0819 μs |  3.05 |    0.10 | 0.1640 |    1373 B |       34.33 |
| Hedging_Secondary_AsyncWork | 4.103 μs | 0.0948 μs | 0.1419 μs |  3.80 |    0.15 | 0.1984 |    1669 B |       41.73 |
