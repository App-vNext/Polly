using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Polly.Monkey;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.RandomGeneratorDependentTestCollection)]
    public class MonkeySpecs : IDisposable
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
        public void Monkey_Context_Free_Enabled_Should_not_execute_user_delegate()
        {
            this.Init();
            string exceptionMessage = "exceptionMessage";
            Exception fault = new Exception(exceptionMessage);
            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, () => true);

            Boolean executed = false;
            policy.Invoking(x => x.Execute(() => { executed = true; }))
                .ShouldThrowExactly<Exception>().WithMessage(exceptionMessage);

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_Context_Free_Enabled_Should_execute_user_delegate()
        {
            this.Init();
            Exception fault = new Exception("test");
            MonkeyPolicy policy = Policy.Monkey(fault, 0.3, () => true);

            Boolean executed = false;
            policy.Invoking(x => x.Execute(() => { executed = true; }))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }
        #endregion

        #region Basic Overload, Exception, With Context
        [Fact]
        public void Monkey_With_Context_Should_not_execute_user_delegate()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            MonkeyPolicy policy = Policy.Monkey(
                new Exception("test"),
                0.6,
                (ctx) =>
                {
                    return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate()
        {
            this.Init();
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            MonkeyPolicy policy = Policy.Monkey(
                new Exception("test"),
                0.4,
                (ctx) =>
                {
                    return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate_2()
        {
            this.Init();
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            MonkeyPolicy policy = Policy.Monkey(
                new Exception("test"),
                0.6,
                (ctx) =>
                {
                    return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }
        #endregion

        #region Overload, Exception based on context
        [Fact]
        public void Monkey_With_Context_Exception_Should_not_execute_user_delegate_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, Exception> fault = (ctx) => new Exception("test");
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_not_execute_user_delegate_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, Exception> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return new InvalidOperationException();
                }

                return new InvalidProgramException("test");
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<InvalidOperationException>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_not_execute_user_delegate_3()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Func<Context, Exception> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return new InvalidOperationException();
                }

                return new InvalidProgramException();
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<InvalidProgramException>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_execute_user_delegate_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Func<Context, Exception> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return new InvalidOperationException();
                }

                return new InvalidProgramException("test");
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, 0.3, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldNotThrow<InvalidOperationException>();

            executed.Should().BeTrue();
        }
        #endregion

        #region Overload, Injection Rate based on context
        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_not_execute_user_delegate_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Exception fault = new Exception();
            Func<Context, double> injectionRate = (ctx) => 0.6;
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_not_execute_user_delegate_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return 0.6;
                }

                return 0.4;
            };

            Exception fault = new Exception();
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_execute_user_delegate_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return 0.6;
                }

                return 0.4;
            };

            Exception fault = new Exception();
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }
        #endregion

        #region Overload, All based on context
        [Fact]
        public void Monkey_With_Context_Should_not_execute_user_delegate_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, Exception> fault = (ctx) => new Exception();
            Func<Context, double> injectionRate = (ctx) => 0.6;
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Should_not_execute_user_delegate_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            string failureMessage = "Failure Message";
            context["ShouldFail"] = "true";
            context["Message"] = failureMessage;
            context["InjectionRate"] = "0.6";

            Func<Context, Exception> fault = (ctx) =>
            {
                if (ctx["Message"] != null)
                {
                    return new InvalidOperationException(ctx["Message"].ToString());
                }

                return new Exception();
            };

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return double.Parse(ctx["InjectionRate"].ToString());
                }

                return 0;
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
            };

            MonkeyPolicy policy = Policy.Monkey(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<InvalidOperationException>().WithMessage(failureMessage);

            executed.Should().BeFalse();
        }
        #endregion

        #region Action based Monkey policies
        [Fact]
        public void Monkey_Context_Free_Introduce_Delay_1()
        {
            this.Init();

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

            //// No delay
            sw.Reset();
            policy = Policy.Monkey(() => { }, 0.6, () => true);
            executed = false;

            sw.Restart();
            policy.Execute(() => { executed = true; });
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_Context_Free_Throw_Exception()
        {
            this.Init();
            Action fault = () => {
                throw new Exception();
            };

            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, () => true);
            Boolean executed = false;
            policy.Invoking(x => x.Execute(() => { executed = true; })).ShouldThrowExactly<Exception>();
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_1()
        {
            this.Init();
            Context context = new Context();
            context["ShouldFail"] = "true";

            Action<Context> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
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
            context["ShouldFail"] = "false";
            executed = false;
            sw.Restart();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_2()
        {
            this.Init();
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Enabled"] = "true";

            Action<Context> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    Thread.Sleep(200);
                }
            };

            Func<Context, bool> enabled = (ctx) => {
                return (ctx["Enabled"] != null && ctx["Enabled"].ToString() == "true");
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
            context["Enabled"] = "false";
            executed = false;
            sw.Restart();
            policy.Execute((ctx) => { executed = true; }, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_3()
        {
            this.Init();
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Enabled"] = "true";
            context["InjectionRate"] = "0.6";

            Action<Context> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    Thread.Sleep(200);
                }
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return (ctx["Enabled"] != null && ctx["Enabled"].ToString() == "true");
            };

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return double.Parse(ctx["InjectionRate"].ToString());
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
            context["InjectionRate"] = "0.4";
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
