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
| ExecuteAsync_Exception_V7 | 30,077.7 ns | 130.12 ns | 190.73 ns | 51.40 |    0.53 | 0.0610 |    2888 B |       12.89 |
| ExecuteAsync_Exception_V8 | 20,036.3 ns |  52.50 ns |  75.30 ns | 34.24 |    0.29 | 0.0610 |    1848 B |        8.25 |
|   ExecuteAsync_Outcome_V8 |    585.3 ns |   3.50 ns |   4.91 ns |  1.00 |    0.00 | 0.0086 |     224 B |        1.00 |
