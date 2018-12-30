using System;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Monkey;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
    public class InjectBehaviourTResultAsyncSpecs : IDisposable
    {
        public InjectBehaviourTResultAsyncSpecs()
        {
            ThreadSafeRandom_LockOncePerThread.NextDouble = () => 0.5;
        }

        public void Dispose()
        {
            ThreadSafeRandom_LockOncePerThread.Reset();
        }

        #region Action based Monkey policies

        [Fact]
        public void Given_not_enabled_should_not_inject_behaviour()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviourAsync<ResultPrimitive>(() => { injectedBehaviourExecuted = true; return Task.CompletedTask; }, 0.6, () => Task.FromResult(false));

            policy.ExecuteAsync(() => { userDelegateExecuted = true; return Task.FromResult(ResultPrimitive.Good); });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeFalse();
        }

        [Fact]
        public void Given_enabled_and_randomly_within_threshold_should_inject_behaviour()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviourAsync<ResultPrimitive>(() => { injectedBehaviourExecuted = true; return Task.CompletedTask; }, 0.6, () => Task.FromResult(true));

            policy.ExecuteAsync(() => { userDelegateExecuted = true; return Task.FromResult(ResultPrimitive.Good); });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeTrue();
        }

        [Fact]
        public void Given_enabled_and_randomly_not_within_threshold_should_not_inject_behaviour()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviourAsync<ResultPrimitive>(() => { injectedBehaviourExecuted = true; return Task.CompletedTask; }, 0.4, () => Task.FromResult(true));

            policy.ExecuteAsync(() => { userDelegateExecuted = true; return Task.FromResult(ResultPrimitive.Good); });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_inject_behaviour_before_executing_user_delegate()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviourAsync<ResultPrimitive>(() =>
            {
                userDelegateExecuted.Should().BeFalse(); // Not yet executed at the time the injected behaviour runs.
                injectedBehaviourExecuted = true;
                return Task.CompletedTask;
            }, 0.6, () => Task.FromResult(true));

            policy.ExecuteAsync(() => { userDelegateExecuted = true; return Task.FromResult(ResultPrimitive.Good); });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeTrue();
        }

        #endregion
    }
}
//using System;
//using System.Diagnostics;
//using System.Threading;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Polly.Monkey;
//using Polly.Utilities;
//using Xunit;

//namespace Polly.Specs.Monkey
//{
//    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
//    public class MonkeyAsyncSpecs : IDisposable
//    {
//        public MonkeyAsyncSpecs()
//        {
//            ThreadSafeRandom_LockOncePerThread.NextDouble = () => 0.5;
//        }

//        public void Dispose()
//        {
//            ThreadSafeRandom_LockOncePerThread.Reset();
//        }

//        #region Action based Monkey policies
//        [Fact]
//        public async Task Monkey_Context_Free_No_Action_Lambda_No_Delay()
//        {
//            //// No delay
//            Boolean executed = false;
//            Stopwatch sw = new Stopwatch();
//            sw.Reset();

//            Func<Context, Task<bool>> enabled = (cts) => Task.FromResult(true);
//            var policy = MonkeyPolicy.MonkeyAsync((cts) => TaskHelper.EmptyTask, 0.6, enabled);
//            executed = false;
//            sw.Restart();
//            await policy.ExecuteAsync(Async(() => { executed = true; return TaskHelper.EmptyTask; });
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeLessThan(200);
//        }

//        [Fact]
//        public async Task Monkey_Context_Free_Introduce_Delay_async()
//        {
//            Func<CancellationToken, Task> fault = async (cts) =>
//            {
//                await Task.Delay(200);
//            };

//            Func<Context, Task<bool>> enabled = (cts) => Task.FromResult(true);

//            var policy = MonkeyPolicy.MonkeyAsync(fault, 0.6, enabled);
//            Boolean executed = false;

//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await policy.ExecuteAsync(Async(() => { executed = true; return TaskHelper.EmptyTask; });
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);
//        }

//        [Fact]
//        public void Monkey_Context_Free_Throw_Exception_async()
//        {
//            Boolean executed = false;
//            Func<CancellationToken, Task> fault = async (cts) =>
//            {
//                await Task.Run(() => { });
//                throw new Exception();
//            };

