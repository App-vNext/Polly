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
| ExecuteAsync_Exception_V7 | 30,264.7 ns | 282.61 ns | 386.83 ns | 53.54 |    0.93 | 0.0916 |    2888 B |       12.89 |
| ExecuteAsync_Exception_V8 | 20,805.9 ns | 123.75 ns | 169.39 ns | 36.81 |    0.76 | 0.0610 |    1848 B |        8.25 |
|   ExecuteAsync_Outcome_V8 |    565.4 ns |   7.41 ns |  10.15 ns |  1.00 |    0.00 | 0.0086 |     224 B |        1.00 |
