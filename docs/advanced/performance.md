# Performance

Polly is FAST and zero allocating as much as possible. We use comprehensive set of [performance benchmarks](https://github.com/App-vNext/Polly/tree/main/bench/Polly.Core.Benchmarks) to monitor the Polly performance.

For example, here are the results of advanced pipeline composed of the following strategies:

- Timeout (outer)
- RateLimiter
- Retry
- Circuit Breaker
- Timeout (inner)

---

| Method              |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
| ------------------- | -------: | --------: | --------: | ----: | ------: | -----: | --------: | ----------: |
| Execute Policy V7   | 2.277 μs | 0.0133 μs | 0.0191 μs |  1.00 |    0.00 | 0.1106 |    2824 B |        1.00 |
| Execute Pipeline V8 | 2.089 μs | 0.0105 μs | 0.0157 μs |  0.92 |    0.01 |      - |      40 B |        0.01 |

---

Compared to older versions of Polly, v8 is both faster and consumes much less memory.

## Performance tips

Here are some tips if you want to archive best possible performance when using Polly.

### Use static lambdas

Lambdas that capture variables from outer scope allocate per each execution. Polly gives you tools to avoid this overhead as demonstrated in the example bellow:

### Use switch expressions for predicates