//            var policy = MonkeyPolicy.MonkeyAsync(fault, 0.6, (cts) => Task.FromResult(true));
//            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
//            policy.Awaiting(async x => await x.ExecuteAsync(Async(actionAsync, new Context()))
//                .ShouldThrowExactly<Exception>();

//            executed.Should().BeFalse();
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Introduce_Delay_async_with_fault_lambda()
//        {
//            Context context = new Context();
//            context["ShouldFail"] = true;

//            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
//            {
//                if ((bool)ctx["ShouldFail"])
//                {
//                    await Task.Delay(200);
//                }
//            };

//            Func<Context, Task<bool>> enabled = (cts) => Task.FromResult(true);

//            var policy = MonkeyPolicy.MonkeyAsync(fault, 0.6, enabled);
//            Boolean executed = false;
//            Func<Context, Task> action = (cts) => { executed = true; return TaskHelper.EmptyTask; };

//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await policy.ExecuteAsync(Async(action, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

//            //// No delay
//            context["ShouldFail"] = false;
//            executed = false;
//            sw.Restart();
//            await policy.ExecuteAsync(Async(action, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeLessThan(200);
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Introduce_Delay_async_with_enabled_lambda()
//        {
//            Context context = new Context();
//            context["ShouldFail"] = true;
//            context["Enabled"] = true;

//            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
//            {
//                if ((bool)ctx["ShouldFail"])
//                {
//                    await Task.Delay(200);
//                }
//            };

//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult((bool)ctx["Enabled"]);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync(fault, 0.6, enabled);
//            Boolean executed = false;
//            Func<Context, Task> action = (cts) => { executed = true; return TaskHelper.EmptyTask; };

//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await policy.ExecuteAsync(Async(action, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

//            //// No delay
//            context["Enabled"] = false;
//            executed = false;
//            sw.Restart();
//            await policy.ExecuteAsync(Async(action, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeLessThan(200);
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Introduce_Delay_async_with_injection_rate_lambda()
//        {
//            Context context = new Context();
//            context["ShouldFail"] = true;
//            context["Enabled"] = true;
//            context["InjectionRate"] = 0.6;

//            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
//            {
//                if ((bool)ctx["ShouldFail"])
//                {
//                    await Task.Delay(200);
//                }
//            };

//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult((bool)ctx["Enabled"]);
//            };

//            Func<Context, Task<double>> injectionRate = (ctx) =>
//            {
//                double rate = 0;
//                if (ctx["InjectionRate"] != null)
//                {
//                    rate = (double)ctx["InjectionRate"];
//                }

//                return Task.FromResult(rate);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync(fault, injectionRate, enabled);
//            Boolean executed = false;
//            Func<Context, Task> action = (cts) => { executed = true; return TaskHelper.EmptyTask; };

//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await policy.ExecuteAsync(Async(action, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

//            //// No delay
//            context["InjectionRate"] = 0.4;
//            executed = false;
//            sw.Restart();
//            await policy.ExecuteAsync(Async(action, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeLessThan(200);
//        }
//        #endregion
//    }
//}
//using System;
//using System.Diagnostics;
//using System.Threading;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Polly.Monkey;
//using Polly.Specs.Helpers;
//using Polly.Utilities;
//using Xunit;

//namespace Polly.Specs.Monkey
//{
//    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
//    public class MonkeyTResultAsyncSpecs : IDisposable
//    {
//        public MonkeyTResultAsyncSpecs()
//        {
//            ThreadSafeRandom_LockOncePerThread.NextDouble = () => 0.5;
//        }

//        public void Dispose()
//        {
//            ThreadSafeRandom_LockOncePerThread.Reset();
//        }

//        #region Action based Monkey policies
//        [Fact]
//        public async Task Monkey_Context_Free_Introduce_No_Delay_async()
//        {
//            Boolean executed = false;
//            Func<Task<ResultPrimitive>> actionAsync = () =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Stopwatch sw = new Stopwatch();
//            //// No delay
//            sw.Reset();
//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(
//                (cts) => Task.FromResult(ResultPrimitive.FaultAgain), 0.6, () => true);
//            executed = false;

//            sw.Restart();
//            await policy.ExecuteAsync(actionAsync);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeLessThan(200);
//        }

