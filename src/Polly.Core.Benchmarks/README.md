# Benchmarks

To run the benchmarks:

``` powershell
# run all benchmarks
dotnet run -c release -f net7.0

# pick benchmarks to run
dotnet run -c release -f net7.0 -- pick
```

The benchmark results are stored in [`BenchmarkDotNet.Artifacts/results`](BenchmarkDotNet.Artifacts/results/) folder.

Run the benchmarks when your changes are significant enough to make sense running them. We do not use fixed hardware so your numbers might differ, however the important is the `Ratio` and `Alloc Ratio` which stays around the same or improves (ideally) between runs. 
