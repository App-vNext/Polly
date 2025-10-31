namespace Polly.Benchmarks;

internal class PollyConfig : ManualConfig
{
    public PollyConfig()
    {
        AddDiagnoser(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);
        AddJob(Job.Default);
    }
}
