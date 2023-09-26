```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                 Method |     Mean |    Error |   StdDev | Allocated |
|----------------------- |---------:|---------:|---------:|----------:|
|         GetPipeline_Ok | 19.46 ns | 0.021 ns | 0.031 ns |         - |
| GetPipeline_Generic_Ok | 47.83 ns | 0.058 ns | 0.086 ns |         - |
