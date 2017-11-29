using System;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Polly.Wrap;
using Xunit;

namespace Polly.Specs.Wrap
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class PolicyWrapSpecsAsync
    {
        #region Instance configuration syntax tests, non-generic outer

        [Fact]
        public void Nongeneric_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
        {
            RetryPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

            Action config = () => retry.WrapAsync((Policy)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Nongeneric_wraps_generic_instance_syntax_wrapping_null_should_throw()
        {
            RetryPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

            Action config = () => retry.WrapAsync<int>((Policy<int>)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Nongeneric_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
        {
            Policy policyA = Policy.NoOpAsync();
            Policy policyB = Policy.NoOpAsync();

            PolicyWrap wrap = policyA.WrapAsync(policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        [Fact]
        public void Nongeneric_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
        {
            Policy policyA = Policy.NoOpAsync();
            Policy<int> policyB = Policy.NoOpAsync<int>();

            PolicyWrap<int> wrap = policyA.WrapAsync(policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        #endregion

        #region   Instance configuration syntax tests, generic outer

        [Fact]
        public void Generic_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
        {
            RetryPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

            Action config = () => retry.WrapAsync((Policy)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Generic_wraps_generic_instance_syntax_wrapping_null_should_throw()
        {
            RetryPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

            Action config = () => retry.WrapAsync((Policy<int>)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Generic_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
        {
            Policy<int> policyA = Policy.NoOpAsync<int>();
            Policy policyB = Policy.NoOpAsync();

            PolicyWrap<int> wrap = policyA.WrapAsync(policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        [Fact]
        public void Generic_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
        {
            Policy<int> policyA = Policy.NoOpAsync<int>();
            Policy<int> policyB = Policy.NoOpAsync<int>();

            PolicyWrap<int> wrap = policyA.WrapAsync(policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        #endregion

        #region Interface extension configuration syntax tests, non-generic outer

        [Fact]
        public void Nongeneric_interface_wraps_nongeneric_instance_syntax_null_wrapping_should_throw()
        {
            IAsyncPolicy outerNull = null;
            IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

            Action config = () => outerNull.WrapAsync(retry);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
        }

        [Fact]
        public void Nongeneric_interface_wraps_generic_instance_syntax_null_wrapping_should_throw()
        {
            IAsyncPolicy outerNull = null;
            IAsyncPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

            Action config = () => outerNull.WrapAsync<int>(retry);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
        }

        [Fact]
        public void Nongeneric_interface_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
        {
            IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

            Action config = () => retry.WrapAsync((Policy)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Nongeneric_interface_wraps_generic_instance_syntax_wrapping_null_should_throw()
        {
            IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

            Action config = () => retry.WrapAsync<int>((Policy<int>)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Nongeneric_interface_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
        {
            IAsyncPolicy policyA = Policy.NoOpAsync();
            IAsyncPolicy policyB = Policy.NoOpAsync();

            IPolicyWrap wrap = policyA.WrapAsync(policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        [Fact]
        public void Nongeneric_interface_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
        {
            IAsyncPolicy policyA = Policy.NoOpAsync();
            IAsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

            IPolicyWrap<int> wrap = policyA.WrapAsync(policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        #endregion

        #region Interface extension configuration syntax tests, generic outer

        [Fact]
        public void Generic_interface_wraps_nongeneric_instance_syntax_null_wrapping_should_throw()
        {
            IAsyncPolicy<int> outerNull = null;
            IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

            Action config = () => outerNull.WrapAsync(retry);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
        }

        [Fact]
        public void Generic_interface_wraps_generic_instance_syntax_null_wrapping_should_throw()
        {
            IAsyncPolicy<int> outerNull = null;
            IAsyncPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

            Action config = () => outerNull.WrapAsync<int>(retry);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
        }

        [Fact]
        public void Generic_interface_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
        {
            IAsyncPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

            Action config = () => retry.WrapAsync((Policy)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Generic_interface_wraps_generic_instance_syntax_wrapping_null_should_throw()
        {
            IAsyncPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

            Action config = () => retry.WrapAsync((Policy<int>)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Generic_interface_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
        {
            IAsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
            IAsyncPolicy policyB = Policy.NoOpAsync();

            IPolicyWrap<int> wrap = policyA.WrapAsync(policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        [Fact]
        public void Generic_interface_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
        {
            IAsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
            IAsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

            IPolicyWrap<int> wrap = policyA.WrapAsync(policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        #endregion

        #region Static configuration syntax tests, non-generic policies

        [Fact]
        public void Wrapping_nothing_using_static_wrap_syntax_should_throw()
        {
            Action config = () => Policy.WrapAsync();

            config.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policies");
        }

        [Fact]
        public void Wrapping_only_one_policy_using_static_wrap_syntax_should_throw()
        {
            Policy singlePolicy = Policy.Handle<Exception>().RetryAsync();
            Action config = () => Policy.WrapAsync(new[] { singlePolicy });

            config.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policies");
        }

        [Fact]
        public void Wrapping_two_policies_using_static_wrap_syntax_should_not_throw()
        {
            Policy retry = Policy.Handle<Exception>().RetryAsync();
            Policy breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));
            Action config = () => Policy.WrapAsync(new[] { retry, breaker });

            config.ShouldNotThrow();
        }

        [Fact]
        public void Wrapping_more_than_two_policies_using_static_wrap_syntax_should_not_throw()
        {
            Policy retry = Policy.Handle<Exception>().RetryAsync(1);
            Policy divideByZeroRetry = Policy.Handle<DivideByZeroException>().RetryAsync(2);
            Policy breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));

            Action config = () => Policy.WrapAsync(new[] { divideByZeroRetry, retry, breaker });

            config.ShouldNotThrow();
        }

        [Fact]
        public void Wrapping_policies_using_static_wrap_syntax_should_set_outer_inner()
        {
            Policy policyA = Policy.NoOpAsync();
            Policy policyB = Policy.NoOpAsync();

            PolicyWrap wrap = Policy.WrapAsync(policyA, policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        #endregion

        #region Static configuration syntax tests, strongly-typed policies

        [Fact]
        public void Wrapping_nothing_using_static_wrap_strongly_typed_syntax_should_throw()
        {
            Action config = () => Policy.WrapAsync<int>();

            config.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policies");
        }

        [Fact]
        public void Wrapping_only_one_policy_using_static_wrap_strongly_typed_syntax_should_throw()
        {
            Policy<int> singlePolicy = Policy<int>.Handle<Exception>().RetryAsync();
            Action config = () => Policy.WrapAsync<int>(new[] { singlePolicy });

            config.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policies");
        }

        [Fact]
        public void Wrapping_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
        {
            Policy<int> retry = Policy<int>.Handle<Exception>().RetryAsync();
            Policy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));
            Action config = () => Policy.WrapAsync<int>(new[] { retry, breaker });

            config.ShouldNotThrow();
        }

        [Fact]
        public void Wrapping_more_than_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
        {
            Policy<int> retry = Policy<int>.Handle<Exception>().RetryAsync();
            Policy<int> divideByZeroRetry = Policy<int>.Handle<DivideByZeroException>().RetryAsync(2);
            Policy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));

            Action config = () => Policy.WrapAsync<int>(new[] { divideByZeroRetry, retry, breaker });

            config.ShouldNotThrow();
        }

        [Fact]
        public void Wrapping_policies_using_static_wrap_strongly_typed_syntax_should_set_outer_inner()
        {
            Policy<int> policyA = Policy.NoOpAsync<int>();
            Policy<int> policyB = Policy.NoOpAsync<int>();

            PolicyWrap<int> wrap = Policy.WrapAsync(policyA, policyB);

            wrap.Outer.Should().BeSameAs(policyA);
            wrap.Inner.Should().BeSameAs(policyB);
        }

        #endregion

        #region Instance-configured: execution tests

        [Fact]
        public void Wrapping_two_policies_by_instance_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
        {
            RetryPolicy retry = Policy.Handle<Exception>().RetryAsync(1); // Two tries in total: first try, plus one retry.
            CircuitBreakerPolicy breaker = Policy.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.MaxValue);

            PolicyWrap retryWrappingBreaker = retry.WrapAsync(breaker);
            PolicyWrap breakerWrappingRetry = breaker.WrapAsync(retry);

            // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
            breaker.Reset();
            retryWrappingBreaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>(2))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
            breaker.Reset();
            breakerWrappingRetry.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>(2))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public async Task Wrapping_two_generic_policies_by_instance_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
        {
            RetryPolicy<ResultPrimitive> retry = Policy.HandleResult(ResultPrimitive.Fault).RetryAsync(1); // Two tries in total: first try, plus one retry.
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreakerAsync(2, TimeSpan.MaxValue);

            var retryWrappingBreaker = retry.WrapAsync(breaker);
            var breakerWrappingRetry = breaker.WrapAsync(retry);

            // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
            breaker.Reset();
            (await retryWrappingBreaker.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
            breaker.Reset();
            (await breakerWrappingRetry.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Static-configured: execution tests

        [Fact]
        public void Wrapping_two_policies_by_static_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
        {
            RetryPolicy retry = Policy.Handle<Exception>().RetryAsync(1); // Two tries in total: first try, plus one retry.
            CircuitBreakerPolicy breaker = Policy.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.MaxValue);

            PolicyWrap retryWrappingBreaker = Policy.WrapAsync(retry, breaker);
            PolicyWrap breakerWrappingRetry = Policy.WrapAsync(breaker, retry);

            // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
            breaker.Reset();
            retryWrappingBreaker.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>(2))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
            breaker.Reset();
            breakerWrappingRetry.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>(2))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public async Task Wrapping_two_generic_policies_by_static_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
        {
            RetryPolicy<ResultPrimitive> retry = Policy.HandleResult(ResultPrimitive.Fault).RetryAsync(1); // Two tries in total: first try, plus one retry.
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreakerAsync(2, TimeSpan.MaxValue);

            var retryWrappingBreaker = Policy.WrapAsync(retry, breaker);
            var breakerWrappingRetry = Policy.WrapAsync(breaker, retry);

            // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
            breaker.Reset();
            (await retryWrappingBreaker.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
            breaker.Reset();
            (await breakerWrappingRetry.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault).ConfigureAwait(false))
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region ExecuteAndCaptureAsyncSpecs

        [Fact]
        public async Task Outermost_policy_handling_exception_should_report_as_PolicyWrap_handled_exception()
        {
            CircuitBreakerPolicy innerHandlingDBZE = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            CircuitBreakerPolicy outerHandlingANE = Policy
                .Handle<ArgumentNullException>()
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            PolicyWrap wrap = outerHandlingANE.WrapAsync(innerHandlingDBZE);

            PolicyResult executeAndCaptureResultOnPolicyWrap =
                await wrap.ExecuteAndCaptureAsync(() => { throw new ArgumentNullException(); });

            executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
            executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<ArgumentNullException>();
            executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.HandledByThisPolicy);
        }

        [Fact]
        public async Task Outermost_policy_not_handling_exception_even_if_inner_policies_do_should_report_as_unhandled_exception()
        {
            CircuitBreakerPolicy innerHandlingDBZE = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            CircuitBreakerPolicy outerHandlingANE = Policy
                .Handle<ArgumentNullException>()
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            PolicyWrap wrap = outerHandlingANE.WrapAsync(innerHandlingDBZE);

            PolicyResult executeAndCaptureResultOnPolicyWrap =
                await wrap.ExecuteAndCaptureAsync(() => { throw new DivideByZeroException(); });

            executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
            executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<DivideByZeroException>();
            executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.Unhandled);
        }

        [Fact]
        public async Task Outermost_generic_policy_handling_exception_should_report_as_PolicyWrap_handled_exception()
        {
            CircuitBreakerPolicy<ResultPrimitive> innerHandlingDBZE = Policy<ResultPrimitive>
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            CircuitBreakerPolicy<ResultPrimitive> outerHandlingANE = Policy<ResultPrimitive>
                .Handle<ArgumentNullException>()
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            PolicyWrap<ResultPrimitive> wrap = outerHandlingANE.WrapAsync(innerHandlingDBZE);

            PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = await  wrap.ExecuteAndCaptureAsync(() => { throw new ArgumentNullException(); });

            executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
            executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<ArgumentNullException>();
            executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.HandledByThisPolicy);
            executeAndCaptureResultOnPolicyWrap.FaultType.Should().Be(FaultType.ExceptionHandledByThisPolicy);
        }

        [Fact]
        public async Task Outermost_generic_policy_not_handling_exception_even_if_inner_policies_do_should_report_as_unhandled_exception()
        {
            CircuitBreakerPolicy<ResultPrimitive> innerHandlingDBZE = Policy<ResultPrimitive>
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            CircuitBreakerPolicy<ResultPrimitive> outerHandlingANE = Policy<ResultPrimitive>
                .Handle<ArgumentNullException>()
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            PolicyWrap<ResultPrimitive> wrap = outerHandlingANE.WrapAsync(innerHandlingDBZE);

            PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = await wrap.ExecuteAndCaptureAsync(() => { throw new DivideByZeroException(); });

            executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
            executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<DivideByZeroException>();
            executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.Unhandled);
            executeAndCaptureResultOnPolicyWrap.FaultType.Should().Be(FaultType.UnhandledException);
        }

        [Fact]
        public async Task Outermost_generic_policy_handling_result_should_report_as_PolicyWrap_handled_result()
        {
            CircuitBreakerPolicy<ResultPrimitive> innerHandlingFaultAgain = Policy
                .HandleResult(ResultPrimitive.FaultAgain)
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            CircuitBreakerPolicy<ResultPrimitive> outerHandlingFault = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            PolicyWrap<ResultPrimitive> wrap = outerHandlingFault.WrapAsync(innerHandlingFaultAgain);

            PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = await wrap.ExecuteAndCaptureAsync(() => TaskHelper.FromResult(ResultPrimitive.Fault));

            executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
            executeAndCaptureResultOnPolicyWrap.FaultType.Should().Be(FaultType.ResultHandledByThisPolicy);
            executeAndCaptureResultOnPolicyWrap.FinalHandledResult.Should().Be(ResultPrimitive.Fault);
            executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeNull();
            executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().BeNull();
        }

        [Fact]
        public async Task Outermost_generic_policy_not_handling_result_even_if_inner_policies_do_should_not_report_as_handled()
        {
            CircuitBreakerPolicy<ResultPrimitive> innerHandlingFaultAgain = Policy
                .HandleResult(ResultPrimitive.FaultAgain)
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            CircuitBreakerPolicy<ResultPrimitive> outerHandlingFault = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreakerAsync(1, TimeSpan.Zero);
            PolicyWrap<ResultPrimitive> wrap = outerHandlingFault.WrapAsync(innerHandlingFaultAgain);

            PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = await wrap.ExecuteAndCaptureAsync(() => TaskHelper.FromResult(ResultPrimitive.FaultAgain));

            executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Successful);
            executeAndCaptureResultOnPolicyWrap.FinalHandledResult.Should().Be(default(ResultPrimitive));
            executeAndCaptureResultOnPolicyWrap.FaultType.Should().BeNull();
            executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeNull();
            executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().BeNull();
        }

        #endregion

    }
}
