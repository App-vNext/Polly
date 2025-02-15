namespace Polly.Specs.RateLimit;

[Collection(Constants.SystemClockDependentTestCollection)]
public class RateLimitPolicyTResultSpecs : RateLimitPolicyTResultSpecsBase, IDisposable
{
    public void Dispose() =>
        SystemClock.Reset();

    protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan) =>
        Policy.RateLimit<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan);

    protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst) =>
        Policy.RateLimit<ResultClassWithRetryAfter>(numberOfExecutions, perTimeSpan, maxBurst);

    protected override IRateLimitPolicy<TResult> GetPolicyViaSyntax<TResult>(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst,
        Func<TimeSpan, Context, TResult> retryAfterFactory) =>
        Policy.RateLimit(numberOfExecutions, perTimeSpan, maxBurst, retryAfterFactory);

    protected override (bool, TimeSpan) TryExecuteThroughPolicy(IRateLimitPolicy policy)
    {
        if (policy is RateLimitPolicy<ResultClassWithRetryAfter> typedPolicy)
        {
            try
            {
                typedPolicy.Execute(() => new ResultClassWithRetryAfter(ResultPrimitive.Good));
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
        if (policy is RateLimitPolicy<TResult> typedPolicy)
        {
            return typedPolicy.Execute(_ => resultIfExecutionPermitted, context);
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
        Func<Context, CancellationToken, EmptyStruct> action = null!;
        IRateLimiter rateLimiter = RateLimiterFactory.Create(TimeSpan.FromSeconds(1), 1);
        Func<TimeSpan, Context, EmptyStruct>? retryAfterFactory = null;

        var instance = Activator.CreateInstance(
            typeof(RateLimitPolicy<EmptyStruct>),
            flags,
            null,
            [rateLimiter, retryAfterFactory],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "EmptyStruct" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_pertimespan_is_negative()
    {
        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => Policy.RateLimit<EmptyStruct>(1, TimeSpan.FromSeconds(-1), 1));

        // Assert
        exception.ParamName.ShouldBe("perTimeSpan");
        exception.ActualValue.ShouldBe(TimeSpan.FromSeconds(-1));
    }

    [Fact]
    public void Should_throw_when_pertimespan_is_zero()
    {
        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => Policy.RateLimit<EmptyStruct>(1, TimeSpan.Zero, 1));

        // Assert
        exception.ParamName.ShouldBe("perTimeSpan");
        exception.ActualValue.ShouldBe(TimeSpan.Zero);
    }
}
