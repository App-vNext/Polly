``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|         Method |     Mean |    Error |   StdDev | Allocated |
|--------------- |---------:|---------:|---------:|----------:|
|         Get_Ok | 19.24 ns | 0.038 ns | 0.056 ns |         - |
| Get_Generic_Ok | 48.37 ns | 0.170 ns | 0.249 ns |         - |
