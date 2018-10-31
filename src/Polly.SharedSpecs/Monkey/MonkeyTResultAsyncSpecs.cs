using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Monkey;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.RandomGeneratorDependentTestCollection)]
    public class MonkeyTResultAsyncSpecs : IDisposable
    {
        public MonkeyTResultAsyncSpecs()
        {
            RandomGenerator.GetRandomNumber = () => 0.5;
        }

        public void Dispose()
        {
            RandomGenerator.Reset();
        }

        #region Action based Monkey policies
        [Fact]
        public async Task Monkey_Context_Free_Introduce_No_Delay_async()
        {
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Stopwatch sw = new Stopwatch();
            //// No delay
            sw.Reset();
            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(
                (cts) => Task.FromResult(ResultPrimitive.FaultAgain), 0.6, () => true);
            executed = false;

            sw.Restart();
            await policy.ExecuteAsync(actionAsync);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public async Task Monkey_Context_Free_Introduce_Delay_async()
        {
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<CancellationToken, Task> fault = async (cts) =>
            {
                await Task.Delay(200);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 1, () => true);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(actionAsync);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);
        }

        [Fact]
        public void Monkey_Context_Free_Throw_Exception_async()
        {
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<CancellationToken, Task> fault = async (cts) =>
            {
                await Task.Run(() => { });
                throw new Exception();
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, () => true);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_Context_Free_Not_Throw_Exception_With_Low_Injection_Rate_async()
        {
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<CancellationToken, Task> fault = async (cts) =>
            {
                await Task.Run(() => { });
                throw new Exception();
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.4, () => true);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }

        [Fact]
        public async Task Monkey_With_Context_Introduce_Delay_With_Fault_Lambda_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    await Task.Delay(200);
                }
            };

            Func<Context, Task<bool>> enabled = (cts) => Task.FromResult(true);

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["ShouldFail"] = false;
            executed = false;
            sw.Restart();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public async Task Monkey_With_Context_Introduce_Delay_With_Enabled_Lambda_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            context["Enabled"] = true;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    await Task.Delay(200);
                }
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult((bool)ctx["Enabled"]);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["Enabled"] = false;
            executed = false;
            sw.Restart();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public async Task Monkey_With_Context_Introduce_Delay_With_Injection_Rate_Lambda_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            context["Enabled"] = true;
            context["InjectionRate"] = 0.6;

            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    await Task.Delay(200);
                }
            };

            Func<Context, Task<double>> injectionRate = (ctx) =>
            {
                double rate = 0;
                if (ctx["InjectionRate"] != null)
                {
                    rate = (double)ctx["InjectionRate"];
                }

                return Task.FromResult(rate);
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult((bool)ctx["Enabled"]);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["InjectionRate"] = 0.4;
            executed = false;
            sw.Restart();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }
        #endregion

        #region TResult Based Monkey Policies
        [Fact]
        public async Task Monkey_Context_Free_Should_Return_Fault_async()
        {
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            ResultPrimitive fault = ResultPrimitive.Fault;

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, () => true);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Monkey_Context_Free_Should_Not_Return_Fault_async()
        {
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            ResultPrimitive fault = ResultPrimitive.Fault;

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.4, () => true);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }

        [Fact]
        public async Task Monkey_With_Context_Enabled_Should_Return_Fault_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            ResultPrimitive fault = ResultPrimitive.Fault;
            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult((bool)ctx["ShouldFail"]);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Monkey_With_Context_Enabled_Should_Not_Return_Fault_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = false;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            ResultPrimitive fault = ResultPrimitive.Fault;
            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult((bool)ctx["ShouldFail"]);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }

        [Fact]
        public async Task Monkey_With_Context_Fault_Should_Return_Fault_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    return Task.FromResult(ResultPrimitive.Fault);
                }

                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult((bool)ctx["ShouldFail"]);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Monkey_With_Context_Fault_Should_Not_Return_Fault_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = false;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
            {
                if ((bool)ctx["ShouldFail"])
                {
                    return Task.FromResult(ResultPrimitive.Fault);
                }

                return Task.FromResult(ResultPrimitive.GoodAgain);
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult(true);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.GoodAgain);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Monkey_With_Context_InjectionRate_Should_Return_Fault_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["InjectionRate"] = 0.6;

            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
            {
                return Task.FromResult(ResultPrimitive.Fault);
            };

            Func<Context, Task<Double>> injectionRate = (ctx) =>
            {
                double rate = 0;
                if (ctx["InjectionRate"] != null)
                {
                    rate = (double)ctx["InjectionRate"];
                }

                return Task.FromResult(rate);
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult(true);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Monkey_With_Context_InjectionRate_Should_Not_Return_Fault_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["InjectionRate"] = 0.4;

            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
            {
                return Task.FromResult(ResultPrimitive.Fault);
            };

            Func<Context, Task<Double>> injectionRate = (ctx) =>
            {
                double rate = 0;
                if (ctx["InjectionRate"] != null)
                {
                    rate = (double)ctx["InjectionRate"];
                }

                return Task.FromResult(rate);
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult(true);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }
        #endregion
    }
}
