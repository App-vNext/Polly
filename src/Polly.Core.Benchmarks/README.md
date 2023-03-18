# Benchmark results

```text
BenchmarkDotNet=v0.13.3, OS=Windows 11 (10.0.22621.1413)
Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.202
  [Host] : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2

Job=MediumRun  Toolchain=InProcessEmitToolchain  IterationCount=15
LaunchCount=2  WarmupCount=10
```

## PIPELINES

|                       Method | Components |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------------- |----------- |------------:|----------:|----------:|------------:|------:|--------:|-------:|----------:|------------:|
|           ExecutePipeline_V7 |          1 |    74.84 ns |  1.279 ns |  1.835 ns |    75.81 ns |  1.00 |    0.00 | 0.0362 |     304 B |        1.00 |
|           ExecutePipeline_V8 |          1 |    72.61 ns |  0.584 ns |  0.819 ns |    72.28 ns |  0.97 |    0.02 | 0.0048 |      40 B |        0.13 |
| ExecutePipeline_V8Delegating |          1 |    90.04 ns |  0.753 ns |  1.104 ns |    89.76 ns |  1.20 |    0.02 | 0.0048 |      40 B |        0.13 |
|                              |            |             |           |           |             |       |         |        |           |             |
|           ExecutePipeline_V7 |          2 |   156.67 ns |  2.810 ns |  4.119 ns |   154.12 ns |  1.00 |    0.00 | 0.0658 |     552 B |        1.00 |
|           ExecutePipeline_V8 |          2 |   185.87 ns |  1.932 ns |  2.580 ns |   185.02 ns |  1.19 |    0.04 | 0.0048 |      40 B |        0.07 |
| ExecutePipeline_V8Delegating |          2 |   127.92 ns |  1.264 ns |  1.772 ns |   129.00 ns |  0.82 |    0.02 | 0.0048 |      40 B |        0.07 |
|                              |            |             |           |           |             |       |         |        |           |             |
|           ExecutePipeline_V7 |          5 |   543.85 ns | 19.540 ns | 29.247 ns |   530.00 ns |  1.00 |    0.00 | 0.1545 |    1296 B |        1.00 |
|           ExecutePipeline_V8 |          5 |   335.67 ns |  3.903 ns |  5.598 ns |   336.22 ns |  0.62 |    0.04 | 0.0048 |      40 B |        0.03 |
| ExecutePipeline_V8Delegating |          5 |   189.99 ns |  4.045 ns |  5.929 ns |   189.05 ns |  0.35 |    0.02 | 0.0048 |      40 B |        0.03 |
|                              |            |             |           |           |             |       |         |        |           |             |
|           ExecutePipeline_V7 |         10 | 1,112.84 ns |  8.585 ns | 12.036 ns | 1,111.96 ns |  1.00 |    0.00 | 0.3014 |    2536 B |        1.00 |
|           ExecutePipeline_V8 |         10 |   543.85 ns |  6.205 ns |  9.095 ns |   545.30 ns |  0.49 |    0.01 | 0.0048 |      40 B |        0.02 |
| ExecutePipeline_V8Delegating |         10 |   258.11 ns |  0.881 ns |  1.318 ns |   258.30 ns |  0.23 |    0.00 | 0.0048 |      40 B |        0.02 |