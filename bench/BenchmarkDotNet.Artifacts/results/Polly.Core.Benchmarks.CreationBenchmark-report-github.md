```

BenchmarkDotNet v0.13.11, Windows 11 (10.0.22631.2861/23H2/2023Update/SunValley3) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
| Method      | Mean        | Error     | StdDev    | Median      | Gen0   | Allocated |
|------------ |------------:|----------:|----------:|------------:|-------:|----------:|
| Fallback_V7 |    78.52 ns |  3.274 ns |  4.901 ns |    76.04 ns | 0.0191 |     480 B |
| Fallback_V8 | 2,832.14 ns | 11.383 ns | 17.037 ns | 2,832.55 ns | 0.2251 |    5704 B |
