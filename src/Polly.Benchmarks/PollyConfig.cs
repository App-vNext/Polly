using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Polly.Benchmarks
{
    internal class PollyConfig : ManualConfig
    {
        public PollyConfig()
        {
            var job = Job.Default;

            AddDiagnoser(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);

            AddJob(PollyJob(job, useNuGet: true).AsBaseline());
            AddJob(PollyJob(job, useNuGet: false));
        }

        private static Job PollyJob(Job job, bool useNuGet)
        {
            var result = job
                .WithId("Polly" + (useNuGet ? string.Empty : "-dev"))
                .WithArguments(
                new[]
                {
                    new MsBuildArgument("/p:BenchmarkFromNuGet=" + useNuGet),
                    new MsBuildArgument("/p:SignAssembly=false"),
                });

            if (useNuGet)
            {
                result = result.WithNuGet("Polly", "7.2.1");
            }

            return result;
        }
    }
}
