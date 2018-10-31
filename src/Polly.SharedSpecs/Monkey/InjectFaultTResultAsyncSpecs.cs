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
    public class InjectFaultTResultAsyncSpecs : IDisposable
    {
        public InjectFaultTResultAsyncSpecs()
        {
            RandomGenerator.GetRandomNumber = () => 0.5;
        }

        public void Dispose()
        {
            RandomGenerator.Reset();
        }

        #region Basic Overload, Exception, Context Free
        [Fact]
        public void InjectFault_Context_Free_Enabled_Should_not_execute_user_delegate_async()
        {
            string exceptionMessage = "exceptionMessage";
            Exception fault = new Exception(exceptionMessage);
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, 0.6, () => true);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync))
                .ShouldThrowExactly<Exception>()
                .WithMessage(exceptionMessage);
            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_Context_Free_Enabled_Should_execute_user_delegate_async()
        {
            string exceptionMessage = "exceptionMessage";
            Exception fault = new Exception(exceptionMessage);
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, 0.3, () => true);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync))
                .ShouldNotThrow<Exception>();
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
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true; return Task.FromResult(ResultPrimitive.Good);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(
                new Exception(),
                0.6,
                async (ctx) =>
                {
                    return await Task.FromResult((bool)ctx["ShouldFail"]);
                });

            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_With_Context_Should_execute_user_delegate_async()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true; return Task.FromResult(ResultPrimitive.Good);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(
                new Exception(),
                0.4,
                async (ctx) =>
                {
                    return await Task.FromResult((bool)ctx["ShouldFail"]);
                });

            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }

        [Fact]
        public void InjectFault_With_Context_Should_execute_user_delegate_async_with_enabled_lambda_return_false()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = false;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true; return Task.FromResult(ResultPrimitive.Good);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(
                new Exception(),
                0.4,
                async (ctx) =>
                {
                    return await Task.FromResult((bool)ctx["ShouldFail"]);
                });

            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }
        #endregion

        #region Overload, All based on context
        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate_async_with_default_values()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) => Task.FromResult(new Exception());
            Func<Context, Task<double>> injectionRate = (ctx) => Task.FromResult(0.6);
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate_async_with_all_values_set()
        {
            string failureMessage = "Failure Message";
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            context["Message"] = failureMessage;
            context["InjectionRate"] = 0.6;
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

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

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<InvalidOperationException>();
            executed.Should().BeFalse();
        }
        #endregion

        #region TResult Based Monkey Policies
        [Fact]
        public async Task InjectFault_Context_Free_Should_Return_Fault_async()
        {
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            ResultPrimitive fault = ResultPrimitive.Fault;

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, 0.6, () => true);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task InjectFault_Context_Free_Should_Not_Return_Fault_async()
        {
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            ResultPrimitive fault = ResultPrimitive.Fault;

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, 0.4, () => true);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }

        [Fact]
        public async Task InjectFault_With_Context_Enabled_Should_Return_Fault_async()
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

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task InjectFault_With_Context_Enabled_Should_Not_Return_Fault_async()
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

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }

        [Fact]
        public async Task InjectFault_With_Context_InjectionRate_Should_Return_Fault_async()
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

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, injectionRate, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task InjectFault_With_Context_InjectionRate_Should_Not_Return_Fault_async()
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

            MonkeyPolicy<ResultPrimitive> policy = Policy.InjectFaultAsync<ResultPrimitive>(fault, injectionRate, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }
        #endregion
    }
}