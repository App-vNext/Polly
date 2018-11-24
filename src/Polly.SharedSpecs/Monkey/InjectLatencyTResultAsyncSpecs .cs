using System;
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
        private int _totalTimeSlept = 0;

        public InjectLatencyTResultAsyncSpecs()
        {
            ThreadSafeRandom_LockOncePerThread.NextDouble = () => 0.5;
            SystemClock.SleepAsync = async (span, ct) => _totalTimeSlept += await Task.FromResult(span.Milliseconds);
        }

        public void Dispose()
        {
            _totalTimeSlept = 0;
            SystemClock.Reset();
            ThreadSafeRandom_LockOncePerThread.Reset();
        }

        #region Context Free

        [Fact]
        public async Task InjectLatency_Context_Free_Should_Introduce_Delay_If_Enabled()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, 0.6, () => true);
            var executed = false;

            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
        }

        [Fact]
        public async Task InjectLatency_Context_Free_Should_Not_Introduce_Delay_If_Dissabled()
        {
            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(TimeSpan.FromMilliseconds(500), 0.6, () => false);
            var executed = false;

            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
        }

        [Fact]
        public async Task InjectLatency_Context_Free_Should_Introduce_Delay_If_InjectionRate_Is_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, 0.6, () => true);
            var executed = false;

            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
        }

        [Fact]
        public async Task InjectLatency_Context_Free_Should_Not_Introduce_Delay_If_InjectionRate_Is_Not_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatencyAsync<ResultPrimitive>(delay, 0.3, () => true);
            var executed = false;

            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
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

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
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

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
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

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
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

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
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

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
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

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
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

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
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

            Func<Context, Task<ResultPrimitive>> actionAsync = _ => { executed = true; return Task.FromResult(ResultPrimitive.Good); };
            var result = await policy.ExecuteAsync(actionAsync, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
        }

        #endregion
    }
}
