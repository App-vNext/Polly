namespace Polly.Specs.Wrap;

[Collection(Constants.SystemClockDependentTestCollection)]
public class PolicyWrapContextAndKeySpecs
{
    #region PolicyKey and execution Context tests

    [Fact]
    public void Should_pass_PolicyKey_to_execution_context_of_outer_policy_as_PolicyWrapKey()
    {
        string retryKey = Guid.NewGuid().ToString();
        string breakerKey = Guid.NewGuid().ToString();
        string wrapKey = Guid.NewGuid().ToString();

        string? policyWrapKeySetOnExecutionContext = null;
        Action<Exception, int, Context> onRetry = (_, _, context) =>
        {
            policyWrapKeySetOnExecutionContext = context.PolicyWrapKey;
        };

        var retry = Policy.Handle<Exception>().Retry(1, onRetry).WithPolicyKey(retryKey);
        var breaker = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero).WithPolicyKey(breakerKey);
        var wrap = retry.Wrap(breaker).WithPolicyKey(wrapKey);

        wrap.RaiseException<Exception>(1);

        policyWrapKeySetOnExecutionContext.ShouldNotBeNull();
        policyWrapKeySetOnExecutionContext.ShouldNotBe(retryKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.ShouldBe(wrapKey);
    }

    [Fact]
    public void Should_pass_PolicyKey_to_execution_context_of_inner_policy_as_PolicyWrapKey()
    {
        string retryKey = Guid.NewGuid().ToString();
        string breakerKey = Guid.NewGuid().ToString();
        string wrapKey = Guid.NewGuid().ToString();

        string? policyWrapKeySetOnExecutionContext = null;
        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) =>
        {
            policyWrapKeySetOnExecutionContext = context.PolicyWrapKey;
        };
        Action<Context> onReset = _ => { };

        var retry = Policy.Handle<Exception>().Retry(1).WithPolicyKey(retryKey);
        var breaker = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero, onBreak, onReset).WithPolicyKey(breakerKey);
        var wrap = retry.Wrap(breaker).WithPolicyKey(wrapKey);

        wrap.RaiseException<Exception>(1);

        policyWrapKeySetOnExecutionContext.ShouldNotBeNull();
        policyWrapKeySetOnExecutionContext.ShouldNotBe(retryKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.ShouldBe(wrapKey);
    }

    [Fact]
    public void Should_restore_PolicyKey_of_outer_policy_to_execution_context_as_move_outwards_through_PolicyWrap()
    {
        var fallback = Policy
            .Handle<Exception>()
            .Fallback(_ => { }, onFallback: (_, context) =>
            {
                context.PolicyWrapKey.ShouldBe("PolicyWrap");
                context.PolicyKey.ShouldBe("FallbackPolicy");
            })
            .WithPolicyKey("FallbackPolicy");

        var retry = Policy
            .Handle<Exception>()
            .Retry(1, onRetry: (_, _, context) =>
            {
                context.PolicyWrapKey.ShouldBe("PolicyWrap");
                context.PolicyKey.ShouldBe("RetryPolicy");
            })
            .WithPolicyKey("RetryPolicy");

        var policyWrap = Policy.Wrap(fallback, retry)
            .WithPolicyKey("PolicyWrap");

        policyWrap.Execute(() => throw new Exception());
    }

    [Fact]
    public void Should_pass_outmost_PolicyWrap_Key_as_PolicyWrapKey_ignoring_inner_PolicyWrap_keys_even_when_executing_policies_in_inner_wrap()
    {
        string retryKey = Guid.NewGuid().ToString();
        string breakerKey = Guid.NewGuid().ToString();
        string fallbackKey = Guid.NewGuid().ToString();
        string innerWrapKey = Guid.NewGuid().ToString();
        string outerWrapKey = Guid.NewGuid().ToString();

        string? policyWrapKeySetOnExecutionContext = null;
        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) =>
        {
            policyWrapKeySetOnExecutionContext = context.PolicyWrapKey;
        };
        Action<Context> doNothingOnReset = _ => { };

        var retry = Policy.Handle<Exception>().Retry(1).WithPolicyKey(retryKey);
        var breaker = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero, onBreak, doNothingOnReset).WithPolicyKey(breakerKey);
        var fallback = Policy.Handle<Exception>().Fallback(() => { }).WithPolicyKey(fallbackKey);

        var innerWrap = retry.Wrap(breaker).WithPolicyKey(innerWrapKey);
        var outerWrap = fallback.Wrap(innerWrap).WithPolicyKey(outerWrapKey);

        outerWrap.RaiseException<Exception>(1);

        policyWrapKeySetOnExecutionContext.ShouldNotBe(retryKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(fallbackKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(innerWrapKey);
        policyWrapKeySetOnExecutionContext.ShouldBe(outerWrapKey);
    }

    [Fact]
    public void Should_pass_outmost_PolicyWrap_Key_as_PolicyWrapKey_to_innermost_Policy_when_execute_method_generic()
    {
        string retryKey = Guid.NewGuid().ToString();
        string breakerKey = Guid.NewGuid().ToString();
        string fallbackKey = Guid.NewGuid().ToString();
        string innerWrapKey = Guid.NewGuid().ToString();
        string outerWrapKey = Guid.NewGuid().ToString();

        string? policyWrapKeySetOnExecutionContext = null;
        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) =>
        {
            policyWrapKeySetOnExecutionContext = context.PolicyWrapKey;
        };
        Action<Context> doNothingOnReset = _ => { };

        var retry = Policy.Handle<Exception>().Retry(1).WithPolicyKey(retryKey);
        var breaker = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.Zero, onBreak, doNothingOnReset).WithPolicyKey(breakerKey);
        var fallback = Policy.Handle<Exception>().Fallback(() => { }).WithPolicyKey(fallbackKey);

        var innerWrap = retry.Wrap(breaker).WithPolicyKey(innerWrapKey);
        var outerWrap = fallback.Wrap(innerWrap).WithPolicyKey(outerWrapKey);

        bool doneOnceOnly = false;
        outerWrap.Execute(() =>
        {
            if (!doneOnceOnly)
            {
                doneOnceOnly = true;
                throw new Exception();
            }
        });

        policyWrapKeySetOnExecutionContext.ShouldNotBe(retryKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(fallbackKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(innerWrapKey);
        policyWrapKeySetOnExecutionContext.ShouldBe(outerWrapKey);
    }

    #endregion
}

