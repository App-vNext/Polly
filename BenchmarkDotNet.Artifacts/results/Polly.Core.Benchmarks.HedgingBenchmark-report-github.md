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
|             Hedging_Primary | 1.087 μs | 0.0101 μs | 0.0141 μs |  1.00 |    0.00 | 0.0019 |      - |      80 B |        1.00 |
|           Hedging_Secondary | 1.910 μs | 0.0180 μs | 0.0269 μs |  1.76 |    0.04 | 0.0095 |      - |     280 B |        3.50 |
|   Hedging_Primary_AsyncWork | 5.156 μs | 0.1238 μs | 0.1814 μs |  4.74 |    0.15 | 0.0534 | 0.0229 |    1435 B |       17.94 |
| Hedging_Secondary_AsyncWork | 7.947 μs | 0.2024 μs | 0.2966 μs |  7.33 |    0.34 | 0.0763 | 0.0381 |    1951 B |       24.39 |
