using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .AddJob(Job.MediumRun.WithToolchain(InProcessEmitToolchain.Instance))
    .AddDiagnoser(MemoryDiagnoser.Default);

BenchmarkSwitcher.FromAssembly(typeof(PollyVersion).Assembly).Run(args, config);
