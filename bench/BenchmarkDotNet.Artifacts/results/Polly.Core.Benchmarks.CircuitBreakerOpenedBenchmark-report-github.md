```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                    Method |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|-------------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 30,519.7 ns | 621.36 ns | 891.14 ns | 38.69 |    1.25 | 0.0916 |    2888 B |       15.04 |
| ExecuteAsync_Exception_V8 | 20,804.6 ns | 144.38 ns | 192.74 ns | 26.34 |    0.24 | 0.0610 |    1816 B |        9.46 |
|   ExecuteAsync_Outcome_V8 |    789.8 ns |   3.93 ns |   5.37 ns |  1.00 |    0.00 | 0.0076 |     192 B |        1.00 |
