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
            Boolean executed = false;
            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, () => true);
            policy.Invoking(x => x.Execute(action))
                .ShouldThrowExactly<Exception>().WithMessage(exceptionMessage);

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_Context_Free_Enabled_Should_execute_user_delegate()
        {
            this.Init();
            string exceptionMessage = "exceptionMessage";
            Exception fault = new Exception(exceptionMessage);
            Boolean executed = false;
            Func<ResultPrimitive> action = () => { executed = true; return ResultPrimitive.Good; };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.3, () => true);
            policy.Invoking(x => x.Execute(action))
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
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(
                new Exception(),
                0.6,
                (ctx) =>
                {
                    return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Invoking(x => x.Execute(action, context))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(
                new Exception(),
                0.4,
                (ctx) =>
                {
                    return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Invoking(x => x.Execute(action, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(
                new Exception(),
                0.6,
                (ctx) =>
                {
                    return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Invoking(x => x.Execute(action, context))
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
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Func<Context, Exception> fault = (ctx) => new Exception("test");
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);
            policy.Invoking(x => x.Execute(action, context))
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
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Func<Context, Exception> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return new InvalidOperationException();
                }

                return new InvalidProgramException("test");
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);
            policy.Invoking(x => x.Execute(action, context))
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
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Func<Context, Exception> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return new InvalidOperationException();
                }

                return new InvalidProgramException();
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);
            policy.Invoking(x => x.Execute(action, context))
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
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Func<Context, Exception> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return new InvalidOperationException();
                }

                return new InvalidProgramException();
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.3, enabled);
            policy.Invoking(x => x.Execute(action, context))
                .ShouldNotThrow<InvalidProgramException>();

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
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Exception fault = new Exception();
            Func<Context, double> injectionRate = (ctx) => 1;
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute(action, context))
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
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Exception fault = new Exception();
            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return 0.6;
                }

                return 0.4;
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute(action, context))
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
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Exception fault = new Exception();
            Func<Context, double> injectionRate = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
                {
                    return 0.6;
                }

                return 0.4;
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute(action, context))
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
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Func<Context, Exception> fault = (ctx) => new Exception();
            Func<Context, double> injectionRate = (ctx) => 0.6;
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute(action, context))
                .ShouldThrow<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Should_not_execute_user_delegate_2()
        {
            this.Init();
            string failureMessage = "Failure Message";
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Message"] = failureMessage;
            context["InjectionRate"] = "0.6";
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

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

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute(action, context))
                .ShouldThrowExactly<InvalidOperationException>();

            executed.Should().BeFalse();
        }
        #endregion

        #region Action based Monkey policies
        [Fact]
        public void Monkey_Context_Free_Introduce_Delay_1()
        {
            this.Init();
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

            //// No delay
            sw.Reset();
            policy = Policy.Monkey<ResultPrimitive>(() => { }, 0.6, () => true);
            executed = false;

            sw.Restart();
            policy.Execute(action);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_Context_Free_Throw_Exception()
        {
            this.Init();
            Func<ResultPrimitive> action = () => { return ResultPrimitive.Good; };
            Action fault = () => {
                throw new Exception();
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, () => true);
            policy.Invoking(x => x.Execute(action)).ShouldThrowExactly<Exception>();
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

            Action<Context> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
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
            context["ShouldFail"] = "false";
            executed = false;
            sw.Restart();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_2()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Enabled"] = "true";
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

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

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["Enabled"] = "false";
            executed = false;
            sw.Restart();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeLessThan(200);
        }

        [Fact]
        public void Monkey_With_Context_Introduce_Delay_3()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            context["Enabled"] = "true";
            context["InjectionRate"] = "0.6";
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };

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

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, injectionRate, enabled);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            policy.Execute(action, context);
            sw.Stop();

            executed.Should().BeTrue();
            sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(200);

            //// No delay
            context["InjectionRate"] = "0.4";
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
            this.Init();
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
            this.Init();
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
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            ResultPrimitive fault = ResultPrimitive.Fault;
            Func<Context, bool> enabled = (ctx) =>
            {
                return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = policy.Execute(action, context);
            response.Should().Be(ResultPrimitive.Fault);
            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Enabled_Should_Not_Return_Fault()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            ResultPrimitive fault = ResultPrimitive.Fault;
            Func<Context, bool> enabled = (ctx) =>
            {
                return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
            };

            MonkeyPolicy<ResultPrimitive> policy = Policy.Monkey<ResultPrimitive>(fault, 0.6, enabled);
            ResultPrimitive response = policy.Execute(action, context);
            response.Should().Be(ResultPrimitive.Good);
            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_With_Context_Fault_Should_Return_Fault()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            Func<Context, ResultPrimitive> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
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
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            Func<Context, ResultPrimitive> fault = (ctx) =>
            {
                if (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true")
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
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["InjectionRate"] = "0.6";

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            Func<Context, ResultPrimitive> fault = (ctx) =>
            {
                return ResultPrimitive.Fault;
            };

            Func<Context, Double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return double.Parse(ctx["InjectionRate"].ToString());
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
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["InjectionRate"] = "0.4";

            Func<Context, ResultPrimitive> action = (ctx) => { executed = true; return ResultPrimitive.Good; };
            Func<Context, ResultPrimitive> fault = (ctx) =>
            {
                return ResultPrimitive.Fault;
            };

            Func<Context, Double> injectionRate = (ctx) =>
            {
                if (ctx["InjectionRate"] != null)
                {
                    return double.Parse(ctx["InjectionRate"].ToString());
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
