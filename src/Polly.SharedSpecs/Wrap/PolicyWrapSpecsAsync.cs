using System;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Specs.Helpers;
using Polly.Wrap;
using Xunit;

namespace Polly.Specs.Wrap
{
    public class PolicyWrapSpecsAsync
    {
        #region Instance configuration syntax tests, non-generic policies

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
        public async void Wrapping_two_generic_policies_by_instance_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
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
        public async void Wrapping_two_generic_policies_by_static_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
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
    }
}
