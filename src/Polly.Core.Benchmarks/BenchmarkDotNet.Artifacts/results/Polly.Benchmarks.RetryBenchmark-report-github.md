``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|          Method |     Mean |   Error |   StdDev |   Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|---------------- |---------:|--------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteRetry_V7 | 233.3 ns | 7.32 ns | 10.73 ns | 229.7 ns |  1.00 |    0.00 | 0.0219 |     552 B |        1.00 |
| ExecuteRetry_V8 | 206.1 ns | 1.22 ns |  1.79 ns | 207.3 ns |  0.88 |    0.04 |      - |         - |        0.00 |
