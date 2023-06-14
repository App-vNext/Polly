``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                    Method |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|-------------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 33,729.3 ns | 437.93 ns | 641.92 ns | 58.35 |    1.74 | 0.3052 |    2888 B |       15.04 |
| ExecuteAsync_Exception_V8 | 24,111.1 ns | 528.78 ns | 741.28 ns | 41.73 |    1.86 | 0.2136 |    1816 B |        9.46 |
|   ExecuteAsync_Outcome_V8 |    578.1 ns |   8.20 ns |  11.76 ns |  1.00 |    0.00 | 0.0229 |     192 B |        1.00 |
