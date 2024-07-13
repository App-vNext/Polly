namespace Polly.Specs.Wrap;

[Collection(Constants.SystemClockDependentTestCollection)]
public class PolicyWrapSpecsAsync
{
    #region Instance configuration syntax tests, non-generic outer

    [Fact]
    public void Nongeneric_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        var retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => retry.WrapAsync(null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Nongeneric_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        var retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => retry.WrapAsync<int>(null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Nongeneric_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy policyA = Policy.NoOpAsync();
        AsyncPolicy policyB = Policy.NoOpAsync();

        AsyncPolicyWrap wrap = policyA.WrapAsync(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    [Fact]
    public void Nongeneric_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy policyA = Policy.NoOpAsync();
        AsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

        AsyncPolicyWrap<int> wrap = policyA.WrapAsync(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    #endregion

    #region   Instance configuration syntax tests, generic outer

    [Fact]
    public void Generic_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        var retry = Policy.HandleResult<int>(0).RetryAsync(1);

        Action config = () => retry.WrapAsync((AsyncPolicy)null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Generic_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        var retry = Policy.HandleResult<int>(0).RetryAsync(1);

        Action config = () => retry.WrapAsync((AsyncPolicy<int>)null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Generic_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
        AsyncPolicy policyB = Policy.NoOpAsync();

        AsyncPolicyWrap<int> wrap = policyA.WrapAsync(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    [Fact]
    public void Generic_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
        AsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

        AsyncPolicyWrap<int> wrap = policyA.WrapAsync(policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    #endregion

    #region Interface extension configuration syntax tests, non-generic outer

    [Fact]
    public void Nongeneric_interface_wraps_nongeneric_instance_syntax_null_wrapping_should_throw()
    {
        IAsyncPolicy outerNull = null!;
        IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => outerNull.WrapAsync(retry);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_generic_instance_syntax_null_wrapping_should_throw()
    {
        IAsyncPolicy outerNull = null!;
        IAsyncPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

        Action config = () => outerNull.WrapAsync<int>(retry);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => retry.WrapAsync(null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => retry.WrapAsync<int>(null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
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
        IAsyncPolicy<int> outerNull = null!;
        IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => outerNull.WrapAsync(retry);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_generic_instance_syntax_null_wrapping_should_throw()
    {
        IAsyncPolicy<int> outerNull = null!;
        IAsyncPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

        Action config = () => outerNull.WrapAsync<int>(retry);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("outerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        IAsyncPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

        Action config = () => retry.WrapAsync((AsyncPolicy)null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        IAsyncPolicy<int> retry = Policy.HandleResult<int>(0).RetryAsync(1);

        Action config = () => retry.WrapAsync((AsyncPolicy<int>)null!);

        config.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("innerPolicy");
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

        config.Should().Throw<ArgumentException>().And.ParamName.Should().Be("policies");
    }

    [Fact]
    public void Wrapping_only_one_policy_using_static_wrap_syntax_should_throw()
    {
        AsyncPolicy singlePolicy = Policy.Handle<Exception>().RetryAsync();
        Action config = () => Policy.WrapAsync(singlePolicy);

        config.Should().Throw<ArgumentException>().And.ParamName.Should().Be("policies");
    }

    [Fact]
    public void Wrapping_two_policies_using_static_wrap_syntax_should_not_throw()
    {
        AsyncPolicy retry = Policy.Handle<Exception>().RetryAsync();
        AsyncPolicy breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));
        Action config = () => Policy.WrapAsync(retry, breaker);

        config.Should().NotThrow();
    }

    [Fact]
    public void Wrapping_more_than_two_policies_using_static_wrap_syntax_should_not_throw()
    {
        AsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);
        AsyncPolicy divideByZeroRetry = Policy.Handle<DivideByZeroException>().RetryAsync(2);
        AsyncPolicy breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));

        Action config = () => Policy.WrapAsync(divideByZeroRetry, retry, breaker);

        config.Should().NotThrow();
    }

    [Fact]
    public void Wrapping_policies_using_static_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy policyA = Policy.NoOpAsync();
        AsyncPolicy policyB = Policy.NoOpAsync();

        AsyncPolicyWrap wrap = Policy.WrapAsync(policyA, policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    #endregion

    #region Static configuration syntax tests, strongly-typed policies

    [Fact]
    public void Wrapping_nothing_using_static_wrap_strongly_typed_syntax_should_throw()
    {
        Action config = () => Policy.WrapAsync<int>();

        config.Should().Throw<ArgumentException>().And.ParamName.Should().Be("policies");
    }

    [Fact]
    public void Wrapping_only_one_policy_using_static_wrap_strongly_typed_syntax_should_throw()
    {
        AsyncPolicy<int> singlePolicy = Policy<int>.Handle<Exception>().RetryAsync();
        Action config = () => Policy.WrapAsync<int>(singlePolicy);

        config.Should().Throw<ArgumentException>().And.ParamName.Should().Be("policies");
    }

    [Fact]
    public void Wrapping_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
    {
        AsyncPolicy<int> retry = Policy<int>.Handle<Exception>().RetryAsync();
        AsyncPolicy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));
        Action config = () => Policy.WrapAsync<int>(retry, breaker);

        config.Should().NotThrow();
    }

    [Fact]
    public void Wrapping_more_than_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
    {
        AsyncPolicy<int> retry = Policy<int>.Handle<Exception>().RetryAsync();
        AsyncPolicy<int> divideByZeroRetry = Policy<int>.Handle<DivideByZeroException>().RetryAsync(2);
        AsyncPolicy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));

        Action config = () => Policy.WrapAsync<int>(divideByZeroRetry, retry, breaker);

        config.Should().NotThrow();
    }

    [Fact]
    public void Wrapping_policies_using_static_wrap_strongly_typed_syntax_should_set_outer_inner()
    {
        AsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
        AsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

        AsyncPolicyWrap<int> wrap = Policy.WrapAsync(policyA, policyB);

        wrap.Outer.Should().BeSameAs(policyA);
        wrap.Inner.Should().BeSameAs(policyB);
    }

    #endregion

    #region Instance-configured: execution tests

    [Fact]
    public async Task Wrapping_two_policies_by_instance_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
    {
        var retry = Policy.Handle<Exception>().RetryAsync(1); // Two tries in total: first try, plus one retry.
        var breaker = Policy.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.MaxValue);

        AsyncPolicyWrap retryWrappingBreaker = retry.WrapAsync(breaker);
        AsyncPolicyWrap breakerWrappingRetry = breaker.WrapAsync(retry);

        // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
        breaker.Reset();
        await retryWrappingBreaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(2))
            .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        await breakerWrappingRetry.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(2))
            .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task Wrapping_two_generic_policies_by_instance_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
    {
        var retry = Policy.HandleResult(ResultPrimitive.Fault).RetryAsync(1); // Two tries in total: first try, plus one retry.
        var breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreakerAsync(2, TimeSpan.MaxValue);

        var retryWrappingBreaker = retry.WrapAsync(breaker);
        var breakerWrappingRetry = breaker.WrapAsync(retry);

        // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
        breaker.Reset();
        (await retryWrappingBreaker.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault))
            .Should().Be(ResultPrimitive.Fault);
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        (await breakerWrappingRetry.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault))
            .Should().Be(ResultPrimitive.Fault);
        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    #endregion

    #region Static-configured: execution tests

    [Fact]
    public async Task Wrapping_two_policies_by_static_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
    {
        var retry = Policy.Handle<Exception>().RetryAsync(1); // Two tries in total: first try, plus one retry.
        var breaker = Policy.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.MaxValue);

        AsyncPolicyWrap retryWrappingBreaker = Policy.WrapAsync(retry, breaker);
        AsyncPolicyWrap breakerWrappingRetry = Policy.WrapAsync(breaker, retry);

        // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
        breaker.Reset();
        await retryWrappingBreaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(2))
            .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        await breakerWrappingRetry.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(2))
            .Should().ThrowAsync<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task Wrapping_two_generic_policies_by_static_syntax_and_executing_should_wrap_outer_then_inner_around_delegate()
    {
        var retry = Policy.HandleResult(ResultPrimitive.Fault).RetryAsync(1); // Two tries in total: first try, plus one retry.
        var breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreakerAsync(2, TimeSpan.MaxValue);

        var retryWrappingBreaker = Policy.WrapAsync(retry, breaker);
        var breakerWrappingRetry = Policy.WrapAsync(breaker, retry);

        // When the retry wraps the breaker, the retry (being outer) should cause the call to be put through the breaker twice - causing the breaker to break.
        breaker.Reset();
        (await retryWrappingBreaker.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault))
              .Should().Be(ResultPrimitive.Fault);
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        (await breakerWrappingRetry.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault))
              .Should().Be(ResultPrimitive.Fault);
        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    #endregion

