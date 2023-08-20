```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                 Method |     Mean |    Error |   StdDev | Allocated |
|----------------------- |---------:|---------:|---------:|----------:|
|         GetPipeline_Ok | 19.03 ns | 0.169 ns | 0.248 ns |         - |
| GetPipeline_Generic_Ok | 51.66 ns | 1.960 ns | 2.811 ns |         - |
