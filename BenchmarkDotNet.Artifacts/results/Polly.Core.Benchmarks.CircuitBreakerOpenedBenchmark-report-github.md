``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                    Method |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|-------------------------- |------------:|----------:|----------:|------------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 29,720.5 ns | 177.63 ns | 265.86 ns | 29,629.0 ns | 56.63 |    0.58 | 0.0610 |    2888 B |       15.04 |
| ExecuteAsync_Exception_V8 | 20,840.2 ns | 191.44 ns | 274.56 ns | 20,696.3 ns | 39.71 |    0.51 | 0.0610 |    1816 B |        9.46 |
|   ExecuteAsync_Outcome_V8 |    525.1 ns |   1.07 ns |   1.47 ns |    525.0 ns |  1.00 |    0.00 | 0.0076 |     192 B |        1.00 |
