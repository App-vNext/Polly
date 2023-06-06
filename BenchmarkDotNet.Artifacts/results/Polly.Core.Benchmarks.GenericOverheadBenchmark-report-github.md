``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                  Method |     Mean |    Error |   StdDev | Ratio | Allocated | Alloc Ratio |
|------------------------ |---------:|---------:|---------:|------:|----------:|------------:|
|    ExecuteAsync_Generic | 29.93 ns | 0.050 ns | 0.070 ns |  1.00 |         - |          NA |
| ExecuteAsync_NonGeneric | 32.20 ns | 0.286 ns | 0.410 ns |  1.08 |         - |          NA |
