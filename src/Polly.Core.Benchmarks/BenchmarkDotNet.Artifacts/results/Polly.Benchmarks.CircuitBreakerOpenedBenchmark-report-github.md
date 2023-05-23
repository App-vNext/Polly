``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                    Method |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|-------------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 29,960.1 ns | 317.77 ns | 455.74 ns | 53.76 |    1.10 | 0.0916 |    2888 B |       12.89 |
| ExecuteAsync_Exception_V8 | 19,520.4 ns | 181.59 ns | 254.57 ns | 35.02 |    0.60 | 0.0610 |    1656 B |        7.39 |
|   ExecuteAsync_Outcome_V8 |    557.3 ns |   3.71 ns |   5.43 ns |  1.00 |    0.00 | 0.0086 |     224 B |        1.00 |
