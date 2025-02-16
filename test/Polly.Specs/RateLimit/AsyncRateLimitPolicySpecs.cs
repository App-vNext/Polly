namespace Polly.Specs.RateLimit;

[Collection(Constants.SystemClockDependentTestCollection)]
public class AsyncRateLimitPolicySpecs : RateLimitPolicySpecsBase, IDisposable
{
    public void Dispose() =>
        SystemClock.Reset();

    protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan) =>
        Policy.RateLimitAsync(numberOfExecutions, perTimeSpan);

    protected override IRateLimitPolicy GetPolicyViaSyntax(int numberOfExecutions, TimeSpan perTimeSpan, int maxBurst) =>
        Policy.RateLimitAsync(numberOfExecutions, perTimeSpan, maxBurst);

    protected override (bool, TimeSpan) TryExecuteThroughPolicy(IRateLimitPolicy policy)
    {
        if (policy is AsyncRateLimitPolicy typedPolicy)
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

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;
        IRateLimiter rateLimiter = new LockFreeTokenBucketRateLimiter(TimeSpan.FromSeconds(1), 1);

        var instance = Activator.CreateInstance(
            typeof(AsyncRateLimitPolicy),
            flags,
            null,
            [rateLimiter],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });
        var generic = methodInfo.MakeGenericMethod(typeof(EmptyStruct));

        var func = () => generic.Invoke(instance, [action, new Context(), CancellationToken, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_pertimespan_is_negative()
    {
        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => Policy.RateLimitAsync(1, TimeSpan.FromSeconds(-1), 1));

        // Assert
        exception.Message.ShouldStartWith("perTimeSpan must be a positive timespan");
        exception.ParamName.ShouldBe("perTimeSpan");
        exception.ActualValue.ShouldBe(TimeSpan.FromSeconds(-1));
    }

    [Fact]
    public void Should_throw_when_pertimespan_is_zero()
    {
        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => Policy.RateLimitAsync(1, TimeSpan.Zero, 1));

        // Assert
        exception.Message.ShouldStartWith("perTimeSpan must be a positive timespan");
        exception.ParamName.ShouldBe("perTimeSpan");
        exception.ActualValue.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Should_not_throw_when_pertimespan_is_positive()
    {
        // Act and Assert
        Should.NotThrow(() => Policy.RateLimitAsync(1, TimeSpan.FromTicks(1), 1));
    }
}