    #region ExecuteAndCaptureAsyncSpecs

    [Fact]
    public async Task Outermost_policy_handling_exception_should_report_as_PolicyWrap_handled_exception()
    {
        var innerHandlingDBZE = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        var outerHandlingANE = Policy
            .Handle<ArgumentNullException>()
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        AsyncPolicyWrap wrap = outerHandlingANE.WrapAsync(innerHandlingDBZE);

        PolicyResult executeAndCaptureResultOnPolicyWrap =
            await wrap.ExecuteAndCaptureAsync(() => { throw new ArgumentNullException(); });

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<ArgumentNullException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.HandledByThisPolicy);
    }

    [Fact]
    public async Task Outermost_policy_not_handling_exception_even_if_inner_policies_do_should_report_as_unhandled_exception()
    {
        var innerHandlingDBZE = Policy
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        var outerHandlingANE = Policy
            .Handle<ArgumentNullException>()
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        AsyncPolicyWrap wrap = outerHandlingANE.WrapAsync(innerHandlingDBZE);

        PolicyResult executeAndCaptureResultOnPolicyWrap =
            await wrap.ExecuteAndCaptureAsync(() => { throw new DivideByZeroException(); });

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<DivideByZeroException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.Unhandled);
    }

    [Fact]
    public async Task Outermost_generic_policy_handling_exception_should_report_as_PolicyWrap_handled_exception()
    {
        var innerHandlingDBZE = Policy<ResultPrimitive>
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        var outerHandlingANE = Policy<ResultPrimitive>
            .Handle<ArgumentNullException>()
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        AsyncPolicyWrap<ResultPrimitive> wrap = outerHandlingANE.WrapAsync(innerHandlingDBZE);

        PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = await wrap.ExecuteAndCaptureAsync(() => { throw new ArgumentNullException(); });

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<ArgumentNullException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.HandledByThisPolicy);
        executeAndCaptureResultOnPolicyWrap.FaultType.Should().Be(FaultType.ExceptionHandledByThisPolicy);
    }

    [Fact]
    public async Task Outermost_generic_policy_not_handling_exception_even_if_inner_policies_do_should_report_as_unhandled_exception()
    {
        var innerHandlingDBZE = Policy<ResultPrimitive>
            .Handle<DivideByZeroException>()
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        var outerHandlingANE = Policy<ResultPrimitive>
            .Handle<ArgumentNullException>()
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        AsyncPolicyWrap<ResultPrimitive> wrap = outerHandlingANE.WrapAsync(innerHandlingDBZE);

        PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = await wrap.ExecuteAndCaptureAsync(() => { throw new DivideByZeroException(); });

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeOfType<DivideByZeroException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().Be(ExceptionType.Unhandled);
        executeAndCaptureResultOnPolicyWrap.FaultType.Should().Be(FaultType.UnhandledException);
    }

    [Fact]
    public async Task Outermost_generic_policy_handling_result_should_report_as_PolicyWrap_handled_result()
    {
        var innerHandlingFaultAgain = Policy
            .HandleResult(ResultPrimitive.FaultAgain)
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        var outerHandlingFault = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        AsyncPolicyWrap<ResultPrimitive> wrap = outerHandlingFault.WrapAsync(innerHandlingFaultAgain);

        PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = await wrap.ExecuteAndCaptureAsync(() => Task.FromResult(ResultPrimitive.Fault));

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FaultType.Should().Be(FaultType.ResultHandledByThisPolicy);
        executeAndCaptureResultOnPolicyWrap.FinalHandledResult.Should().Be(ResultPrimitive.Fault);
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeNull();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().BeNull();
    }

    [Fact]
    public async Task Outermost_generic_policy_not_handling_result_even_if_inner_policies_do_should_not_report_as_handled()
    {
        var innerHandlingFaultAgain = Policy
            .HandleResult(ResultPrimitive.FaultAgain)
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        var outerHandlingFault = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreakerAsync(1, TimeSpan.Zero);
        AsyncPolicyWrap<ResultPrimitive> wrap = outerHandlingFault.WrapAsync(innerHandlingFaultAgain);

        PolicyResult<ResultPrimitive> executeAndCaptureResultOnPolicyWrap = await wrap.ExecuteAndCaptureAsync(() => Task.FromResult(ResultPrimitive.FaultAgain));

        executeAndCaptureResultOnPolicyWrap.Outcome.Should().Be(OutcomeType.Successful);
        executeAndCaptureResultOnPolicyWrap.FinalHandledResult.Should().Be(default);
        executeAndCaptureResultOnPolicyWrap.FaultType.Should().BeNull();
        executeAndCaptureResultOnPolicyWrap.FinalException.Should().BeNull();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.Should().BeNull();
    }

    #endregion
}
