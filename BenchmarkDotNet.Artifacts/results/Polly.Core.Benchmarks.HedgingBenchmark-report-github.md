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
|             Hedging_Primary | 1.057 μs | 0.0027 μs | 0.0040 μs |  1.00 |    0.00 | 0.0019 |      - |      80 B |        1.00 |
|           Hedging_Secondary | 1.773 μs | 0.0130 μs | 0.0194 μs |  1.68 |    0.02 | 0.0095 |      - |     280 B |        3.50 |
|   Hedging_Primary_AsyncWork | 5.112 μs | 0.1458 μs | 0.2138 μs |  4.84 |    0.21 | 0.0534 | 0.0229 |    1506 B |       18.82 |
| Hedging_Secondary_AsyncWork | 7.910 μs | 0.1827 μs | 0.2501 μs |  7.48 |    0.25 | 0.0763 | 0.0381 |    1999 B |       24.99 |
