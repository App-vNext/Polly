namespace Polly.Specs.Wrap;

[Collection(Constants.SystemClockDependentTestCollection)]
public class PolicyWrapSpecs
{
    #region Instance configuration syntax tests, non-generic outer

    [Fact]
    public void Nongeneric_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        RetryPolicy retry = Policy.Handle<Exception>().Retry(1);

        Action config = () => retry.Wrap(null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Nongeneric_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        RetryPolicy retry = Policy.Handle<Exception>().Retry(1);

        Action config = () => retry.Wrap<int>(null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Nongeneric_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        Policy policyA = Policy.NoOp();
        Policy policyB = Policy.NoOp();

        PolicyWrap wrap = policyA.Wrap(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    [Fact]
    public void Nongeneric_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        Policy policyA = Policy.NoOp();
        Policy<int> policyB = Policy.NoOp<int>();

        PolicyWrap<int> wrap = policyA.Wrap(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    #endregion

    #region Instance configuration syntax tests, generic outer

    [Fact]
    public void Generic_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        RetryPolicy<int> retry = Policy.HandleResult<int>(0).Retry(1);

        Action config = () => retry.Wrap((Policy)null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Generic_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        RetryPolicy<int> retry = Policy.HandleResult<int>(0).Retry(1);

        Action config = () => retry.Wrap((Policy<int>)null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Generic_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        Policy<int> policyA = Policy.NoOp<int>();
        Policy policyB = Policy.NoOp();

        PolicyWrap<int> wrap = policyA.Wrap(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    [Fact]
    public void Generic_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        Policy<int> policyA = Policy.NoOp<int>();
        Policy<int> policyB = Policy.NoOp<int>();

        PolicyWrap<int> wrap = policyA.Wrap(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    #endregion

    #region Interface extension configuration syntax tests, non-generic outer

    [Fact]
    public void Nongeneric_interface_wraps_nongeneric_instance_syntax_null_wrapping_should_throw()
    {
        ISyncPolicy outerNull = null!;
        ISyncPolicy retry = Policy.Handle<Exception>().Retry(1);

        Action config = () => outerNull.Wrap(retry);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_generic_instance_syntax_null_wrapping_should_throw()
    {
        ISyncPolicy outerNull = null!;
        ISyncPolicy<int> retry = Policy.HandleResult<int>(0).Retry(1);

        Action config = () => outerNull.Wrap<int>(retry);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        ISyncPolicy retry = Policy.Handle<Exception>().Retry(1);

        Action config = () => retry.Wrap(null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        ISyncPolicy retry = Policy.Handle<Exception>().Retry(1);

        Action config = () => retry.Wrap<int>(null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        ISyncPolicy policyA = Policy.NoOp();
        ISyncPolicy policyB = Policy.NoOp();

        IPolicyWrap wrap = policyA.Wrap(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    [Fact]
    public void Nongeneric_interface_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        ISyncPolicy policyA = Policy.NoOp();
        ISyncPolicy<int> policyB = Policy.NoOp<int>();

        IPolicyWrap<int> wrap = policyA.Wrap(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    #endregion

    #region Interface extension configuration syntax tests, generic outer

    [Fact]
    public void Generic_interface_wraps_nongeneric_instance_syntax_null_wrapping_should_throw()
    {
        ISyncPolicy<int> outerNull = null!;
        ISyncPolicy retry = Policy.Handle<Exception>().Retry(1);

        Action config = () => outerNull.Wrap(retry);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_generic_instance_syntax_null_wrapping_should_throw()
    {
        ISyncPolicy<int> outerNull = null!;
        ISyncPolicy<int> retry = Policy.HandleResult<int>(0).Retry(1);

        Action config = () => outerNull.Wrap<int>(retry);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        ISyncPolicy<int> retry = Policy.HandleResult<int>(0).Retry(1);

        Action config = () => retry.Wrap((Policy)null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        ISyncPolicy<int> retry = Policy.HandleResult<int>(0).Retry(1);

        Action config = () => retry.Wrap((Policy<int>)null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        ISyncPolicy<int> policyA = Policy.NoOp<int>();
        ISyncPolicy policyB = Policy.NoOp();

        IPolicyWrap<int> wrap = policyA.Wrap(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    [Fact]
    public void Generic_interface_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        ISyncPolicy<int> policyA = Policy.NoOp<int>();
        ISyncPolicy<int> policyB = Policy.NoOp<int>();

        IPolicyWrap<int> wrap = policyA.Wrap(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    #endregion

    #region Static configuration syntax tests, non-generic policies

    [Fact]
    public void Wrapping_nothing_using_static_wrap_syntax_should_throw()
    {
        Action config = () => Policy.Wrap();

        config.Should().Throw<ArgumentException>().And.ParamName.Should().Be("policies");
    }

    [Fact]
    public void Wrapping_only_one_policy_using_static_wrap_syntax_should_throw()
    {
        Policy singlePolicy = Policy.Handle<Exception>().Retry();

#pragma warning disable S3878 // Remove this array creation and simply pass the elements
        Action config = () => Policy.Wrap(new[] { singlePolicy });
#pragma warning restore S3878

        config.Should().Throw<ArgumentException>().And.ParamName.Should().Be("policies");
    }

    [Fact]
    public void Wrapping_two_policies_using_static_wrap_syntax_should_not_throw()
    {
        Policy retry = Policy.Handle<Exception>().Retry();
        Policy breaker = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromSeconds(10));
        Action config = () => Policy.Wrap(retry, breaker);

        config.Should().NotThrow();
    }

    [Fact]
    public void Wrapping_more_than_two_policies_using_static_wrap_syntax_should_not_throw()
    {
        Policy retry = Policy.Handle<Exception>().Retry(1);
        Policy divideByZeroRetry = Policy.Handle<DivideByZeroException>().Retry(2);
        Policy breaker = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromSeconds(10));

        Action config = () => Policy.Wrap(divideByZeroRetry, retry, breaker);

        config.Should().NotThrow();
    }

    [Fact]
    public void Wrapping_policies_using_static_wrap_syntax_should_set_outer_inner()
    {
        Policy policyA = Policy.NoOp();
        Policy policyB = Policy.NoOp();

        PolicyWrap wrap = Policy.Wrap(policyA, policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    #endregion

    #region Static configuration syntax tests, strongly-typed policies

    [Fact]
    public void Wrapping_nothing_using_static_wrap_strongly_typed_syntax_should_throw()
    {
        Action config = () => Policy.Wrap<int>();

        config.Should().Throw<ArgumentException>().And.ParamName.Should().Be("policies");
    }

    [Fact]
    public void Wrapping_only_one_policy_using_static_wrap_strongly_typed_syntax_should_throw()
    {
        Policy<int> singlePolicy = Policy<int>.Handle<Exception>().Retry();
        Action config = () => Policy.Wrap<int>(singlePolicy);

        config.Should().Throw<ArgumentException>().And.ParamName.Should().Be("policies");
    }

    [Fact]
    public void Wrapping_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
    {
        Policy<int> retry = Policy<int>.Handle<Exception>().Retry();
        Policy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromSeconds(10));
        Action config = () => Policy.Wrap<int>(retry, breaker);

        config.Should().NotThrow();
    }

    [Fact]
    public void Wrapping_more_than_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
    {
        Policy<int> retry = Policy<int>.Handle<Exception>().Retry();
        Policy<int> divideByZeroRetry = Policy<int>.Handle<DivideByZeroException>().Retry(2);
        Policy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromSeconds(10));

        Action config = () => Policy.Wrap<int>(divideByZeroRetry, retry, breaker);

        config.Should().NotThrow();
    }

    [Fact]
    public void Wrapping_policies_using_static_wrap_strongly_typed_syntax_should_set_outer_inner()
    {
        Policy<int> policyA = Policy.NoOp<int>();
        Policy<int> policyB = Policy.NoOp<int>();

        PolicyWrap<int> wrap = Policy.Wrap(policyA, policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
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
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        breakerWrappingRetry.Invoking(x => x.RaiseException<DivideByZeroException>(2))
            .Should().Throw<DivideByZeroException>();
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
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        breakerWrappingRetry.Invoking(x => x.RaiseException<DivideByZeroException>(2))
            .Should().Throw<DivideByZeroException>();
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

    #region ExecuteAndCaptureSpecs

    [Fact]
    public void Outermost_policy_handling_exception_should_report_as_PolicyWrap_handled_exception()
    {
        CircuitBreakerPolicy innerHandlingDBZE = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreaker(1, TimeSpan.Zero);
        CircuitBreakerPolicy outerHandlingANE = Policy
            .Handle<ArgumentNullException>()
            .CircuitBreaker(1, TimeSpan.Zero);
        PolicyWrap wrap = outerHandlingANE.Wrap(innerHandlingDBZE);

        PolicyResult executeAndCaptureResultOnPolicyWrap = wrap.ExecuteAndCapture(() => { throw new ArgumentNullException(); });

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<ArgumentNullException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.HandledByThisPolicy);
    }

    [Fact]
    public void Outermost_policy_not_handling_exception_even_if_inner_policies_do_should_report_as_unhandled_exception()
    {
        CircuitBreakerPolicy innerHandlingDBZE = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreaker(1, TimeSpan.Zero);
        CircuitBreakerPolicy outerHandlingANE = Policy
            .Handle<ArgumentNullException>()
            .CircuitBreaker(1, TimeSpan.Zero);
        PolicyWrap wrap = outerHandlingANE.Wrap(innerHandlingDBZE);

        PolicyResult executeAndCaptureResultOnPolicyWrap = wrap.ExecuteAndCapture(() => { throw new DivideByZeroException(); });

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<DivideByZeroException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.Unhandled);
    }

    [Fact]
    public void Outermost_generic_policy_handling_exception_should_report_as_PolicyWrap_handled_exception()
    {
        CircuitBreakerPolicy<ResultPrimitive> innerHandlingDBZE = Policy<ResultPrimitive>
            .Handle<DivideByZeroException>()
            .CircuitBreaker(1, TimeSpan.Zero);
        CircuitBreakerPolicy outerHandlingANE = Policy
            .Handle<ArgumentNullException>()
            .CircuitBreaker(1, TimeSpan.Zero);
        PolicyWrap<ResultPrimitive> wrap = outerHandlingANE.Wrap(innerHandlingDBZE);

        PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = wrap.ExecuteAndCapture(() => { throw new ArgumentNullException(); });

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<ArgumentNullException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.HandledByThisPolicy);
        executeAndCaptureResultOnPolicyWrap.FaultType.Should().Be(FaultType.ExceptionHandledByThisPolicy);
    }

    [Fact]
    public void Outermost_generic_policy_not_handling_exception_even_if_inner_policies_do_should_report_as_unhandled_exception()
    {
        CircuitBreakerPolicy<ResultPrimitive> innerHandlingDBZE = Policy<ResultPrimitive>
            .Handle<DivideByZeroException>()
            .CircuitBreaker(1, TimeSpan.Zero);
        CircuitBreakerPolicy outerHandlingANE = Policy
            .Handle<ArgumentNullException>()
            .CircuitBreaker(1, TimeSpan.Zero);
        PolicyWrap<ResultPrimitive> wrap = outerHandlingANE.Wrap(innerHandlingDBZE);

        PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = wrap.ExecuteAndCapture(() => { throw new DivideByZeroException(); });

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<DivideByZeroException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.Unhandled);
        executeAndCaptureResultOnPolicyWrap.FaultType.Should().Be(FaultType.UnhandledException);
    }

    [Fact]
    public void Outermost_generic_policy_handling_result_should_report_as_PolicyWrap_handled_result()
    {
        CircuitBreakerPolicy<ResultPrimitive> innerHandlingFaultAgain = Policy
            .HandleResult(ResultPrimitive.FaultAgain)
            .CircuitBreaker(1, TimeSpan.Zero);
        CircuitBreakerPolicy<ResultPrimitive> outerHandlingFault = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreaker(1, TimeSpan.Zero);
        PolicyWrap<ResultPrimitive> wrap = outerHandlingFault.Wrap(innerHandlingFaultAgain);

        PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = wrap.ExecuteAndCapture(() => ResultPrimitive.Fault);

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FaultType.Should().Be(FaultType.ResultHandledByThisPolicy);
        executeAndCaptureResultOnPolicyWrap.FinalHandledResult.Should().Be(ResultPrimitive.Fault);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeNull();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().BeNull();
    }

    [Fact]
    public void Outermost_generic_policy_not_handling_result_even_if_inner_policies_do_should_not_report_as_handled()
    {
        CircuitBreakerPolicy<ResultPrimitive> innerHandlingFaultAgain = Policy
            .HandleResult(ResultPrimitive.FaultAgain)
            .CircuitBreaker(1, TimeSpan.Zero);
        CircuitBreakerPolicy<ResultPrimitive> outerHandlingFault = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreaker(1, TimeSpan.Zero);
        PolicyWrap<ResultPrimitive> wrap = outerHandlingFault.Wrap(innerHandlingFaultAgain);

        PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = wrap.ExecuteAndCapture(() => ResultPrimitive.FaultAgain);

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Successful);
        executeAndCaptureResultOnPolicyWrap.FinalHandledResult.Should().Be(default);
        executeAndCaptureResultOnPolicyWrap.FaultType.Should().BeNull();
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeNull();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().BeNull();
    }

    #endregion
}
