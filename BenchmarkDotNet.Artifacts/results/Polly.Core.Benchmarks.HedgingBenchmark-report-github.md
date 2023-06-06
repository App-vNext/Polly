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
|             Hedging_Primary |   941.7 ns |   5.45 ns |   7.81 ns |  1.00 |    0.00 |      - |      - |      40 B |        1.00 |
|           Hedging_Secondary | 1,679.7 ns |   7.23 ns |  10.83 ns |  1.78 |    0.02 | 0.0076 |      - |     200 B |        5.00 |
|   Hedging_Primary_AsyncWork | 4,867.5 ns |  71.40 ns | 102.40 ns |  5.17 |    0.12 | 0.0534 | 0.0229 |    1407 B |       35.17 |
| Hedging_Secondary_AsyncWork | 7,093.2 ns | 234.32 ns | 343.47 ns |  7.51 |    0.37 | 0.0687 | 0.0610 |    1737 B |       43.42 |
