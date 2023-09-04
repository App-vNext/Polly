# Benchmarks

To run the benchmarks, use the `benchmarks.ps1` script in this repository:

``` powershell
# Run all benchmarks
./benchmarks.ps1

# Pick benchmarks to run
./benchmarks.ps1 -Interactive
```

The benchmark results are stored in [`BenchmarkDotNet.Artifacts/results`](BenchmarkDotNet.Artifacts/results/) folder.

Run the benchmarks when your changes are significant enough to make sense running them. We do not use fixed hardware so your numbers might differ, however the important is the `Ratio` and `Alloc Ratio` which stays around the same or improves (ideally) between runs.
