namespace Polly.Specs.Wrap;

[Collection(Constants.SystemClockDependentTestCollection)]
public class PolicyWrapContextAndKeyAsyncSpecs
{
    #region PolicyKey and execution Context tests

    [Fact]
    public async Task Should_pass_PolicyKey_to_execution_context_of_outer_policy_as_PolicyWrapKey()
    {
        string retryKey = Guid.NewGuid().ToString();
        string breakerKey = Guid.NewGuid().ToString();
        string wrapKey = Guid.NewGuid().ToString();

        string? policyWrapKeySetOnExecutionContext = null;
        Action<Exception, int, Context> onRetry = (_, _, context) =>
        {
            policyWrapKeySetOnExecutionContext = context.PolicyWrapKey;
        };

        var retry = Policy.Handle<Exception>().RetryAsync(1, onRetry).WithPolicyKey(retryKey);
        var breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.Zero).WithPolicyKey(breakerKey);
        var wrap = retry.WrapAsync(breaker).WithPolicyKey(wrapKey);

        await wrap.RaiseExceptionAsync<Exception>(1);

        policyWrapKeySetOnExecutionContext.Should().NotBe(retryKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.Should().Be(wrapKey);
    }

    [Fact]
    public async Task Should_pass_PolicyKey_to_execution_context_of_inner_policy_as_PolicyWrapKey()
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

        var retry = Policy.Handle<Exception>().RetryAsync(1).WithPolicyKey(retryKey);
        var breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.Zero, onBreak, onReset).WithPolicyKey(breakerKey);
        var wrap = retry.WrapAsync(breaker).WithPolicyKey(wrapKey);

        await wrap.RaiseExceptionAsync<Exception>(1);

        policyWrapKeySetOnExecutionContext.Should().NotBe(retryKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.Should().Be(wrapKey);
    }

    [Fact]
    public async Task Should_restore_PolicyKey_of_outer_policy_to_execution_context_as_move_outwards_through_PolicyWrap()
    {
        IAsyncPolicy fallback = Policy
            .Handle<Exception>()
            .FallbackAsync((_, _) => TaskHelper.EmptyTask, (_, context) =>
            {
                context.PolicyWrapKey.Should().Be("PolicyWrap");
                context.PolicyKey.Should().Be("FallbackPolicy");
                return TaskHelper.EmptyTask;
            })
            .WithPolicyKey("FallbackPolicy");

        IAsyncPolicy retry = Policy
            .Handle<Exception>()
            .RetryAsync(1, onRetry: (_, _, context) =>
            {
                context.PolicyWrapKey.Should().Be("PolicyWrap");
                context.PolicyKey.Should().Be("RetryPolicy");
            })
            .WithPolicyKey("RetryPolicy");

        IAsyncPolicy policyWrap = Policy.WrapAsync(fallback, retry)
            .WithPolicyKey("PolicyWrap");

        await policyWrap.ExecuteAsync(() => throw new Exception());
    }

    [Fact]
    public async Task Should_restore_PolicyKey_of_outer_policy_to_execution_context_as_move_outwards_through_PolicyWrap_with_deeper_async_execution()
    {
        IAsyncPolicy fallback = Policy
            .Handle<Exception>()
            .FallbackAsync((_, _) => TaskHelper.EmptyTask, (_, context) =>
            {
                context.PolicyWrapKey.Should().Be("PolicyWrap");
                context.PolicyKey.Should().Be("FallbackPolicy");
                return TaskHelper.EmptyTask;
            })
            .WithPolicyKey("FallbackPolicy");

        IAsyncPolicy retry = Policy
            .Handle<Exception>()
            .RetryAsync(1, onRetry: (_, _, context) =>
            {
                context.PolicyWrapKey.Should().Be("PolicyWrap");
                context.PolicyKey.Should().Be("RetryPolicy");
            })
            .WithPolicyKey("RetryPolicy");

        IAsyncPolicy policyWrap = Policy.WrapAsync(fallback, retry)
            .WithPolicyKey("PolicyWrap");

        await policyWrap.ExecuteAsync(async () => await Task.Run(() => throw new Exception())); // Regression test for issue 510
    }

    [Fact]
    public async Task Should_pass_outmost_PolicyWrap_Key_as_PolicyWrapKey_ignoring_inner_PolicyWrap_keys_even_when_executing_policies_in_inner_WrapAsync()
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

        var retry = Policy.Handle<Exception>().RetryAsync(1).WithPolicyKey(retryKey);
        var breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.Zero, onBreak, doNothingOnReset).WithPolicyKey(breakerKey);
        var fallback = Policy.Handle<Exception>().FallbackAsync(_ => TaskHelper.EmptyTask).WithPolicyKey(fallbackKey);

        var innerWrap = retry.WrapAsync(breaker).WithPolicyKey(innerWrapKey);
        var outerWrap = fallback.WrapAsync(innerWrap).WithPolicyKey(outerWrapKey);

        await outerWrap.RaiseExceptionAsync<Exception>(1);

        policyWrapKeySetOnExecutionContext.Should().NotBe(retryKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(fallbackKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(innerWrapKey);
        policyWrapKeySetOnExecutionContext.Should().Be(outerWrapKey);
    }

    [Fact]
    public async Task Should_pass_outmost_PolicyWrap_Key_as_PolicyWrapKey_to_innermost_Policy_when_execute_method_generic()
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

        var retry = Policy.Handle<Exception>().RetryAsync(1).WithPolicyKey(retryKey);
        var breaker = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.Zero, onBreak, doNothingOnReset).WithPolicyKey(breakerKey);
        var fallback = Policy.Handle<Exception>().FallbackAsync(_ => TaskHelper.EmptyTask).WithPolicyKey(fallbackKey);

        var innerWrap = retry.WrapAsync(breaker).WithPolicyKey(innerWrapKey);
        var outerWrap = fallback.WrapAsync(innerWrap).WithPolicyKey(outerWrapKey);

        bool doneOnceOnly = false;
        await outerWrap.ExecuteAsync(() =>
        {
            if (!doneOnceOnly)
            {
                doneOnceOnly = true;
                throw new Exception();
            }

            return TaskHelper.EmptyTask;
        });

        policyWrapKeySetOnExecutionContext.Should().NotBe(retryKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(fallbackKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(innerWrapKey);
        policyWrapKeySetOnExecutionContext.Should().Be(outerWrapKey);
    }

    #endregion
}

