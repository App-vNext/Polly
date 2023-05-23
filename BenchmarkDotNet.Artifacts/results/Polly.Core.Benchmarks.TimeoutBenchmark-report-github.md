``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|            Method |     Mean |   Error |  StdDev |   Median | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------ |---------:|--------:|--------:|---------:|------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 373.0 ns | 2.82 ns | 4.21 ns | 373.1 ns |  1.00 | 0.0286 |     728 B |        1.00 |
| ExecuteTimeout_V8 | 264.2 ns | 3.77 ns | 5.28 ns | 267.9 ns |  0.71 | 0.0010 |      32 B |        0.04 |