//        [Fact]
//        public async Task Monkey_Context_Free_Introduce_Delay_async()
//        {
//            Boolean executed = false;
//            Func<Task<ResultPrimitive>> actionAsync = () =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<CancellationToken, Task> fault = async (cts) =>
//            {
//                await Task.Delay(200);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 1, () => true);

//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await policy.ExecuteAsync(actionAsync);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);
//        }

//        [Fact]
//        public void Monkey_Context_Free_Throw_Exception_async()
//        {
//            Boolean executed = false;
//            Func<Task<ResultPrimitive>> actionAsync = () =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<CancellationToken, Task> fault = async (cts) =>
//            {
//                await Task.Run(() => { });
//                throw new Exception();
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.6, () => true);
//            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync))
//                .ShouldThrowExactly<Exception>();

//            executed.Should().BeFalse();
//        }

//        [Fact]
//        public void Monkey_Context_Free_Not_Throw_Exception_With_Low_Injection_Rate_async()
//        {
//            Boolean executed = false;
//            Func<Task<ResultPrimitive>> actionAsync = () =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<CancellationToken, Task> fault = async (cts) =>
//            {
//                await Task.Run(() => { });
//                throw new Exception();
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.4, () => true);
//            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync))
//                .ShouldNotThrow<Exception>();

//            executed.Should().BeTrue();
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Introduce_Delay_With_Fault_Lambda_async()
//        {
//            Boolean executed = false;
//            Context context = new Context();
//            context["ShouldFail"] = true;
//            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
//            {
//                if ((bool)ctx["ShouldFail"])
//                {
//                    await Task.Delay(200);
//                }
//            };

//            Func<Context, Task<bool>> enabled = (cts) => Task.FromResult(true);

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);

//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await policy.ExecuteAsync(actionAsync, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

//            //// No delay
//            context["ShouldFail"] = false;
//            executed = false;
//            sw.Restart();
//            await policy.ExecuteAsync(actionAsync, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeLessThan(200);
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Introduce_Delay_With_Enabled_Lambda_async()
//        {
//            Boolean executed = false;
//            Context context = new Context();
//            context["ShouldFail"] = true;
//            context["Enabled"] = true;
//            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
//            {
//                if ((bool)ctx["ShouldFail"])
//                {
//                    await Task.Delay(200);
//                }
//            };

//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult((bool)ctx["Enabled"]);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);

//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await policy.ExecuteAsync(actionAsync, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

//            //// No delay
//            context["Enabled"] = false;
//            executed = false;
//            sw.Restart();
//            await policy.ExecuteAsync(actionAsync, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeLessThan(200);
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Introduce_Delay_With_Injection_Rate_Lambda_async()
//        {
//            Boolean executed = false;
//            Context context = new Context();
//            context["ShouldFail"] = true;
//            context["Enabled"] = true;
//            context["InjectionRate"] = 0.6;

//            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
//            {
//                if ((bool)ctx["ShouldFail"])
//                {
//                    await Task.Delay(200);
//                }
//            };

//            Func<Context, Task<double>> injectionRate = (ctx) =>
//            {
//                double rate = 0;
//                if (ctx["InjectionRate"] != null)
//                {
//                    rate = (double)ctx["InjectionRate"];
//                }

//                return Task.FromResult(rate);
//            };

//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult((bool)ctx["Enabled"]);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);

//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await policy.ExecuteAsync(actionAsync, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

//            //// No delay
//            context["InjectionRate"] = 0.4;
//            executed = false;
//            sw.Restart();
//            await policy.ExecuteAsync(actionAsync, context);
//            sw.Stop();

//            executed.Should().BeTrue();
//            sw.ElapsedMilliseconds.Should().BeLessThan(200);
//        }
//        #endregion

//        #region TResult Based Monkey Policies
//        [Fact]
//        public async Task Monkey_Context_Free_Should_Return_Fault_async()
//        {
//            Boolean executed = false;
//            Func<Task<ResultPrimitive>> actionAsync = () =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            ResultPrimitive fault = ResultPrimitive.Fault;

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.6, () => true);
//            ResultPrimitive response = await policy.ExecuteAsync(actionAsync);
//            response.Should().Be(ResultPrimitive.Fault);
//            executed.Should().BeFalse();
//        }