[Collection(Constants.SystemClockDependentTestCollection)]
public class PolicyWrapTResultContextAndKeyAsyncSpecs
{
    #region PolicyKey and execution Context tests

    [Fact]
    public async Task Should_pass_PolicyKey_to_execution_context_of_outer_policy_as_PolicyWrapKey()
    {
        string retryKey = Guid.NewGuid().ToString();
        string breakerKey = Guid.NewGuid().ToString();
        string wrapKey = Guid.NewGuid().ToString();

        string? policyWrapKeySetOnExecutionContext = null;
        Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, _, context) =>
        {
            policyWrapKeySetOnExecutionContext = context.PolicyWrapKey;
        };

        var retry = Policy.HandleResult(ResultPrimitive.Fault).RetryAsync(1, onRetry).WithPolicyKey(retryKey);
        var breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreakerAsync(1, TimeSpan.Zero).WithPolicyKey(breakerKey);
        var wrap = retry.WrapAsync(breaker).WithPolicyKey(wrapKey);

        await wrap.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good);

        policyWrapKeySetOnExecutionContext.Should().NotBe(retryKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.Should().Be(wrapKey);
    }

    [Fact]
    public async Task Should_pass_PolicyKey_to_execution_context_of_inner_policy_as_PolicyWrapKey()
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

        var retry = Policy.HandleResult(ResultPrimitive.Fault).RetryAsync(1).WithPolicyKey(retryKey);
        var breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreakerAsync(1, TimeSpan.Zero, onBreak, onReset).WithPolicyKey(breakerKey);
        var wrap = retry.WrapAsync(breaker).WithPolicyKey(wrapKey);

        await wrap.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good);

        policyWrapKeySetOnExecutionContext.Should().NotBe(retryKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.Should().Be(wrapKey);
    }

    [Fact]
    public async Task Should_restore_PolicyKey_of_outer_policy_to_execution_context_as_move_outwards_through_PolicyWrap()
    {
        IAsyncPolicy<ResultPrimitive> fallback = Policy<ResultPrimitive>
            .Handle<Exception>()
            .FallbackAsync((_, _) => Task.FromResult(ResultPrimitive.Undefined), (_, context) =>
            {
                context.PolicyWrapKey.Should().Be("PolicyWrap");
                context.PolicyKey.Should().Be("FallbackPolicy");
                return TaskHelper.EmptyTask;
            })
            .WithPolicyKey("FallbackPolicy");

        IAsyncPolicy<ResultPrimitive> retry = Policy<ResultPrimitive>
            .Handle<Exception>()
            .RetryAsync(1, onRetry: (_, _, context) =>
            {
                context.PolicyWrapKey.Should().Be("PolicyWrap");
                context.PolicyKey.Should().Be("RetryPolicy");
            })
            .WithPolicyKey("RetryPolicy");

        IAsyncPolicy<ResultPrimitive> policyWrap = Policy.WrapAsync(fallback, retry)
            .WithPolicyKey("PolicyWrap");

        await policyWrap.ExecuteAsync(() => throw new Exception());
    }

    [Fact]
    public async Task Should_restore_PolicyKey_of_outer_policy_to_execution_context_as_move_outwards_through_PolicyWrap_with_deeper_async_execution()
    {
        IAsyncPolicy<ResultPrimitive> fallback = Policy<ResultPrimitive>
            .Handle<Exception>()
            .FallbackAsync((_, _) => Task.FromResult(ResultPrimitive.Undefined), (_, context) =>
            {
                context.PolicyWrapKey.Should().Be("PolicyWrap");
                context.PolicyKey.Should().Be("FallbackPolicy");
                return TaskHelper.EmptyTask;
            })
            .WithPolicyKey("FallbackPolicy");

        IAsyncPolicy<ResultPrimitive> retry = Policy<ResultPrimitive>
            .Handle<Exception>()
            .RetryAsync(1, onRetry: (_, _, context) =>
            {
                context.PolicyWrapKey.Should().Be("PolicyWrap");
                context.PolicyKey.Should().Be("RetryPolicy");
            })
            .WithPolicyKey("RetryPolicy");

        IAsyncPolicy<ResultPrimitive> policyWrap = Policy.WrapAsync(fallback, retry)
            .WithPolicyKey("PolicyWrap");

        await policyWrap.ExecuteAsync(async () => await Task.Run(() => // Regression test for issue 510
        {
            throw new Exception();
#pragma warning disable 0162 // unreachable code detected
            return ResultPrimitive.WhateverButTooLate;
#pragma warning restore 0162

        }));
    }

    [Fact]
    public async Task Should_pass_outmost_PolicyWrap_Key_as_PolicyWrapKey_ignoring_inner_PolicyWrap_keys_even_when_executing_policies_in_inner_WrapAsync()
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

        var retry = Policy.HandleResult(ResultPrimitive.Fault).RetryAsync(1).WithPolicyKey(retryKey);
        var breaker = Policy.HandleResult(ResultPrimitive.Fault).CircuitBreakerAsync(1, TimeSpan.Zero, onBreak, doNothingOnReset).WithPolicyKey(breakerKey);
        var fallback = Policy.HandleResult(ResultPrimitive.Fault).FallbackAsync(ResultPrimitive.Substitute).WithPolicyKey(fallbackKey);

        var innerWrap = retry.WrapAsync(breaker).WithPolicyKey(innerWrapKey);
        var outerWrap = fallback.WrapAsync(innerWrap).WithPolicyKey(outerWrapKey);

        await outerWrap.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good);

        policyWrapKeySetOnExecutionContext.Should().NotBe(retryKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(breakerKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(fallbackKey);
        policyWrapKeySetOnExecutionContext.Should().NotBe(innerWrapKey);
        policyWrapKeySetOnExecutionContext.Should().Be(outerWrapKey);
    }

    #endregion
}

