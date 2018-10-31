using System;
using FluentAssertions;
using Polly.Monkey;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Monkey
{
    [Collection(Polly.Specs.Helpers.Constants.RandomGeneratorDependentTestCollection)]
    public class InjectFaultSpecs : IDisposable
    {
        public InjectFaultSpecs()
        {
            RandomGenerator.GetRandomNumber = () => 0.5;
        }

        public void Dispose()
        {
            RandomGenerator.Reset();
        }

        #region Basic Overload, Exception, Context Free
        [Fact]
        public void InjectFault_Context_Free_Enabled_Should_not_execute_user_delegate()
        {
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
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            MonkeyPolicy policy = Policy.InjectFault(
                new Exception("test"),
                0.6,
                (ctx) =>
                {
                    return ((bool)ctx["ShouldFail"]);
                });

            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_With_Context_Should_execute_user_delegate()
        {
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = true;
            MonkeyPolicy policy = Policy.InjectFault(
                new Exception("test"),
                0.4,
                (ctx) =>
                {
                    return ((bool)ctx["ShouldFail"]);
                });

            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }

        [Fact]
        public void InjectFault_With_Context_Should_execute_user_delegate_with_enabled_lambda_return_false()
        {
            Exception fault = new Exception("test");
            Boolean executed = false;
            Context context = new Context();
            context["ShouldFail"] = false;
            MonkeyPolicy policy = Policy.InjectFault(
                new Exception("test"),
                0.6,
                (ctx) =>
                {
                    return ((bool)ctx["ShouldFail"]);
                });

            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldNotThrow<Exception>();

            executed.Should().BeTrue();
        }
        #endregion

        #region Overload, All based on context
        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate_with_default_context()
        {
            Boolean executed = false;
            Context context = new Context();

            Func<Context, Exception> fault = (ctx) => new Exception();
            Func<Context, double> injectionRate = (ctx) => 0.6;
            Func<Context, bool> enabled = (ctx) => true;
            MonkeyPolicy policy = Policy.InjectFault(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<Exception>();

            executed.Should().BeFalse();
        }

        [Fact]
        public void InjectFault_With_Context_Should_not_execute_user_delegate_with_all_context_set()
        {
            Boolean executed = false;
            Context context = new Context();
            string failureMessage = "Failure Message";
            context["ShouldFail"] = true;
            context["Message"] = failureMessage;
            context["InjectionRate"] = 0.6;

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
                    return (double)ctx["InjectionRate"];
                }

                return 0;
            };

            Func<Context, bool> enabled = (ctx) =>
            {
                return ((bool)ctx["ShouldFail"]);
            };

            MonkeyPolicy policy = Policy.InjectFault(fault, injectionRate, enabled);
            policy.Invoking(x => x.Execute((ctx) => { executed = true; }, context))
                .ShouldThrowExactly<InvalidOperationException>().WithMessage(failureMessage);

            executed.Should().BeFalse();
        }
        #endregion
    }
}