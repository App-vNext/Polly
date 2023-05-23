``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                      Method |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|---------------------------- |-----------:|----------:|----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
|             Hedging_Primary |   990.8 ns |   4.23 ns |   6.20 ns |   990.7 ns |  1.00 |    0.00 | 0.0019 |      - |      80 B |        1.00 |
|           Hedging_Secondary | 1,754.9 ns |  15.31 ns |  21.46 ns | 1,742.7 ns |  1.77 |    0.02 | 0.0095 |      - |     280 B |        3.50 |
|   Hedging_Primary_AsyncWork | 4,966.6 ns | 245.75 ns | 344.50 ns | 4,826.0 ns |  5.01 |    0.33 | 0.0458 | 0.0229 |    1328 B |       16.60 |
| Hedging_Secondary_AsyncWork | 7,596.7 ns | 251.90 ns | 377.03 ns | 7,457.9 ns |  7.68 |    0.40 | 0.0610 | 0.0534 |    1693 B |       21.16 |
