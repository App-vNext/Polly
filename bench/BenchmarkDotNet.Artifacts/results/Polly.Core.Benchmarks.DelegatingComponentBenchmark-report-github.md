```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method                              | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| DelegatingComponent_ExecuteCore_Jit | 23.53 ns | 0.026 ns | 0.037 ns |  1.00 |    0.00 |      - |         - |          NA |
| DelegatingComponent_ExecuteCore_Aot | 33.34 ns | 0.837 ns | 1.228 ns |  1.42 |    0.05 | 0.0010 |      24 B |          NA |
