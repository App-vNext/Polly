using System;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
    public class InjectLatencyTResultSpecs : IDisposable
    {
        private int _totalTimeSlept = 0;

        public InjectLatencyTResultSpecs()
        {
            RandomGenerator.GetRandomNumber = () => 0.5;
            SystemClock.Sleep = (span, ct) => _totalTimeSlept += span.Milliseconds;
        }

        public void Dispose()
        {
            _totalTimeSlept = 0;
            SystemClock.Reset();
            RandomGenerator.Reset();
        }

        #region Context Free

        [Fact]
        public void InjectLatency_Context_Free_Should_Introduce_Delay_If_Enabled()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatency<ResultPrimitive>(delay, 0.6, () => true);
            var executed = false;

            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
        }

        [Fact]
        public void InjectLatency_Context_Free_Should_Not_Introduce_Delay_If_Dissabled()
        {
            var policy = Policy.InjectLatency<ResultPrimitive>(TimeSpan.FromMilliseconds(500), 0.6, () => false);
            var executed = false;

            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
        }

        [Fact]
        public void InjectLatency_Context_Free_Should_Introduce_Delay_If_InjectionRate_Is_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatency<ResultPrimitive>(delay, 0.6, () => true);
            var executed = false;

            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
        }

        [Fact]
        public void InjectLatency_Context_Free_Should_Not_Introduce_Delay_If_InjectionRate_Is_Not_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatency<ResultPrimitive>(delay, 0.3, () => true);
            var executed = false;

            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
        }

        #endregion

        #region With Context

        [Fact]
        public void InjectLatency_With_Context_With_Enabled_Lambda_Should_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = true;

            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["Enabled"]);
            };

            var policy = Policy.InjectLatency<ResultPrimitive>(delay, 0.6, enabled);
            Boolean executed = false;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
        }

        [Fact]
        public void InjectLatency_With_Context_With_Enabled_Lambda_Should_Not_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = false;

            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["Enabled"]);
            };

            var policy = Policy.InjectLatency<ResultPrimitive>(delay, 0.6, enabled);
            Boolean executed = false;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
        }

        [Fact]
        public void InjectLatency_With_Context_With_Enabled_Lambda_Should_Not_Introduce_Delay_If_InjectionRate_Is_Not_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = true;

            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["Enabled"]);
            };

            var policy = Policy.InjectLatency<ResultPrimitive>(delay, 0.3, enabled);
            Boolean executed = false;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
        }

        [Fact]
        public void InjectLatency_With_Context_With_InjectionRate_Lambda_Should_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = true;
            context["InjectionRate"] = 0.6;

            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["Enabled"]);
            };

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return (double)ctx["InjectionRate"];
                }

                return 0;
            };

            var policy = Policy.InjectLatency<ResultPrimitive>(delay, injectionRate, enabled);
            Boolean executed = false;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
        }

        [Fact]
        public void InjectLatency_With_Context_With_InjectionRate_Lambda_Should_Not_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["Enabled"] = true;
            context["InjectionRate"] = 0.3;

            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["Enabled"]);
            };

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return (double)ctx["InjectionRate"];
                }

                return 0;
            };

            var policy = Policy.InjectLatency<ResultPrimitive>(delay, injectionRate, enabled);
            Boolean executed = false;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
        }

        [Fact]
        public void InjectLatency_With_Context_With_Latency_Lambda_Should_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["ShouldInjectLatency"] = true;
            context["Enabled"] = true;
            context["InjectionRate"] = 0.6;

            Func<Context, TimeSpan> latencyProvider = (ctx) =>
            {
                if ((bool)ctx["ShouldInjectLatency"])
                {
                    return delay;
                }

                return TimeSpan.FromMilliseconds(0);
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["Enabled"]);
            };

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return (double)ctx["InjectionRate"];
                }

                return 0;
            };

            var policy = Policy.InjectLatency<ResultPrimitive>(latencyProvider, injectionRate, enabled);
            Boolean executed = false;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(delay.Milliseconds);
        }

        [Fact]
        public void InjectLatency_With_Context_With_Latency_Lambda_Should_Not_Introduce_Delay()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["ShouldInjectLatency"] = false;
            context["Enabled"] = true;
            context["InjectionRate"] = 0.6;

            Func<Context, TimeSpan> latencyProvider = (ctx) =>
            {
                if ((bool)ctx["ShouldInjectLatency"])
                {
                    return delay;
                }

                return TimeSpan.FromMilliseconds(0);
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["Enabled"]);
            };

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return (double)ctx["InjectionRate"];
                }

                return 0;
            };

            var policy = Policy.InjectLatency<ResultPrimitive>(latencyProvider, injectionRate, enabled);
            Boolean executed = false;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action, context);

            executed.Should().BeTrue();
            result.Should().Be(ResultPrimitive.Good);
            _totalTimeSlept.Should().Be(0);
        }

        [Fact]
        public void InjectLatency_With_Context_With_Latency_Lambda_Should_Not_Introduce_Delay_If_InjectionRate_Is_Not_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var context = new Context();
            context["ShouldInjectLatency"] = true;
            context["Enabled"] = true;
            context["InjectionRate"] = 0.3;

            Func<Context, TimeSpan> latencyProvider = (ctx) =>
            {
                if ((bool)ctx["ShouldInjectLatency"])
                {
                    return delay;
                }

                return TimeSpan.FromMilliseconds(0);
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["Enabled"]);
            };

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return (double)ctx["InjectionRate"];
                }

                return 0;
            };

            var policy = Policy.InjectLatency<ResultPrimitive>(latencyProvider, injectionRate, enabled);
            Boolean executed = false;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            var result = policy.Execute(action, context);

            executed.Should().BeTrue();
            _totalTimeSlept.Should().Be(0);
        }

        #endregion
    }
}
