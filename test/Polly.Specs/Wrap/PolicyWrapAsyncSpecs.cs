namespace Polly.Specs.Wrap;

[Collection(Constants.SystemClockDependentTestCollection)]
public class PolicyWrapAsyncSpecs
{
    #region Instance configuration syntax tests, non-generic outer

    [Fact]
    public void Nongeneric_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        var retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => retry.WrapAsync(null!);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("innerPolicy");
    }

    [Fact]
    public void Nongeneric_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        var retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => retry.WrapAsync<int>(null!);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("innerPolicy");
    }

    [Fact]
    public void Nongeneric_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy policyA = Policy.NoOpAsync();
        AsyncPolicy policyB = Policy.NoOpAsync();

        AsyncPolicyWrap wrap = policyA.WrapAsync(policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
    }

    [Fact]
    public void Nongeneric_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy policyA = Policy.NoOpAsync();
        AsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

        AsyncPolicyWrap<int> wrap = policyA.WrapAsync(policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
    }

    #endregion

    #region   Instance configuration syntax tests, generic outer

    [Fact]
    public void Generic_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        var retry = Policy.HandleResult(0).RetryAsync(1);

        Action config = () => retry.WrapAsync((AsyncPolicy)null!);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("innerPolicy");
    }

    [Fact]
    public void Generic_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        var retry = Policy.HandleResult(0).RetryAsync(1);

        Action config = () => retry.WrapAsync((AsyncPolicy<int>)null!);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("innerPolicy");
    }

    [Fact]
    public void Generic_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
        AsyncPolicy policyB = Policy.NoOpAsync();

        AsyncPolicyWrap<int> wrap = policyA.WrapAsync(policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
    }

    [Fact]
    public void Generic_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
        AsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

        AsyncPolicyWrap<int> wrap = policyA.WrapAsync(policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
    }

    #endregion

    #region Interface extension configuration syntax tests, non-generic outer

    [Fact]
    public void Nongeneric_interface_wraps_nongeneric_instance_syntax_null_wrapping_should_throw()
    {
        IAsyncPolicy outerNull = null!;
        IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => outerNull.WrapAsync(retry);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("outerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_generic_instance_syntax_null_wrapping_should_throw()
    {
        IAsyncPolicy outerNull = null!;
        IAsyncPolicy<int> retry = Policy.HandleResult(0).RetryAsync(1);

        Action config = () => outerNull.WrapAsync(retry);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("outerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => retry.WrapAsync(null!);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("innerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => retry.WrapAsync<int>(null!);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("innerPolicy");
    }

    [Fact]
    public void Nongeneric_interface_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        IAsyncPolicy policyA = Policy.NoOpAsync();
        IAsyncPolicy policyB = Policy.NoOpAsync();

        var wrap = policyA.WrapAsync(policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
    }

    [Fact]
    public void Nongeneric_interface_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        IAsyncPolicy policyA = Policy.NoOpAsync();
        IAsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

        var wrap = policyA.WrapAsync(policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
    }

    #endregion

    #region Interface extension configuration syntax tests, generic outer

    [Fact]
    public void Generic_interface_wraps_nongeneric_instance_syntax_null_wrapping_should_throw()
    {
        IAsyncPolicy<int> outerNull = null!;
        IAsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);

        Action config = () => outerNull.WrapAsync(retry);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("outerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_generic_instance_syntax_null_wrapping_should_throw()
    {
        IAsyncPolicy<int> outerNull = null!;
        IAsyncPolicy<int> retry = Policy.HandleResult(0).RetryAsync(1);

        Action config = () => outerNull.WrapAsync(retry);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("outerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_nongeneric_instance_syntax_wrapping_null_should_throw()
    {
        IAsyncPolicy<int> retry = Policy.HandleResult(0).RetryAsync(1);

        Action config = () => retry.WrapAsync((AsyncPolicy)null!);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("innerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_generic_instance_syntax_wrapping_null_should_throw()
    {
        IAsyncPolicy<int> retry = Policy.HandleResult(0).RetryAsync(1);

        Action config = () => retry.WrapAsync((AsyncPolicy<int>)null!);

        Should.Throw<ArgumentNullException>(config).ParamName.ShouldBe("innerPolicy");
    }

    [Fact]
    public void Generic_interface_wraps_nongeneric_using_instance_wrap_syntax_should_set_outer_inner()
    {
        IAsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
        IAsyncPolicy policyB = Policy.NoOpAsync();

        var wrap = policyA.WrapAsync(policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
    }

    [Fact]
    public void Generic_interface_wraps_generic_using_instance_wrap_syntax_should_set_outer_inner()
    {
        IAsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
        IAsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

        var wrap = policyA.WrapAsync(policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
    }

    #endregion

    #region Static configuration syntax tests, non-generic policies

    [Fact]
    public void Wrapping_nothing_using_static_wrap_syntax_should_throw()
    {
        Action config = () => Policy.WrapAsync();

        Should.Throw<ArgumentException>(config).ParamName.ShouldBe("policies");
    }

    [Fact]
    public void Wrapping_only_one_policy_using_static_wrap_syntax_should_throw()
    {
        AsyncPolicy singlePolicy = Policy.Handle<Exception>().RetryAsync();
        Action config = () => Policy.WrapAsync(singlePolicy);

        Should.Throw<ArgumentException>(config).ParamName.ShouldBe("policies");
    }

    [Fact]
    public void Wrapping_two_policies_using_static_wrap_syntax_should_not_throw()
    {
        AsyncPolicy retry = Policy.Handle<Exception>().RetryAsync();
        AsyncPolicy breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));
        Action config = () => Policy.WrapAsync(retry, breaker);

        Should.NotThrow(config);
    }

    [Fact]
    public void Wrapping_more_than_two_policies_using_static_wrap_syntax_should_not_throw()
    {
        AsyncPolicy retry = Policy.Handle<Exception>().RetryAsync(1);
        AsyncPolicy divideByZeroRetry = Policy.Handle<DivideByZeroException>().RetryAsync(2);
        AsyncPolicy breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));

        Action config = () => Policy.WrapAsync(divideByZeroRetry, retry, breaker);

        Should.NotThrow(config);
    }

    [Fact]
    public void Wrapping_policies_using_static_wrap_syntax_should_set_outer_inner()
    {
        AsyncPolicy policyA = Policy.NoOpAsync();
        AsyncPolicy policyB = Policy.NoOpAsync();

        AsyncPolicyWrap wrap = Policy.WrapAsync(policyA, policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
    }

    #endregion

    #region Static configuration syntax tests, strongly-typed policies

    [Fact]
    public void Wrapping_nothing_using_static_wrap_strongly_typed_syntax_should_throw()
    {
        Action config = () => Policy.WrapAsync<int>();

        Should.Throw<ArgumentException>(config).ParamName.ShouldBe("policies");
    }

    [Fact]
    public void Wrapping_only_one_policy_using_static_wrap_strongly_typed_syntax_should_throw()
    {
        AsyncPolicy<int> singlePolicy = Policy<int>.Handle<Exception>().RetryAsync();
        Action config = () => Policy.WrapAsync(singlePolicy);

        Should.Throw<ArgumentException>(config).ParamName.ShouldBe("policies");
    }

    [Fact]
    public void Wrapping_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
    {
        AsyncPolicy<int> retry = Policy<int>.Handle<Exception>().RetryAsync();
        AsyncPolicy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));
        Action config = () => Policy.WrapAsync(retry, breaker);

        Should.NotThrow(config);
    }

    [Fact]
    public void Wrapping_more_than_two_policies_using_static_wrap_strongly_typed_syntax_should_not_throw()
    {
        AsyncPolicy<int> retry = Policy<int>.Handle<Exception>().RetryAsync();
        AsyncPolicy<int> divideByZeroRetry = Policy<int>.Handle<DivideByZeroException>().RetryAsync(2);
        AsyncPolicy<int> breaker = Policy<int>.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromSeconds(10));

        Action config = () => Policy.WrapAsync(divideByZeroRetry, retry, breaker);

        Should.NotThrow(config);
    }

    [Fact]
    public void Wrapping_policies_using_static_wrap_strongly_typed_syntax_should_set_outer_inner()
    {
        AsyncPolicy<int> policyA = Policy.NoOpAsync<int>();
        AsyncPolicy<int> policyB = Policy.NoOpAsync<int>();

        AsyncPolicyWrap<int> wrap = Policy.WrapAsync(policyA, policyB);

        wrap.Outer.ShouldBeSameAs(policyA);
        wrap.Inner.ShouldBeSameAs(policyB);
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
        await Should.ThrowAsync<DivideByZeroException>(() => retryWrappingBreaker.RaiseExceptionAsync<DivideByZeroException>(2));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        await Should.ThrowAsync<DivideByZeroException>(() => breakerWrappingRetry.RaiseExceptionAsync<DivideByZeroException>(2));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
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
            .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        (await breakerWrappingRetry.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
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
        await Should.ThrowAsync<DivideByZeroException>(() => retryWrappingBreaker.RaiseExceptionAsync<DivideByZeroException>(2));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        await Should.ThrowAsync<DivideByZeroException>(() => breakerWrappingRetry.RaiseExceptionAsync<DivideByZeroException>(2));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
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
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // When the breaker wraps the retry, the retry (being inner) should retry twice before throwing the exception back on the breaker - the exception only hits the breaker once - so the breaker should not break.
        breaker.Reset();
        (await breakerWrappingRetry.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault))
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
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

        executeAndCaptureResultOnPolicyWrap.Outcome.ShouldBe(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.ShouldBeOfType<ArgumentNullException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.ShouldBe(ExceptionType.HandledByThisPolicy);
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

        executeAndCaptureResultOnPolicyWrap.Outcome.ShouldBe(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.ShouldBeOfType<DivideByZeroException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.ShouldBe(ExceptionType.Unhandled);
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

        executeAndCaptureResultOnPolicyWrap.Outcome.ShouldBe(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.ShouldBeOfType<ArgumentNullException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.ShouldBe(ExceptionType.HandledByThisPolicy);
        executeAndCaptureResultOnPolicyWrap.FaultType.ShouldBe(FaultType.ExceptionHandledByThisPolicy);
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

        executeAndCaptureResultOnPolicyWrap.Outcome.ShouldBe(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FinalException.ShouldBeOfType<DivideByZeroException>();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.ShouldBe(ExceptionType.Unhandled);
        executeAndCaptureResultOnPolicyWrap.FaultType.ShouldBe(FaultType.UnhandledException);
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

        executeAndCaptureResultOnPolicyWrap.Outcome.ShouldBe(OutcomeType.Failure);
        executeAndCaptureResultOnPolicyWrap.FaultType.ShouldBe(FaultType.ResultHandledByThisPolicy);
        executeAndCaptureResultOnPolicyWrap.FinalHandledResult.ShouldBe(ResultPrimitive.Fault);
        executeAndCaptureResultOnPolicyWrap.FinalException.ShouldBeNull();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.ShouldBeNull();
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

        executeAndCaptureResultOnPolicyWrap.Outcome.ShouldBe(OutcomeType.Successful);
        executeAndCaptureResultOnPolicyWrap.FinalHandledResult.ShouldBe(default);
        executeAndCaptureResultOnPolicyWrap.FaultType.ShouldBeNull();
        executeAndCaptureResultOnPolicyWrap.FinalException.ShouldBeNull();
        executeAndCaptureResultOnPolicyWrap.ExceptionType.ShouldBeNull();
    }

    #endregion
}
