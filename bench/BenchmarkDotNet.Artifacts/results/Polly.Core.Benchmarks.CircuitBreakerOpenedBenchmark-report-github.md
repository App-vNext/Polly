```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                    | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ExecuteAsync_Exception_V7 | 23,229.9 ns | 26.25 ns | 39.29 ns | 36.23 |    0.20 | 0.0916 |    2504 B |       13.04 |
| ExecuteAsync_Exception_V8 | 15,832.2 ns | 29.74 ns | 43.59 ns | 24.69 |    0.16 | 0.0610 |    1816 B |        9.46 |
| ExecuteAsync_Outcome_V8   |    641.2 ns |  2.07 ns |  2.97 ns |  1.00 |    0.00 | 0.0076 |     192 B |        1.00 |
