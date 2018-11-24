using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Polly.Monkey;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
    public class MonkeySpecs : IDisposable
    {
        public MonkeySpecs()
        {
            ThreadSafeRandom_LockOncePerThread.NextDouble = () => 0.5;
        }

        public void Dispose()
        {
            ThreadSafeRandom_LockOncePerThread.Reset();
        }

        #region Action based Monkey policies
        [Fact]
        public void Monkey_Context_Free_Introduce_No_Delay()
        {
            Boolean executed = false;
            Stopwatch sw = new Stopwatch();
            MonkeyPolicy policy = Policy.Monkey(() => { }, 0.6, () => true);
            executed = false;

            sw.Restart();
            policy.Execute(() => { executed = true; });
            sw.Stop();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_Context_Free_Introduce_Delay()
        {
            Action fault = () =>
            {
                Thread.Sleep(200);
            };

            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, () => true);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute(() => { executed = true; });
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);
        }

        [Fact]
        public void Monkey_Context_Free_Throw_Exception()
        {
            Action fault = () => {
                throw new InvalidOperationException();
            };

            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, () => true);
            Boolean executed = false;
            policy.Invoking(x => x.Execute(() => { executed = true; })).ShouldThrowExactly<InvalidOperationException>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_Context_Free_Not_Throw_Exception_With_Low_Injection_Rate()
        {
            Action fault = () => {
                throw new InvalidOperationException();
            };

            MonkeyPolicy policy = Policy.Monkey(fault, 0.4, () => true);
            Boolean executed = false;
            policy.Invoking(x => x.Execute(() => { executed = true; })).ShouldNotThrow<InvalidOperationException>();
            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_With_Fault_Lambda()
        {
            Context context = new Context();
            context["ShouldFail"] = true;

            Action<Context> fault = (ctx) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    Thread.Sleep(200);
                }
            };

            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, () => true);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["ShouldFail"] = false;
            executed = false;
            sw.Restart();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_With_Enabled_Lambda()
        {
            Context context = new Context();
            context["ShouldFail"] = true;
            context["Enabled"] = true;

            Action<Context> fault = (ctx) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    Thread.Sleep(200);
                }
            };

            Func<Context, bool> enabled = (ctx) => {
                return (bool)ctx["Enabled"];
            };

            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["Enabled"] = false;
            executed = false;
            sw.Restart();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_Injection_Rate_Lambda()
        {
            Context context = new Context();
            context["ShouldFail"] = true;
            context["Enabled"] = true;
            context["InjectionRate"] = 0.6;

            Action<Context> fault = (ctx) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    Thread.Sleep(200);
                }
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

            MonkeyPolicy policy = Policy.Monkey(fault, injectionRate, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["InjectionRate"] = 0.4;
            executed = false;
            sw.Restart();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }
        #endregion
    }
}
