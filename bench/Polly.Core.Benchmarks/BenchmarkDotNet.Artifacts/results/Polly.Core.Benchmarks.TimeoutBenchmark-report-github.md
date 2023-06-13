``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|            Method |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------ |---------:|---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteTimeout_V7 | 416.3 ns | 38.64 ns | 56.64 ns | 386.4 ns |  1.00 |    0.00 | 0.0868 |     728 B |        1.00 |
| ExecuteTimeout_V8 | 345.2 ns | 19.16 ns | 26.86 ns | 345.6 ns |  0.84 |    0.12 |      - |         - |        0.00 |
