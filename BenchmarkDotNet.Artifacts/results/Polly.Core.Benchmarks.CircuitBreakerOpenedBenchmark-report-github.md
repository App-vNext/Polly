``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                    Method |         Mean |     Error |    StdDev |  Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|-------------------------- |-------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 125,329.4 ns | 167.90 ns | 251.31 ns | 225.33 |    0.72 |      - |    2888 B |       15.04 |
| ExecuteAsync_Exception_V8 |  86,747.0 ns | 293.62 ns | 411.61 ns | 155.96 |    1.02 |      - |    1816 B |        9.46 |
|   ExecuteAsync_Outcome_V8 |     556.3 ns |   0.85 ns |   1.22 ns |   1.00 |    0.00 | 0.0076 |     192 B |        1.00 |