[Collection(Constants.SystemClockDependentTestCollection)]
public class PolicyWrapTResultContextAndKeySpecs
{
    #region PolicyKey and execution Context tests

    [Fact]
    public void Should_pass_PolicyKey_to_execution_context_of_outer_policy_as_PolicyWrapKey()
    {
        string retryKey = Guid.NewGuid().ToString();
        string breakerKey = Guid.NewGuid().ToString();
        string wrapKey = Guid.NewGuid().ToString();

        string? policyWrapKeySetOnExecutionContext = null;
        Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, _, context) =>
        {
            policyWrapKeySetOnExecutionContext = context.PolicyWrapKey;
        };

        var retry = Policy.HandleResult(ResultPrimitive.Fault).Retry(1, onRetry).WithPolicyKey(retryKey);
        var breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreaker(1, TimeSpan.Zero).WithPolicyKey(breakerKey);
        var wrap = retry.Wrap(breaker).WithPolicyKey(wrapKey);

        wrap.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Good);

        policyWrapKeySetOnExecutionContext.ShouldNotBe(retryKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.ShouldBe(wrapKey);
    }

    [Fact]
    public void Should_pass_PolicyKey_to_execution_context_of_inner_policy_as_PolicyWrapKey()
    {
        string retryKey = Guid.NewGuid().ToString();
        string breakerKey = Guid.NewGuid().ToString();
        string wrapKey = Guid.NewGuid().ToString();

        string? policyWrapKeySetOnExecutionContext = null;
        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, _, context) =>
        {
            policyWrapKeySetOnExecutionContext = context.PolicyWrapKey;
        };
        Action<Context> onReset = _ => { };

        var retry = Policy.HandleResult(ResultPrimitive.Fault).Retry(1).WithPolicyKey(retryKey);
        var breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreaker(1, TimeSpan.Zero, onBreak, onReset).WithPolicyKey(breakerKey);
        var wrap = retry.Wrap(breaker).WithPolicyKey(wrapKey);

        wrap.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Good);

        policyWrapKeySetOnExecutionContext.ShouldNotBe(retryKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.ShouldBe(wrapKey);
    }

    [Fact]
    public void Should_restore_PolicyKey_of_outer_policy_to_execution_context_as_move_outwards_through_PolicyWrap()
    {
        ISyncPolicy<ResultPrimitive> fallback = Policy<ResultPrimitive>
            .Handle<Exception>()
            .Fallback<ResultPrimitive>(ResultPrimitive.Undefined, onFallback: (_, context) =>
            {
                context.PolicyWrapKey.ShouldBe("PolicyWrap");
                context.PolicyKey.ShouldBe("FallbackPolicy");
            })
            .WithPolicyKey("FallbackPolicy");

        ISyncPolicy<ResultPrimitive> retry = Policy<ResultPrimitive>
            .Handle<Exception>()
            .Retry(1, onRetry: (_, _, context) =>
            {
                context.PolicyWrapKey.ShouldBe("PolicyWrap");
                context.PolicyKey.ShouldBe("RetryPolicy");
            })
            .WithPolicyKey("RetryPolicy");

        Policy<ResultPrimitive> policyWrap = Policy.Wrap(fallback, retry)
            .WithPolicyKey("PolicyWrap");

        policyWrap.Execute(() => throw new Exception());
    }

    [Fact]
    public void Should_pass_outmost_PolicyWrap_Key_as_PolicyWrapKey_ignoring_inner_PolicyWrap_keys_even_when_executing_policies_in_inner_wrap()
    {
        string retryKey = Guid.NewGuid().ToString();
        string breakerKey = Guid.NewGuid().ToString();
        string fallbackKey = Guid.NewGuid().ToString();
        string innerWrapKey = Guid.NewGuid().ToString();
        string outerWrapKey = Guid.NewGuid().ToString();

        string? policyWrapKeySetOnExecutionContext = null;
        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, _, context) =>
        {
            policyWrapKeySetOnExecutionContext = context.PolicyWrapKey;
        };
        Action<Context> doNothingOnReset = _ => { };

        var retry = Policy.HandleResult(ResultPrimitive.Fault).Retry(1).WithPolicyKey(retryKey);
        var breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreaker(1, TimeSpan.Zero, onBreak, doNothingOnReset).WithPolicyKey(breakerKey);
        var fallback = Policy.HandleResult(ResultPrimitive.Fault).Fallback(ResultPrimitive.Substitute).WithPolicyKey(fallbackKey);

        var innerWrap = retry.Wrap(breaker).WithPolicyKey(innerWrapKey);
        var outerWrap = fallback.Wrap(innerWrap).WithPolicyKey(outerWrapKey);

        outerWrap.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Good);

        policyWrapKeySetOnExecutionContext.ShouldNotBe(retryKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(fallbackKey);
        policyWrapKeySetOnExecutionContext.ShouldNotBe(innerWrapKey);
        policyWrapKeySetOnExecutionContext.ShouldBe(outerWrapKey);
    }

    #endregion
}

