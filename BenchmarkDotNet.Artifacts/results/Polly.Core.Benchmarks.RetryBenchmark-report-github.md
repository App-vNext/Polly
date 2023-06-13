``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|          Method |     Mean |   Error |  StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|---------------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| ExecuteRetry_V7 | 262.6 ns | 2.47 ns | 3.62 ns |  1.00 | 0.0219 |     552 B |        1.00 |
| ExecuteRetry_V8 | 202.0 ns | 1.06 ns | 1.55 ns |  0.77 |      - |         - |        0.00 |
