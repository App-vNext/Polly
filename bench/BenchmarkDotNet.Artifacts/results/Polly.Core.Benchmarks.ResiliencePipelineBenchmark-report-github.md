```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                                         | Mean      | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|----------------------------------------------- |----------:|---------:|---------:|------:|--------:|----------:|------------:|
| ExecuteOutcomeAsync                            |  44.54 ns | 0.395 ns | 0.579 ns |  1.00 |    0.00 |         - |          NA |
| ExecuteAsync_ResilienceContextAndState         | 130.02 ns | 0.845 ns | 1.212 ns |  2.92 |    0.04 |         - |          NA |
| ExecuteAsync_CancellationToken                 | 149.78 ns | 1.103 ns | 1.617 ns |  3.36 |    0.05 |         - |          NA |
| ExecuteAsync_GenericStrategy_CancellationToken | 146.42 ns | 0.077 ns | 0.115 ns |  3.29 |    0.04 |         - |          NA |
| Execute_ResilienceContextAndState              |  45.44 ns | 0.524 ns | 0.768 ns |  1.02 |    0.03 |         - |          NA |
| Execute_CancellationToken                      |  69.38 ns | 1.320 ns | 1.894 ns |  1.56 |    0.05 |         - |          NA |
| Execute_GenericStrategy_CancellationToken      |  67.92 ns | 0.542 ns | 0.760 ns |  1.53 |    0.02 |         - |          NA |
