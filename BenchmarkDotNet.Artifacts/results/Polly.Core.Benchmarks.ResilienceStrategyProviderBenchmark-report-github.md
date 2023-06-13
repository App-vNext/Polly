``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|         Method |     Mean |    Error |   StdDev |   Median | Allocated |
|--------------- |---------:|---------:|---------:|---------:|----------:|
|         Get_Ok | 18.98 ns | 0.148 ns | 0.212 ns | 18.98 ns |         - |
| Get_Generic_Ok | 48.27 ns | 0.825 ns | 1.209 ns | 47.25 ns |         - |
