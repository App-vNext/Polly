``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                      Method |       Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
|---------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
|             Hedging_Primary |   958.3 ns |   4.36 ns |   6.52 ns |  1.00 |    0.00 | 0.0019 |      - |      80 B |        1.00 |
|           Hedging_Secondary | 1,817.0 ns |  11.59 ns |  17.35 ns |  1.90 |    0.01 | 0.0095 |      - |     280 B |        3.50 |
|   Hedging_Primary_AsyncWork | 5,000.9 ns | 164.21 ns | 235.50 ns |  5.21 |    0.23 | 0.0458 | 0.0229 |    1244 B |       15.55 |
| Hedging_Secondary_AsyncWork | 7,898.5 ns | 181.13 ns | 271.10 ns |  8.24 |    0.26 | 0.0687 | 0.0610 |    1755 B |       21.94 |
