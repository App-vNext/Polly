using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Monkey;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    public class MonkeyAsyncSpecs : IDisposable
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
            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.6, () => true);
            Boolean executed = false;
            Func<Task> actionAsync = () => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync)).ShouldThrowExactly<Exception>().WithMessage(exceptionMessage);
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_Context_Free_Enabled_Should_execute_user_delegate_async()
        {
            this.Init();
            Exception fault = new Exception();
            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.3, () => true);
            Boolean executed = false;
            Func<Task> actionAsync = () => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync)).ShouldNotThrow<Exception>();
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
            MonkeyPolicy policy = Policy.MonkeyAsync(
                new Exception("test"),
                0.6,
                async (ctx) =>
                {
                    return await Task.FromResult((ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true"));
                });

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate_async()
        {
            this.Init();
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            MonkeyPolicy policy = Policy.MonkeyAsync(
                new Exception("test"),
                0.4,
                async (ctx) =>
                {
                    return await Task.FromResult((ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true"));
                });

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldNotThrow<Exception>();
            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate_async_2()
        {
            this.Init();
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            MonkeyPolicy policy = Policy.MonkeyAsync(
                new Exception("test"),
                0.6,
                async (ctx) =>
                {
                    return await Task.FromResult((ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true"));
                });

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldNotThrow<Exception>();
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

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) => Task.FromResult(new Exception("test"));
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.6, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
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

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) =>
            {
                Exception ex;
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    ex = new InvalidOperationException();
                }
                else
                {
                    ex = new InvalidProgramException("test");
                }

                return Task.FromResult(ex);
            };

            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.6, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<InvalidOperationException>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_not_execute_user_delegate_async_3()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) =>
            {
                Exception ex;
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    ex = new InvalidOperationException();
                }
                else
                {
                    ex = new InvalidProgramException("test");
                }

                return Task.FromResult(ex);
            };

            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.6, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<InvalidProgramException>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_execute_user_delegate_async_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) =>
            {
                Exception ex;
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    ex = new InvalidOperationException();
                }
                else
                {
                    ex = new InvalidProgramException("test");
                }

                return Task.FromResult(ex);
            };

            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.3, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldNotThrow<InvalidProgramException>();
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

            Exception fault = new Exception();
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            Func<Context, Task<Double>> injectionRate = (ctx) => Task.FromResult(0.6);
            MonkeyPolicy policy = Policy.MonkeyAsync(fault, injectionRate, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_not_execute_user_delegate_async_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Exception fault = new Exception();
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            Func<Context, Task<Double>> injectionRate = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return Task.FromResult(0.6);
                }

                return Task.FromResult(0.4);
            };

            MonkeyPolicy policy = Policy.MonkeyAsync(fault, injectionRate, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_execute_user_delegate_async_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Exception fault = new Exception();
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            Func<Context, Task<Double>> injectionRate = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return Task.FromResult(0.6);
                }

                return Task.FromResult(0.4);
            };

            MonkeyPolicy policy = Policy.MonkeyAsync(fault, injectionRate, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldNotThrow<Exception>();
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

            Func<Context, CancellationToken, Task<Exception>> fault = (ctx, cts) => Task.FromResult(new Exception());
            Func<Context, Task<bool>> enabled = (ctx) => Task.FromResult(true);
            Func<Context, Task<Double>> injectionRate = (ctx) => Task.FromResult(0.6);
            MonkeyPolicy policy = Policy.MonkeyAsync(fault, injectionRate, enabled);

            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context)).ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Should_not_execute_user_delegate_async_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            string failureMessage = "Failure Message";
            context["ShouldFail"] = "true";
            context["Message"] = failureMessage;
            context["InjectionRate"] = "0.6";

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

            Func <Context, Task<bool>> enabled  = (ctx) =>
            {
                return Task.FromResult(ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
            };

            MonkeyPolicy policy = Policy.MonkeyAsync(fault, injectionRate, enabled);
            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, context))
                .ShouldThrowExactly<InvalidOperationException>()
                .WithMessage(failureMessage);
            executed.Should().BeFalse();
        }
        #endregion

        #region Action based Monkey policies
        [Fact]
        public async Task Monkey_Context_Free_Introduce_Delay_async_1()
        {
            this.Init();
            Func<CancellationToken, Task> fault = async (cts) =>
            {
                await Task.Delay(200);
            };

            Func<Context, Task<bool>> enabled = (cts) => Task.FromResult(true);

            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.6, enabled);
            Boolean executed = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(() => { executed = true; return TaskHelper.EmptyTask; });
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            sw.Reset();
            policy = Policy.MonkeyAsync((cts) => TaskHelper.EmptyTask, 0.6, enabled);
            executed = false;
            sw.Restart();
            await policy.ExecuteAsync(() => { executed = true; return TaskHelper.EmptyTask; });
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_Context_Free_Throw_Exception_async()
        {
            this.Init();
            Boolean executed = false;
            Func<CancellationToken, Task> fault = async (cts) =>
            {
                await Task.Run(() => { });
                throw new Exception();
            };

            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.6, (cts) => Task.FromResult(true));
            Func<Context, Task> actionAsync = (_) => { executed = true; return TaskHelper.EmptyTask; };
            policy.Awaiting(async x => await x.ExecuteAsync(actionAsync, new Context()))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public async Task Monkey_With_Context_Introduce_Delay_async_1()
        {
            this.Init();
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, CancellationToken, Task> fault = async (ctx, cts) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    await Task.Delay(200);
                }
            };

            Func<Context, Task<bool>> enabled = (cts) => Task.FromResult(true);

            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.6, enabled);
            Boolean executed = false;
            Func<Context, Task> action = (cts) => { executed = true; return TaskHelper.EmptyTask; };

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["ShouldFail"] = "false";
            executed = false;
            sw.Restart();
            await policy.ExecuteAsync(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public async Task Monkey_With_Context_Introduce_Delay_async_2()
        {
            this.Init();
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Enabled"] = "true";

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

            MonkeyPolicy policy = Policy.MonkeyAsync(fault, 0.6, enabled);
            Boolean executed = false;
            Func<Context, Task> action = (cts) => { executed = true; return TaskHelper.EmptyTask; };

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["Enabled"] = "false";
            executed = false;
            sw.Restart();
            await policy.ExecuteAsync(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public async Task Monkey_With_Context_Introduce_Delay_async_3()
        {
            this.Init();
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Enabled"] = "true";
            context["InjectionRate"] = "0.6";

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

            Func<Context, Task<double>> injectionRate = (ctx) =>
            {
                double rate = 0;
                if (ctx["InjectionRate"] != null)
                {
                    rate = double.Parse(ctx["InjectionRate"].ToString());
                }

                return Task.FromResult(rate);
            };

            MonkeyPolicy policy = Policy.MonkeyAsync(fault, injectionRate, enabled);
            Boolean executed = false;
            Func<Context, Task> action = (cts) => { executed = true; return TaskHelper.EmptyTask; };

            Stopwatch sw = new Stopwatch();
            sw.Start();
            await policy.ExecuteAsync(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["InjectionRate"] = "0.4";
            executed = false;
            sw.Restart();
            await policy.ExecuteAsync(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }
        #endregion
    }
}
