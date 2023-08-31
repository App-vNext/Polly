```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.2134/22H2/2022Update/SunValley2) (Hyper-V)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.400
  [Host] : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                 Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
|              NoOpAsync | 113.0 ns | 1.65 ns | 2.47 ns |  1.00 |    0.00 | 0.0120 |     304 B |        1.00 |
| NullResiliencePipeline | 509.4 ns | 2.95 ns | 4.32 ns |  4.51 |    0.10 | 0.0143 |     376 B |        1.24 |
