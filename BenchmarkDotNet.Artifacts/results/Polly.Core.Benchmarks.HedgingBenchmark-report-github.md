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
|             Hedging_Primary | 1.209 μs | 0.0074 μs | 0.0110 μs |  1.00 |    0.00 | 0.0019 |      - |      80 B |        1.00 |
|           Hedging_Secondary | 2.152 μs | 0.0666 μs | 0.0933 μs |  1.78 |    0.08 | 0.0076 |      - |     280 B |        3.50 |
|   Hedging_Primary_AsyncWork | 5.962 μs | 0.5393 μs | 0.7560 μs |  4.93 |    0.64 | 0.0534 | 0.0229 |    1443 B |       18.04 |
| Hedging_Secondary_AsyncWork | 9.242 μs | 1.0615 μs | 1.5889 μs |  7.64 |    1.32 | 0.0610 | 0.0458 |    1815 B |       22.69 |
