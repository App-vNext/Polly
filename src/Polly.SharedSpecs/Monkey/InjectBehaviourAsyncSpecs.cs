using System;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Monkey;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
    public class InjectBehaviourAsyncSpecs : IDisposable
    {
        public InjectBehaviourAsyncSpecs()
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

            var policy = MonkeyPolicy.InjectBehaviourAsync(() => { injectedBehaviourExecuted = true; return Task.CompletedTask; }, 0.6, () => Task.FromResult(false));

            policy.ExecuteAsync(() => { userDelegateExecuted = true; return Task.CompletedTask; });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeFalse();
        }

        [Fact]
        public void Given_enabled_and_randomly_within_threshold_should_inject_behaviour()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviourAsync(() => { injectedBehaviourExecuted = true; return Task.CompletedTask; }, 0.6, () => Task.FromResult(true));

            policy.ExecuteAsync(() => { userDelegateExecuted = true; return Task.CompletedTask; });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeTrue();
        }

        [Fact]
        public void Given_enabled_and_randomly_not_within_threshold_should_not_inject_behaviour()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviourAsync(() => { injectedBehaviourExecuted = true; return Task.CompletedTask; }, 0.4, () => Task.FromResult(true));

            policy.ExecuteAsync(() => { userDelegateExecuted = true; return Task.CompletedTask; });

            userDelegateExecuted.Should().BeTrue();
            injectedBehaviourExecuted.Should().BeFalse();
        }

        [Fact]
        public void Should_inject_behaviour_before_executing_user_delegate()
        {
            Boolean userDelegateExecuted = false;
            Boolean injectedBehaviourExecuted = false;

            var policy = MonkeyPolicy.InjectBehaviourAsync(() =>
            {
                userDelegateExecuted.Should().BeFalse(); // Not yet executed at the time the injected behaviour runs.
                injectedBehaviourExecuted = true;
                return Task.CompletedTask;
            }, 0.6, () => Task.FromResult(true));

            policy.ExecuteAsync(() => { userDelegateExecuted = true; return Task.CompletedTask; });

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
