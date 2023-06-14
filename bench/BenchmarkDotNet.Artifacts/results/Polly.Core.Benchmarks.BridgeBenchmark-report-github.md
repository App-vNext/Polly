``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1702/22H2/2022Update/SunValley2)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.203
  [Host] : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15  
LaunchCount=2  WarmupCount=10  

```
|                 Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|              NoOpAsync |  79.57 ns |  2.665 ns |  3.648 ns |  1.00 |    0.00 | 0.0362 |     304 B |        1.00 |
| NullResilienceStrategy | 413.16 ns | 11.365 ns | 16.658 ns |  5.19 |    0.33 | 0.0448 |     376 B |        1.24 |
