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
    public class MonkeyTResultAsyncSpecs : IDisposable
    {
        public void Dispose()
        {
            RandomGenerator.Reset();
        }

        public void Init()
        {
            RandomGenerator.GetRandomNumber = () => 0.5;
        }

        #region Basic Overload, Exception, Context Free
        [Fact]
        public void Monkey_Context_Free_Enabled_Should_not_execute_user_delegate_async()
        {
            this.Init();
            string exceptionMessage = "exceptionMessage";
            Exception fault = new Exception(exceptionMessage);
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, () => true);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync))
                .ShouldThrowExactly<Exception>()
                .WithMessage(exceptionMessage);
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_Context_Free_Enabled_Should_execute_user_delegate_async()
        {
            this.Init();
            string exceptionMessage = "exceptionMessage";
            Exception fault = new Exception(exceptionMessage);
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () => { executed = true; return Task.FromResult(ResultPrimitive.Good); };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.3, () => true);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync))
                .ShouldNotThrow<Exception>();
            executed.Should().BeTrue();
        }
        #endregion

        #region Basic Overload, Exception, With Context
        [Fact]
        public void Monkey_With_Context_Should_not_execute_user_delegate_async()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true; return Task.FromResult(ResultPrimitive.Good);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(
                new Exception(),
                0.6,
                async (ctx) =>
                {
                    return await Task.FromResult(ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate_async()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true; return Task.FromResult(ResultPrimitive.Good);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(
                new Exception(),
                0.4,
                async (ctx) =>
                {
                    return await Task.FromResult(ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate_async_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true; return Task.FromResult(ResultPrimitive.Good);
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(
                new Exception(),
                0.4,
                async (ctx) =>
                {
                    return await Task.FromResult(ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }
        #endregion

        #region Overload, Exception based on context
        [Fact]
        public void Monkey_With_Context_Exception_Should_not_execute_user_delegate_async_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) => Task.FromResult(new Exception());
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_not_execute_user_delegate_async_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) =>
            {
                Exception ex;
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    ex = new InvalidOperationException();
                } 
                else
                {
                    ex = new InvalidProgramException();
                }

                return Task.FromResult(ex);
            };

            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<InvalidOperationException>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_not_execute_user_delegate_async_3()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) =>
            {
                Exception ex;
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    ex = new InvalidOperationException();
                }
                else
                {
                    ex = new InvalidProgramException();
                }

                return Task.FromResult(ex);
            };

            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<InvalidProgramException>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_execute_user_delegate_async_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) =>
            {
                Exception ex;
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    ex = new InvalidOperationException();
                }
                else
                {
                    ex = new InvalidProgramException();
                }

                return Task.FromResult(ex);
            };

            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.3, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldNotThrow<InvalidOperationException>();
            executed.Should().BeTrue();
        }
        #endregion

        #region Overload, Injection Rate based on context
        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_not_execute_user_delegate_async_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Exception fault = new Exception();
            Func<Context, Task<double>> injectionRate = (ctx) => Task.FromResult(0.6);
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_not_execute_user_delegate_async_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Exception fault = new Exception();
            Func<Context, Task<double>> injectionRate = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return Task.FromResult(0.6);
                }

                return Task.FromResult(0.4);
            };

            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_execute_user_delegate_async_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Exception fault = new Exception();
            Func<Context, Task<double>> injectionRate = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return Task.FromResult(0.6);
                }

                return Task.FromResult(0.4);
            };

            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldNotThrow<Exception>();
            executed.Should().BeTrue();
        }
        #endregion

        #region Overload, All based on context
        [Fact]
        public void Monkey_With_Context_Should_not_execute_user_delegate_async_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) => Task.FromResult(new Exception());
            Func<Context, Task<double>> injectionRate = (ctx) => Task.FromResult(0.6);
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Should_not_execute_user_delegate_async_2()
        {
            this.Init();
            string failureMessage = "Failure Message";
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Message"] = failureMessage;
            context["InjectionRate"] = "0.6";
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
                    rate = double.Parse(ctx["InjectionRate"].ToString());
                }

                return Task.FromResult(rate);
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult(ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<InvalidOperationException>();
            executed.Should().BeFalse();
        }
        #endregion

        #region Action based Monkey policies
        [Fact]
        public async Task Monkey_Context_Free_Introduce_Delay_async_1()
        {
            this.Init();
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

            //// No delay
            sw.Reset();
            policy = Policy.MonkeyAsync<ResultPrimitive>((cts) => Task.FromResult(ResultPrimitive.FaultAgain), 0.6, () => true);
            executed = false;

            sw.Restart();
            await policy.ExecuteAsync(actionAsync);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_Context_Free_Throw_Exception_async()
        {
            this.Init();
            Boolean executed = false;
            Func<Task<ResultPrimitive>> actionAsync = () =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<CancellationToken, Task> fault = async (cts) =>
            {
                throw new Exception();
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, () => true);
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Monkey_With_Context_Introduce_Delay_async_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
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
            context["ShouldFail"] = "false";
            executed = false;
            sw.Restart();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public async Task Monkey_With_Context_Introduce_Delay_async_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Enabled"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    await Task.Delay(200);
                }
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult(ctx["Enabled"] != null && ctx["Enabled"].ToString() == "true");
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["ShouldFail"] = "false";
            executed = false;
            sw.Restart();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public async Task Monkey_With_Context_Introduce_Delay_async_3()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Enabled"] = "true";
            context["InjectionRate"] = "0.6";

            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    await Task.Delay(200);
                }
            };

            Func<Context, Task<double>> injectionRate = (ctx) =>
            {
                double rate = 0;
                if (ctx["InjectionRate"] != null)
                {
                    rate = double.Parse(ctx["InjectionRate"].ToString());
                }

                return Task.FromResult(rate);
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult(ctx["Enabled"] != null && ctx["Enabled"].ToString() == "true");
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, injectionRate, enabled);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(actionAsync, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["ShouldFail"] = "false";
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
            this.Init();
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
            this.Init();
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
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            ResultPrimitive fault = ResultPrimitive.Fault;
            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult(ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Monkey_With_Context_Enabled_Should_Not_Return_Fault_async()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            ResultPrimitive fault = ResultPrimitive.Fault;
            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult(ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }

        [Fact]
        public async Task Monkey_With_Context_Fault_Should_Return_Fault_async()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return Task.FromResult(ResultPrimitive.Fault);
                }

                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, Task<bool>> enabled = (ctx) =>
            {
                return Task.FromResult(ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.MonkeyAsync<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = await policy.ExecuteAsync(actionAsync, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Monkey_With_Context_Fault_Should_Not_Return_Fault_async()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            Func<Context, Task<ResultPrimitive>> actionAsync = (ctx) =>
            {
                executed = true;
                return Task.FromResult(ResultPrimitive.Good);
            };

            Func<Context, CancellationToken, Task<ResultPrimitive>> fault = (ctx, cts) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
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
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["InjectionRate"] = "0.6";

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
                    rate = double.Parse(ctx["InjectionRate"].ToString());
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
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["InjectionRate"] = "0.4";

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
                    rate = double.Parse(ctx["InjectionRate"].ToString());
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
