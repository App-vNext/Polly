using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Polly.Monkey;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.RandomGeneratorDependentTestCollection)]
    public class MonkeyTResultSpecs : IDisposable
    {
        public MonkeyTResultSpecs()
        {
            RandomGenerator.GetRandomNumber = () => 0.5;
        }

        public void Dispose()
        {
            RandomGenerator.Reset();
        }

        #region Action based Monkey policies
        [Fact]
        public void Monkey_Context_Free_Introduce_No_Delay()
        {
            Boolean executed = false;
            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };

            Stopwatch sw = new Stopwatch();
            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(() => { }, 0.6, () => true);
            executed = false;

            sw.Restart();
            policy.Execute(action);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_Context_Free_Introduce_Delay()
        {
            Boolean executed = false;
            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };

            Action fault = () =>
            {
                Thread.Sleep(200);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 1, () => true);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute(action);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);
        }

        [Fact]
        public void Monkey_Context_Free_Throw_Exception()
        {
            Func<ResultPrimitive> action = () => { return ResultPrimitive.Good; };
            Action fault = () => {
                throw new Exception();
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, () => true);
            policy.Invoking(x => x.Execute(action)).ShouldThrowExactly<Exception>();
        }

        [Fact]
        public void Monkey_Context_Free_Not_Throw_Exception()
        {
            Func<ResultPrimitive> action = () => { return ResultPrimitive.Good; };
            Action fault = () => {
                throw new Exception();
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.4, () => true);
            policy.Invoking(x => x.Execute(action)).ShouldNotThrow<Exception>();
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_With_Fault_Lambda()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Action<Context> fault = (ctx) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    Thread.Sleep(200);
                }
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, () => true);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["ShouldFail"] = false;
            executed = false;
            sw.Restart();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_With_Enabled_Lambda()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            context["Enabled"] = true;
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Action<Context> fault = (ctx) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    Thread.Sleep(200);
                }
            };

            Func<Context, bool> enabled = (ctx) => {
                return ((bool)ctx["Enabled"]);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["Enabled"] = false;
            executed = false;
            sw.Restart();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_With_Injection_Rate_Lambda()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            context["Enabled"] = true;
            context["InjectionRate"] = 0.6;
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

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

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, injectionRate, enabled);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["InjectionRate"] = 0.4;
            executed = false;
            sw.Restart();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }
        #endregion

        #region TResult Based Monkey Policies
        [Fact]
        public void Monkey_Context_Free_Should_Return_Fault()
        {
            Boolean executed = false;
            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };
            ResultPrimitive fault = ResultPrimitive.Fault;

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, () => true);
            ResultPrimitive response = policy.Execute(action);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_Context_Free_Should_Not_Return_Fault()
        {
            Boolean executed = false;
            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };
            ResultPrimitive fault = ResultPrimitive.Fault;

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.4, () => true);
            ResultPrimitive response = policy.Execute(action);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_With_Context_Enabled_Should_Return_Fault()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            ResultPrimitive fault = ResultPrimitive.Fault;
            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["ShouldFail"]);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = policy.Execute(action, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Enabled_Should_Not_Return_Fault()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = false;
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            ResultPrimitive fault = ResultPrimitive.Fault;
            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["ShouldFail"]);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = policy.Execute(action, context);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_With_Context_Fault_Should_Return_Fault()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            Func<Context, ResultPrimitive> fault = (ctx) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    return ResultPrimitive.Fault;
                }

                return ResultPrimitive.GoodAgain;
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return true;
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = policy.Execute(action, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Fault_Should_Not_Return_Fault()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = false;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            Func<Context, ResultPrimitive> fault = (ctx) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    return ResultPrimitive.Fault;
                }

                return ResultPrimitive.GoodAgain;
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return true;
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = policy.Execute(action, context);
            response.Should().Be(ResultPrimitive.GoodAgain);
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_Return_Fault()
        {
            Boolean executed = false;
            Context context = new Context();
            context["InjectionRate"] = 0.6;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            Func<Context, ResultPrimitive> fault = (ctx) =>
            {
                return ResultPrimitive.Fault;
            };

            Func<Context, Double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return (double)ctx["InjectionRate"];
                }

                return 0;
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return true;
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, injectionRate, enabled);
            ResultPrimitive response = policy.Execute(action, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_Not_Return_Fault()
        {
            Boolean executed = false;
            Context context = new Context();
            context["InjectionRate"] = 0.4;

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            Func<Context, ResultPrimitive> fault = (ctx) =>
            {
                return ResultPrimitive.Fault;
            };

            Func<Context, Double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return (double)ctx["InjectionRate"];
                }

                return 0;
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return true;
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, injectionRate, enabled);
            ResultPrimitive response = policy.Execute(action, context);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }
        #endregion
    }
}