//        [Fact]
//        public async Task Monkey_Context_Free_Should_Not_Return_Fault_async()
//        {
//            Boolean executed = false;
//            Func<Task<ResultPrimitive>> actionAsync = () =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            ResultPrimitive fault = ResultPrimitive.Fault;

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.4, () => true);
//            ResultPrimitive response = await policy.ExecuteAsync(actionAsync);
//            response.Should().Be(ResultPrimitive.Good);
//            executed.Should().BeTrue();
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Enabled_Should_Return_Fault_async()
//        {
//            Boolean executed = false;
//            Context context = new Context();
//            context["ShouldFail"] = true;
//            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            ResultPrimitive fault = ResultPrimitive.Fault;
//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult((bool)ctx["ShouldFail"]);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
//            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
//            response.Should().Be(ResultPrimitive.Fault);
//            executed.Should().BeFalse();
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Enabled_Should_Not_Return_Fault_async()
//        {
//            Boolean executed = false;
//            Context context = new Context();
//            context["ShouldFail"] = false;
//            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            ResultPrimitive fault = ResultPrimitive.Fault;
//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult((bool)ctx["ShouldFail"]);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
//            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
//            response.Should().Be(ResultPrimitive.Good);
//            executed.Should().BeTrue();
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Fault_Should_Return_Fault_async()
//        {
//            Boolean executed = false;
//            Context context = new Context();
//            context["ShouldFail"] = true;
//            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
//            {
//                if ((bool)ctx["ShouldFail"])
//                {
//                    return Task.FromResult(ResultPrimitive.Fault);
//                }

//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult((bool)ctx["ShouldFail"]);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
//            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
//            response.Should().Be(ResultPrimitive.Fault);
//            executed.Should().BeFalse();
//        }

//        [Fact]
//        public async Task Monkey_With_Context_Fault_Should_Not_Return_Fault_async()
//        {
//            Boolean executed = false;
//            Context context = new Context();
//            context["ShouldFail"] = false;
//            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
//            {
//                if ((bool)ctx["ShouldFail"])
//                {
//                    return Task.FromResult(ResultPrimitive.Fault);
//                }

//                return Task.FromResult(ResultPrimitive.GoodAgain);
//            };

//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult(true);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
//            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
//            response.Should().Be(ResultPrimitive.GoodAgain);
//            executed.Should().BeFalse();
//        }

//        [Fact]
//        public async Task Monkey_With_Context_InjectionRate_Should_Return_Fault_async()
//        {
//            Boolean executed = false;
//            Context context = new Context();
//            context["InjectionRate"] = 0.6;

//            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
//            {
//                return Task.FromResult(ResultPrimitive.Fault);
//            };

//            Func<Context, Task<Double>> injectionRate = (ctx) =>
//            {
//                double rate = 0;
//                if (ctx["InjectionRate"] != null)
//                {
//                    rate = (double)ctx["InjectionRate"];
//                }

//                return Task.FromResult(rate);
//            };

//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult(true);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);
//            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
//            response.Should().Be(ResultPrimitive.Fault);
//            executed.Should().BeFalse();
//        }

//        [Fact]
//        public async Task Monkey_With_Context_InjectionRate_Should_Not_Return_Fault_async()
//        {
//            Boolean executed = false;
//            Context context = new Context();
//            context["InjectionRate"] = 0.4;

//            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
//            {
//                executed = true;
//                return Task.FromResult(ResultPrimitive.Good);
//            };

//            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
//            {
//                return Task.FromResult(ResultPrimitive.Fault);
//            };

//            Func<Context, Task<Double>> injectionRate = (ctx) =>
//            {
//                double rate = 0;
//                if (ctx["InjectionRate"] != null)
//                {
//                    rate = (double)ctx["InjectionRate"];
//                }

//                return Task.FromResult(rate);
//            };

//            Func<Context, Task<bool>> enabled = (ctx) =>
//            {
//                return Task.FromResult(true);
//            };

//            var policy = MonkeyPolicy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);
//            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
//            response.Should().Be(ResultPrimitive.Good);
//            executed.Should().BeTrue();
//        }
//        #endregion
//    }
//}
