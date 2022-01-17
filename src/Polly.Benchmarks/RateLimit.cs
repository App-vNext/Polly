﻿using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Polly.Benchmarks
{
    [Config(typeof(PollyConfig))]
    public class RateLimit
    {
        private static readonly Policy SyncPolicy = Policy.RateLimit(20, TimeSpan.FromSeconds(1), int.MaxValue);
        private static readonly AsyncPolicy AsyncPolicy = Policy.RateLimitAsync(20, TimeSpan.FromSeconds(1), int.MaxValue);

        [Benchmark]
        public void RateLimit_Synchronous_Succeeds()
        {
            SyncPolicy.Execute(() => Workloads.Action());
        }

        [Benchmark]
        public async Task RateLimit_Asynchronous_Succeeds()
        {
            await AsyncPolicy.ExecuteAsync(() => Workloads.ActionAsync());
        }

        [Benchmark]
        public int RateLimit_Synchronous_With_Result_Succeeds()
        {
            return SyncPolicy.Execute(() => Workloads.Func<int>());
        }

        [Benchmark]
        public async Task<int> RateLimit_Asynchronous_With_Result_Succeeds()
        {
            return await AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());
        }
    }
}
