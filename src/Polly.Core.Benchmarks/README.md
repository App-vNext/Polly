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

|             Method | Components |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------- |----------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| ExecutePipeline_V7 |          1 |    81.25 ns |  1.304 ns |  1.870 ns |  1.00 |    0.00 | 0.0362 |     304 B |        1.00 |
| ExecutePipeline_V8 |          1 |    82.25 ns |  1.414 ns |  2.073 ns |  1.01 |    0.04 | 0.0048 |      40 B |        0.13 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |          2 |   166.04 ns |  2.875 ns |  4.215 ns |  1.00 |    0.00 | 0.0658 |     552 B |        1.00 |
| ExecutePipeline_V8 |          2 |   108.29 ns |  1.504 ns |  2.251 ns |  0.65 |    0.02 | 0.0048 |      40 B |        0.07 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |          5 |   531.04 ns |  4.728 ns |  6.930 ns |  1.00 |    0.00 | 0.1545 |    1296 B |        1.00 |
| ExecutePipeline_V8 |          5 |   245.50 ns |  2.344 ns |  3.509 ns |  0.46 |    0.01 | 0.0048 |      40 B |        0.03 |
|                    |            |             |           |           |       |         |        |           |             |
| ExecutePipeline_V7 |         10 | 1,128.82 ns | 10.838 ns | 15.886 ns |  1.00 |    0.00 | 0.3014 |    2536 B |        1.00 |
| ExecutePipeline_V8 |         10 |   449.31 ns |  2.926 ns |  4.379 ns |  0.40 |    0.01 | 0.0048 |      40 B |        0.02 |