using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Polly.Core.Benchmarks;

var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .AddJob(Job.MediumRun.WithToolchain(InProcessEmitToolchain.Instance))
    .AddDiagnoser(MemoryDiagnoser.Default);

var switcher = BenchmarkSwitcher.FromAssembly(typeof(PollyVersion).Assembly);

if (args.Length > 0 && args[0] == "pick")
{
    switcher.Run(args, config);
}
else
{
    switcher.RunAll(config);
}
