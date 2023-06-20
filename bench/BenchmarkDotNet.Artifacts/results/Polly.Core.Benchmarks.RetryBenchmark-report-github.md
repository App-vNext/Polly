``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.304
  [Host] : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|          Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRetry_V7 | 210.9 ns | 4.37 ns | 6.27 ns |  1.00 |    0.00 | 0.0658 |     552 B |        1.00 |
| ExecuteRetry_V8 | 224.1 ns | 2.34 ns | 3.43 ns |  1.06 |    0.04 |      - |         - |        0.00 |
