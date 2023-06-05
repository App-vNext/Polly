``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2), VM=Hyper-V
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.302
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|         Method |     Mean |    Error |   StdDev |   Gen0 | Allocated |
|--------------- |---------:|---------:|---------:|-------:|----------:|
|         Get_Ok | 25.41 ns | 0.186 ns | 0.273 ns | 0.0016 |      40 B |
| Get_Generic_Ok | 53.83 ns | 0.672 ns | 1.005 ns | 0.0013 |      32 B |
