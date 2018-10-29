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
    public class InjectFaultSpecs : IDisposable
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
        public void InjectFault_Context_Free_Enabled_Should_not_execute_user_delegate()
        {
            this.Init();
            string exceptionMessage = "exceptionMessage";
            Exception fault = new Exception(exceptionMessage);
            MonkeyPolicy policy = Policy.InjectFault(fault, 0.6, () => true);

            Boolean executed = false;
            policy.Invoking(x => x.Execute(() => { executed = true; }))
                .ShouldThrowExactly<Exception>().WithMessage(exceptionMessage);

            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_Context_Free_Enabled_Should_execute_user_delegate()
        {
            this.Init();
            Exception fault = new Exception("test");
            MonkeyPolicy policy = Policy.InjectFault(fault, 0.3, () => true);

            Boolean executed = false;
            policy.Invoking(x => x.Execute(() => { executed = true; }))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }
        #endregion

        #region Basic Overload, Exception, With Context
        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            MonkeyPolicy policy = Policy.InjectFault(
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
        public void InjectFault_With_Context_Should_execute_user_delegate()
        {
            this.Init();
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";
            MonkeyPolicy policy = Policy.InjectFault(
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
        public void InjectFault_With_Context_Should_execute_user_delegate_2()
        {
            this.Init();
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "false";
            MonkeyPolicy policy = Policy.InjectFault(
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

        #region Overload, All based on context
        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate_1()
        {
            this.Init();
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = "true";

            Func<Context, Exception> fault = (ctx) => new Exception();
            Func<Context, double> injectionRate = (ctx) => 0.6;
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.InjectFault(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate_2()
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

            MonkeyPolicy policy = Policy.InjectFault(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<InvalidOperationException>().WithMessage(failureMessage);

            executed.Should().BeFalse();
        }
        #endregion
    }
}