using System;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Specs.Helpers;
using Polly.Wrap;
using Xunit;

namespace Polly.Specs
{
    public class PolicyWrapSpecs
    {
        #region Instance configuration syntax tests, non-generic policies

        [Fact]
        public void Nongeneric_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
        {
            RetryPolicy retry = Policy.Handle<Exception>().Retry(1);

            Action config = () => retry.Wrap((Policy)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Nongeneric_wraps_generic_instance_syntax_wrapping_null_should_throw()
        {
            RetryPolicy retry = Policy.Handle<Exception>().Retry(1);

            Action config = () => retry.Wrap<int>((Policy<int>)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        [Fact]
        public void Generic_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
        {
            RetryPolicy<int> retry = Policy.HandleResult<int>(0).Retry(1);

            Action config = () => retry.Wrap((Policy)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }


        [Fact]
        public void Generic_wraps_generic_instance_syntax_wrapping_null_should_throw()
        {
            RetryPolicy<int> retry = Policy.HandleResult<int>(0).Retry(1);

            Action config = () => retry.Wrap((Policy<int>)null);

            config.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
        }

        #endregion

        #region Static configuration syntax tests, non-generic policies

        [Fact]
        public void Wrapping_nothing_using_static_wrap_syntax_should_throw()
        {
            Action config = () => Policy.Wrap();

            config.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policies");
        }

        [Fact]
        public void Wrapping_only_one_policy_using_static_wrap_syntax_should_throw()
        {
            Policy singlePolicy = Policy.Handle<Exception>().Retry();
            Action config = () => Policy.Wrap(new[] {singlePolicy});

            config.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policies");
        }

        [Fact]
        public void Wrapping_two_policies_using_static_wrap_syntax_should_not_throw()
        {
            Policy retry = Policy.Handle<Exception>().Retry();
            Policy breaker = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromSeconds(10));
            Action config = () => Policy.Wrap(new[] {retry, breaker});

            config.ShouldNotThrow();
        }

        [Fact]
        public void Wrapping_more_than_two_policies_using_static_wrap_syntax_should_not_throw()
        {
            Policy retry = Policy.Handle<Exception>().Retry(1);
            Policy divideByZeroRetry = Policy.Handle<DivideByZeroException>().Retry(2);
            Policy breaker = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromSeconds(10));

            Action config = () => Policy.Wrap(new[] {divideByZeroRetry, retry, breaker});

            config.ShouldNotThrow();
        }

        #endregion

        #region Static configuration syntax tests, strongly-typed policies

        [Fact]
        public void Wrapping_nothing_using_static_wrap_strongly_typed_syntax_should_throw()
        {
            Action config = () => Policy.Wrap<int>();

            config.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policies");
        }

        [Fact]
        public void Wrapping_only_one_policy_using_static_wrap_strongly_typed_syntax_should_throw()
        {
            Policy<int> singlePolicy = Policy<int>.Handle<Exception>().Retry();
            Action config = () => Policy.Wrap<int>(new[] { singlePolicy });

            config.ShouldThrow<ArgumentException>().And.ParamName.Should().Be("policies");
        }

        [Fact]
        public void Wrapping_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
        {
            Policy<int> retry = Policy<int>.Handle<Exception>().Retry();
            Policy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromSeconds(10));
            Action config = () => Policy.Wrap<int>(new[] { retry, breaker });

            config.ShouldNotThrow();
        }

        [Fact]
        public void Wrapping_more_than_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
        {
            Policy<int> retry = Policy<int>.Handle<Exception>().Retry();
            Policy<int> divideByZeroRetry = Policy<int>.Handle<DivideByZeroException>().Retry(2);
            Policy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromSeconds(10));

            Action config = () => Policy.Wrap<int>(new[] { divideByZeroRetry, retry, breaker });

            config.ShouldNotThrow();
        }

        #endregion

        #region Instance-configured: execution tests

        [Fact]
        public void Wrapping_two_policies_by_instance_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
        {
            RetryPolicy retry = Policy.Handle<Exception>().Retry(1); // Two tries in total: first try, plus one retry.
            CircuitBreakerPolicy breaker = Policy.Handle<Exception>().CircuitBreaker(2, TimeSpan.MaxValue);

            PolicyWrap retryWrappingBreaker = retry.Wrap(breaker);
            PolicyWrap breakerWrappingRetry = breaker.Wrap(retry);

            // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
            breaker.Reset();
            retryWrappingBreaker.Invoking(x => x.RaiseException<DivideByZeroException>(2))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
            breaker.Reset();
            breakerWrappingRetry.Invoking(x => x.RaiseException<DivideByZeroException>(2))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Wrapping_two_generic_policies_by_instance_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
        {
            RetryPolicy<ResultPrimitive> retry = Policy.HandleResult(ResultPrimitive.Fault).Retry(1); // Two tries in total: first try, plus one retry.
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreaker(2, TimeSpan.MaxValue);

            var retryWrappingBreaker = retry.Wrap(breaker);
            var breakerWrappingRetry = breaker.Wrap(retry);

            // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
            breaker.Reset();
            retryWrappingBreaker.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
            breaker.Reset();
            breakerWrappingRetry.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Static-configured: execution tests

        [Fact]
        public void Wrapping_two_policies_by_static_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
        {
            RetryPolicy retry = Policy.Handle<Exception>().Retry(1); // Two tries in total: first try, plus one retry.
            CircuitBreakerPolicy breaker = Policy.Handle<Exception>().CircuitBreaker(2, TimeSpan.MaxValue);

            PolicyWrap retryWrappingBreaker = Policy.Wrap(retry, breaker);
            PolicyWrap breakerWrappingRetry = Policy.Wrap(breaker, retry);

            // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
            breaker.Reset();
            retryWrappingBreaker.Invoking(x => x.RaiseException<DivideByZeroException>(2))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
            breaker.Reset();
            breakerWrappingRetry.Invoking(x => x.RaiseException<DivideByZeroException>(2))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Wrapping_two_generic_policies_by_static_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
        {
            RetryPolicy<ResultPrimitive> retry = Policy.HandleResult(ResultPrimitive.Fault).Retry(1); // Two tries in total: first try, plus one retry.
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreaker(2, TimeSpan.MaxValue);

            var retryWrappingBreaker = Policy.Wrap(retry, breaker);
            var breakerWrappingRetry = Policy.Wrap(breaker, retry);

            // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
            breaker.Reset();
            retryWrappingBreaker.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
            breaker.Reset();
            breakerWrappingRetry.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion
    }
}
