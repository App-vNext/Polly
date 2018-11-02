using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
    public class InjectLatencyTResultAsyncSpecs : IDisposable
    {
        private readonly int Tolerance = 15; // milliseconds

        public InjectLatencyTResultAsyncSpecs()
        {
            RandomGenerator.GetRandomNumber = () => 0.5;
        }

        public void Dispose()
        {
            SystemClock.Reset();
            RandomGenerator.Reset();
        }

        #region Context Free

        [Fact]
        public async Task InjectLatency_Context_Free_Should_Introduce_Delay_If_Enabled()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, 0.6, () => true);
            var executed = false;

            var sw = new Stopwatch();
            sw.Start();

            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
        }

        [Fact]
        public async Task InjectLatency_Context_Free_Should_Not_Introduce_Delay_If_Dissabled()
        {
            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(TimeSpan.FromMilliseconds(500), 0.6, () => false);
            var executed = false;

            var sw = new Stopwatch();
            sw.Start();

            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
        }

        [Fact]
        public async Task InjectLatency_Context_Free_Should_Introduce_Delay_If_InjectionRate_Is_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, 0.6, () => true);
            var executed = false;

            var sw = new Stopwatch();
            sw.Start();

            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
        }

        [Fact]
        public async Task InjectLatency_Context_Free_Should_Not_Introduce_Delay_If_InjectionRate_Is_Not_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, 0.3, () => true);
            var executed = false;

            var sw = new Stopwatch();
            sw.Start();

            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
        }

        #endregion

        #region With Context

        [Fact]
        public async Task InjectLatency_With_Context_With_Enabled_Lambda_Should_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = true;

            Func<Context, Task<bool>> enabled = async (ctx) =>
            {
                return await Task.FromResult((bool)ctx["Enabled"]);
            };

            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, 0.6, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
        }

        [Fact]
        public async Task InjectLatency_With_Context_With_Enabled_Lambda_Should_Not_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = false;

            Func<Context, Task<bool>> enabled = async (ctx) =>
            {
                return await Task.FromResult((bool)ctx["Enabled"]);
            };

            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, 0.6, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
        }

        [Fact]
        public async Task InjectLatency_With_Context_With_Enabled_Lambda_Should_Not_Introduce_Delay_If_InjectionRate_Is_Not_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = true;

            Func<Context, Task<bool>> enabled = async (ctx) =>
            {
                return await Task.FromResult((bool)ctx["Enabled"]);
            };

            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, 0.3, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
        }

        [Fact]
        public async Task InjectLatency_With_Context_With_InjectionRate_Lambda_Should_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = true;
            context["InjectionRate"] = 0.6;

            Func<Context, Task<bool>> enabled = async (ctx) =>
            {
                return await Task.FromResult((bool)ctx["Enabled"]);
            };

            Func<Context, Task<double>> injectionRate = async (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return await Task.FromResult((double) ctx["InjectionRate"]);
                }

                return await Task.FromResult(0);
            };

            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
        }

        [Fact]
        public async Task InjectLatency_With_Context_With_InjectionRate_Lambda_Should_Not_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = true;
            context["InjectionRate"] = 0.3;

            Func<Context, Task<bool>> enabled = async (ctx) =>
            {
                return await Task.FromResult((bool)ctx["Enabled"]);
            };

            Func<Context, Task<double>> injectionRate = async (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return await Task.FromResult((double) ctx["InjectionRate"]);
                }

                return await Task.FromResult(0);
            };

            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
        }

        [Fact]
        public async Task InjectLatency_With_Context_With_Latency_Lambda_Should_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["ShouldInjectLatency"] = true;
            context["Enabled"] = true;
            context["InjectionRate"] = 0.6;

            Func<Context, CancellationToken, Task<TimeSpan>> latencyProvider = async (ctx, ct) =>
            {
                if ((bool)ctx["ShouldInjectLatency"])
                {
                    return await Task.FromResult(delay);
                }

                return await Task.FromResult(TimeSpan.FromMilliseconds(0));
            };

            Func<Context, Task<bool>> enabled = async (ctx) =>
            {
                return await Task.FromResult((bool)ctx["Enabled"]);
            };

            Func<Context, Task<double>> injectionRate = async (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return await Task.FromResult((double) ctx["InjectionRate"]);
                }

                return await Task.FromResult(0);
            };

            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(latencyProvider, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
        }

        [Fact]
        public async Task InjectLatency_With_Context_With_Latency_Lambda_Should_Not_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["ShouldInjectLatency"] = false;
            context["Enabled"] = true;
            context["InjectionRate"] = 0.6;

            Func<Context, CancellationToken, Task<TimeSpan>> latencyProvider = async (ctx, ct) =>
            {
                if ((bool)ctx["ShouldInjectLatency"])
                {
                    return await Task.FromResult(delay);
                }

                return await Task.FromResult(TimeSpan.FromMilliseconds(0));
            };

            Func<Context, Task<bool>> enabled = async (ctx) =>
            {
                return await Task.FromResult((bool)ctx["Enabled"]);
            };

            Func<Context, Task<double>> injectionRate = async (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return await Task.FromResult((double) ctx["InjectionRate"]);
                }

                return await Task.FromResult(0);
            };

            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(latencyProvider, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
        }

        [Fact]
        public async Task InjectLatency_With_Context_With_Latency_Lambda_Should_Not_Introduce_Delay_If_InjectionRate_Is_Not_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["ShouldInjectLatency"] = true;
            context["Enabled"] = true;
            context["InjectionRate"] = 0.3;

            Func<Context, CancellationToken, Task<TimeSpan>> latencyProvider = async (ctx, ct) =>
            {
                if ((bool)ctx["ShouldInjectLatency"])
                {
                    return await Task.FromResult(delay);
                }

                return await Task.FromResult(TimeSpan.FromMilliseconds(0));
            };

            Func<Context, Task<bool>> enabled = async (ctx) =>
            {
                return await Task.FromResult((bool)ctx["Enabled"]);
            };

            Func<Context, Task<double>> injectionRate = async (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return await Task.FromResult((double) ctx["InjectionRate"]);
                }

                return await Task.FromResult(0);
            };

            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(latencyProvider, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
        }

        #endregion
    }
}
