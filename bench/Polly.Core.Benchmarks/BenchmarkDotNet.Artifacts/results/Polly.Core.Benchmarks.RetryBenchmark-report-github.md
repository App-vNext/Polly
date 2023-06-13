``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|          Method |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------- |---------:|---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRetry_V7 | 264.2 ns | 10.80 ns | 15.83 ns | 255.3 ns |  1.00 |    0.00 | 0.0658 |     552 B |        1.00 |
| ExecuteRetry_V8 | 244.7 ns |  7.75 ns | 10.61 ns | 244.1 ns |  0.93 |    0.05 |      - |         - |        0.00 |
