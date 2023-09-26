```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host] : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                    Method |        Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|-------------------------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 29,867.4 ns | 35.32 ns | 50.66 ns | 41.19 |    0.64 | 0.0916 |    2888 B |       15.04 |
| ExecuteAsync_Exception_V8 | 20,525.5 ns | 21.19 ns | 31.71 ns | 28.32 |    0.41 | 0.0610 |    1816 B |        9.46 |
|   ExecuteAsync_Outcome_V8 |    724.8 ns |  7.24 ns | 10.84 ns |  1.00 |    0.00 | 0.0076 |     192 B |        1.00 |
