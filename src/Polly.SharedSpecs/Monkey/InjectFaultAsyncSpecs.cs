using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Monkey;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.AmbientContextDependentTestCollection)]
    public class InjectFaultAsyncSpecs : IDisposable
    {
        public InjectFaultAsyncSpecs()
        {
            ThreadSafeRandom_LockOncePerThread.NextDouble = () => 0.5;
        }

        public void Dispose()
        {
            ThreadSafeRandom_LockOncePerThread.Reset();
        }

        #region Basic Overload, Exception, Context Free
        [Fact]
        public void InjectFault_Context_Free_Enabled_Should_not_execute_user_delegate_async()
        {
            string exceptionMessage = "exceptionMessage";
            Exception fault = new Exception(exceptionMessage);
            var policy = MonkeyPolicy.InjectFaultAsync(fault, 0.6, () => true);
            Boolean executed = false;
            Func<Task> actionAsync = () => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync)).ShouldThrowExactly<Exception>().WithMessage(exceptionMessage);
            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_Context_Free_Enabled_Should_execute_user_delegate_async()
        {
            Exception fault = new Exception();
            var policy = MonkeyPolicy.InjectFaultAsync(fault, 0.3, () => true);
            Boolean executed = false;
            Func<Task> actionAsync = () => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync)).ShouldNotThrow<Exception>();
            executed.Should().BeTrue();
        }

        [Fact]
        public void InjectFault_Context_Free_Enabled_Should_execute_user_delegate_not_throw_if_injected_fault_is_permitted_null()
        {
            Exception fault = null;
            var policy = MonkeyPolicy.InjectFaultAsync(fault, 0.3, () => true);

            Boolean executed = false;
            Func<Task> actionAsync = () => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync)).ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }
        #endregion

        #region Basic Overload, Exception, With Context
        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            var policy = MonkeyPolicy.InjectFaultAsync(
                new Exception("test"),
                0.6,
                async (ctx) =>
                {
                    return await Task.FromResult((bool)ctx["ShouldFail"]);
                });

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_With_Context_Should_execute_user_delegate_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            var policy = MonkeyPolicy.InjectFaultAsync(
                new Exception("test"),
                0.4,
                async (ctx) =>
                {
                    return await Task.FromResult((bool)ctx["ShouldFail"]);
                });

            Func<Context, Task> actionAsync = _ => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldNotThrow<Exception>();
            executed.Should().BeTrue();
        }

        [Fact]
        public void InjectFault_With_Context_Should_execute_user_delegate_async_with_enabled_lambda_false()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = false;
            var policy = MonkeyPolicy.InjectFaultAsync(
                new Exception("test"),
                0.6,
                async (ctx) =>
                {
                    return await Task.FromResult((bool)ctx["ShouldFail"]);
                });

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldNotThrow<Exception>();
            executed.Should().BeTrue();
        }
        #endregion

        #region Overload, All based on context

        [Fact]
        public void InjectFault_should_throw_if_injection_rate_is_out_of_range_too_low()
        {
            Boolean executed = false;
            Context context = new Context();

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) => Task.FromResult(new Exception());
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            Func<Context, Task<Double>> injectionRate = (ctx) => Task.FromResult(-0.1);
            var policy = MonkeyPolicy.InjectFaultAsync(fault, injectionRate, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<ArgumentOutOfRangeException>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_should_throw_if_injection_rate_is_out_of_range_too_high()
        {
            Boolean executed = false;
            Context context = new Context();

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) => Task.FromResult(new Exception());
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            Func<Context, Task<Double>> injectionRate = (ctx) => Task.FromResult(1.01);
            var policy = MonkeyPolicy.InjectFaultAsync(fault, injectionRate, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<ArgumentOutOfRangeException>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate_async_basic()
        {
            Boolean executed = false;
            Context context = new Context();

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) => Task.FromResult(new Exception());
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            Func<Context, Task<Double>> injectionRate = (ctx) => Task.FromResult(0.6);
            var policy = MonkeyPolicy.InjectFaultAsync(fault, injectionRate, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate_async_with_all_context_set()
        {
            Boolean executed = false;
            Context context = new Context();
            string failureMessage = "Failure Message";
            context["ShouldFail"] = true;
            context["Message"] = failureMessage;
            context["InjectionRate"] = 0.6;

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) =>
            {
                if (ctx["Message"] != null)
                {
                    Exception ex = new InvalidOperationException(ctx["Message"].ToString());
                    return Task.FromResult(ex);
                }

                return Task.FromResult(new Exception());
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
                return Task.FromResult((bool)ctx["ShouldFail"]);
            };

            var policy = MonkeyPolicy.InjectFaultAsync(fault, injectionRate, enabled);
            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<InvalidOperationException>()
                .WithMessage(failureMessage);
            executed.Should().BeFalse();
        }
        #endregion
    }
}
