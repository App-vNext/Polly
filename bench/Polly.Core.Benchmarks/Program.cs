using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .AddJob(Job.MediumRun.WithToolchain(InProcessEmitToolchain.Instance))
    .AddDiagnoser(MemoryDiagnoser.Default);

var summary = BenchmarkSwitcher.FromAssembly(typeof(PollyVersion).Assembly).Run(args, config);
return summary.SelectMany((p) => p.Reports).Any((p) => !p.Success) ? 1 : 0;
