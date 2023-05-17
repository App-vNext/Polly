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

BenchmarkRunner.Run(typeof(PollyVersion).Assembly, config);
