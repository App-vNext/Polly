namespace Polly.Specs.RateLimit;

[Collection(Constants.SystemClockDependentTestCollection)]
public class AsyncRateLimitPolicyTResultSpecs : RateLimitPolicyTResultSpecsBase, IDisposable
{
    public void Dispose() =>
        SystemClock.Reset();

    protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan) =>
        Policy.RateLimitAsync<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan);

    protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst) =>
        Policy.RateLimitAsync<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan, maxBurst);

    protected override IRateLimitPolicy<TResult> GetPolicyViaSyntax<TResult>(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst,
        Func<TimeSpan, Context, TResult> retryAfterFactory) =>
        Policy.RateLimitAsync(numberOfExecutions, perTimeSpan, maxBurst, retryAfterFactory);

    protected override (bool, TimeSpan) TryExecuteThroughPolicy(IRateLimitPolicy policy)
    {
        if (policy is AsyncRateLimitPolicy<ResultClassWithRetryAfter> typedPolicy)
        {
            try
            {
                typedPolicy.ExecuteAsync(() => Task.FromResult(new ResultClassWithRetryAfter(ResultPrimitive.Good))).GetAwaiter().GetResult();
                return (true, TimeSpan.Zero);
            }
            catch (RateLimitRejectedException e)
            {
                return (false, e.RetryAfter);
            }
        }
        else
        {
            throw new InvalidOperationException("Unexpected policy type in test construction.");
        }
    }

    protected override TResult TryExecuteThroughPolicy<TResult>(IRateLimitPolicy<TResult> policy, Context context, TResult resultIfExecutionPermitted)
    {
        if (policy is AsyncRateLimitPolicy<TResult> typedPolicy)
        {
            return typedPolicy.ExecuteAsync(_ => Task.FromResult(resultIfExecutionPermitted), context).GetAwaiter().GetResult();
        }
        else
        {
            throw new InvalidOperationException("Unexpected policy type in test construction.");
        }
    }

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;
        IRateLimiter rateLimiter = new LockFreeTokenBucketRateLimiter(TimeSpan.FromSeconds(1), 1);
        Func<TimeSpan, Context, EmptyStruct>? retryAfterFactory = null!;

        var instance = Activator.CreateInstance(
            typeof(AsyncRateLimitPolicy<EmptyStruct>),
            flags,
            null,
            [rateLimiter, retryAfterFactory],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_pertimespan_is_negative()
    {
        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => Policy.RateLimitAsync<EmptyStruct>(1, TimeSpan.FromSeconds(-1), 1));

        // Assert
        exception.ParamName.ShouldBe("perTimeSpan");
        exception.ActualValue.ShouldBe(TimeSpan.FromSeconds(-1));
    }

    [Fact]
    public void Should_throw_when_pertimespan_is_zero()
    {
        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => Policy.RateLimitAsync<EmptyStruct>(1, TimeSpan.Zero, 1));

        // Assert
        exception.ParamName.ShouldBe("perTimeSpan");
        exception.ActualValue.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Should_not_throw_when_pertimespan_is_greater_than_zero()
    {
        // Act and Assert
        Should.NotThrow(() => Policy.RateLimitAsync<EmptyStruct>(1, TimeSpan.FromTicks(1), 1));
    }
}
