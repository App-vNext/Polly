using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FluentAssertions;
using Polly.Monkey;
using Polly.NoOp;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;

namespace Polly.Specs.Monkey
{
    public class MonkeySpecs : IDisposable
    {
        public MonkeySpecs()
        {
            RandomGenerator.GetRandomNumber = () => 0.5;
        }

        public void Dispose()
        {
            Random rand = new Random();
            RandomGenerator.GetRandomNumber = () => rand.NextDouble();
        }

        #region Basic Overload, Exception, Context Free
        [Fact]
        public void Monkey_Context_Free_Enabled_Should_not_execute_user_delegate()
        {
            Exception fault = new Exception("test");
            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, () => true);

            Boolean executed = false;
            policy.Invoking(x => x.Execute(() => { executed = true; }))
                .ShouldThrow<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_Context_Free_Enabled_Should_execute_user_delegate()
        {
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
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            MonkeyPolicy policy = Policy.Monkey(
                new Exception("test"),
                0.6,
                (ctx) => {
                    return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrow<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate()
        {
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            MonkeyPolicy policy = Policy.Monkey(
                new Exception("test"),
                0.4,
                (ctx) => {
                    return (ctx["ShouldFail"] != null && ctx["ShouldFail"].ToString() == "true");
                });

            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }

        [Fact]
        public void Monkey_With_Context_Should_execute_user_delegate_2()
        {
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            MonkeyPolicy policy = Policy.Monkey(
                new Exception("test"),
                0.6,
                (ctx) => {
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
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, Exception> fault = (ctx) => new Exception("test");
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrow<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_not_execute_user_delegate_2()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, Exception> fault = (ctx) =>
            {
                if (context["ShouldFail"] != null && context["ShouldFail"].ToString() == "true")
                {
                    return new InvalidOperationException();
                }

                return new InvalidProgramException("test");
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrow<InvalidOperationException>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_not_execute_user_delegate_3()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Func<Context, Exception> fault = (ctx) =>
            {
                if (context["ShouldFail"] != null && context["ShouldFail"].ToString() == "true")
                {
                    return new InvalidOperationException();
                }

                return new InvalidProgramException();
            };

            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, 0.6, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrow<InvalidProgramException>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_Exception_Should_execute_user_delegate_1()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Func<Context, Exception> fault = (ctx) =>
            {
                if (context["ShouldFail"] != null && context["ShouldFail"].ToString() == "true")
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
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Exception fault = new Exception();
            Func<Context, double> injectionRate = (ctx) => 0.6;
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrow<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_not_execute_user_delegate_2()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (context["ShouldFail"] != null && context["ShouldFail"].ToString() == "true")
                {
                    return 0.6;
                }

                return 0.4;
            };

            Exception fault = new Exception();
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.Monkey(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrow<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void Monkey_With_Context_InjectionRate_Should_execute_user_delegate_1()
        {
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";

            Func<Context, double> injectionRate = (ctx) =>
            {
                if (context["ShouldFail"] != null && context["ShouldFail"].ToString() == "true")
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
    }
}
