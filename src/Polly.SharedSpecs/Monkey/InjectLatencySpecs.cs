using System;
using System.Diagnostics;
using FluentAssertions;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
    public class InjectLatencySpecs : IDisposable
    {
        private readonly int Tolerance = 15; // milliseconds

        public InjectLatencySpecs()
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
        public void InjectLatency_Context_Free_Should_Introduce_Delay_If_Enabled()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatency(delay, 0.6, () => true);
            var executed = false;

            var sw = new Stopwatch();
            sw.Start();
            policy.Execute(() => { executed = true; });
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
        }

        [Fact]
        public void InjectLatency_Context_Free_Should_Not_Introduce_Delay_If_Dissabled()
        {
            var policy = Policy.InjectLatency(TimeSpan.FromMilliseconds(500), 0.6, () => false);
            var executed = false;

            var sw = new Stopwatch();
            sw.Start();
            policy.Execute(() => { executed = true; });
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
        }

        [Fact]
        public void InjectLatency_Context_Free_Should_Introduce_Delay_If_InjectionRate_Is_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatency(delay, 0.6, () => true);
            var executed = false;

            var sw = new Stopwatch();
            sw.Start();
            policy.Execute(() => { executed = true; });
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
        }

        [Fact]
        public void InjectLatency_Context_Free_Should_Not_Introduce_Delay_If_InjectionRate_Is_Not_Covered()
        {
            var delay = TimeSpan.FromMilliseconds(500);
            var policy = Policy.InjectLatency(delay, 0.3, () => true);
            var executed = false;

            var sw = new Stopwatch();
            sw.Start();
            policy.Execute(() => { executed = true; });
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
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

            var policy = Policy.InjectLatency(delay, 0.6, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
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

            var policy = Policy.InjectLatency(delay, 0.6, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
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

            var policy = Policy.InjectLatency(delay, 0.3, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
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

            var policy = Policy.InjectLatency(delay, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
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

            var policy = Policy.InjectLatency(delay, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
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

            var policy = Policy.InjectLatency(latencyProvider, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(delay, Tolerance);
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

            var policy = Policy.InjectLatency(latencyProvider, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
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

            var policy = Policy.InjectLatency(latencyProvider, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.Elapsed.Should().BeCloseTo(TimeSpan.FromMilliseconds(0));
        }

        #endregion
    }
}
