namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryForeverTResultAsyncSpecs : IDisposable
{
    public WaitAndRetryForeverTResultAsyncSpecs() => SystemClock.SleepAsync = (_, _) => TaskHelper.EmptyTask;

    [Fact]
    public async Task Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
    {
        Dictionary<ResultPrimitive, TimeSpan> expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>
        {
            [ResultPrimitive.Fault] = TimeSpan.FromSeconds(2),
            [ResultPrimitive.FaultAgain] = TimeSpan.FromSeconds(4)
        };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .WaitAndRetryForeverAsync(
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                (_, timeSpan, _) =>
                {
                    actualRetryWaits.Add(timeSpan);
                    return TaskHelper.EmptyTask;
                });

        using (var enumerator = expectedRetryWaits.GetEnumerator())
        {
            await policy.ExecuteAsync(async () =>
            {
                await TaskHelper.EmptyTask;
                return enumerator.MoveNext()
                    ? enumerator.Current.Key
                    : ResultPrimitive.Undefined;
            });
        }

        actualRetryWaits.ShouldBe(expectedRetryWaits.Values);
    }

    [Fact]
    public void Should_throw_if_onRetry_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onRetry = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_) => TimeSpan.Zero, onRetry);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, int, TimeSpan> onRetryAttempts = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_) => TimeSpan.Zero, onRetryAttempts);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onRetryContext = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _) => TimeSpan.Zero, onRetryContext);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, int, TimeSpan, Context> onRetryAttemptsContext = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _) => TimeSpan.Zero, onRetryAttemptsContext);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_if_onRetryAsync_is_null()
    {
        Func<DelegateResult<ResultPrimitive>, TimeSpan, Task> onRetryAsync = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_) => TimeSpan.Zero, onRetryAsync);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetryAsync");

        Func<DelegateResult<ResultPrimitive>, int, TimeSpan, Task> onRetryAttemptsAsync = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_) => TimeSpan.Zero, onRetryAttemptsAsync);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetryAsync");

        Func<DelegateResult<ResultPrimitive>, TimeSpan, Context, Task> onRetryResultRetryAfterAsync = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _, _) => TimeSpan.Zero, onRetryResultRetryAfterAsync);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetryAsync");

        Func<DelegateResult<ResultPrimitive>, TimeSpan, Context, Task> onRetryResultTimeSpanAsync = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _, _) => TimeSpan.Zero, onRetryResultTimeSpanAsync);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetryAsync");

        Func<DelegateResult<ResultPrimitive>, int, TimeSpan, Context, Task> onRetryResultSecondsAsync = null!;
        Func<int, DelegateResult<ResultPrimitive>, Context, TimeSpan> sleepDurationProvider = (_, _, _) => TimeSpan.Zero;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProvider, onRetryResultSecondsAsync);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetryAsync");
    }

    [Fact]
    public void Should_throw_if_sleepDurationProvider_is_null()
    {
        Func<int, TimeSpan> sleepDurationProvider = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProvider);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");

        Func<int, Context, TimeSpan> sleepDurationProviderContext = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProviderContext);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProvider, (_, _) => { });

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProvider, (_, _, _) => { });

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProvider, (_, _) => TaskHelper.EmptyTask);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProvider, (_, _, _) => TaskHelper.EmptyTask);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProviderContext, (_, _, _) => TaskHelper.EmptyTask);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProviderContext, (_, _, _, _) => TaskHelper.EmptyTask);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");

        Func<int, DelegateResult<ResultPrimitive>, Context, TimeSpan> sleepDurationProviderFunc = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProviderFunc, (_, _, _) => TaskHelper.EmptyTask);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync(sleepDurationProviderFunc, (_, _, _, _) => TaskHelper.EmptyTask);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("sleepDurationProvider");
    }

    [Fact]
    public void Should_not_throw_if_arguments_are_valid()
    {
        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_) => TimeSpan.Zero);

        Should.NotThrow(policy);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _) => TimeSpan.Zero);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_) => TimeSpan.Zero, (_, _) => { });

        Should.NotThrow(policy);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_) => TimeSpan.Zero, (_, _, _) => { });

        Should.NotThrow(policy);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_) => TimeSpan.Zero, (_, _) => TaskHelper.EmptyTask);

        Should.NotThrow(policy);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_) => TimeSpan.Zero, (_, _, _) => TaskHelper.EmptyTask);

        Should.NotThrow(policy);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _) => TimeSpan.Zero, (_, _, _) => { });

        Should.NotThrow(policy);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _) => TimeSpan.Zero, (_, _, _, _) => { });

        Should.NotThrow(policy);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _) => TimeSpan.Zero, (_, _, _) => TaskHelper.EmptyTask);

        Should.NotThrow(policy);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _) => TimeSpan.Zero, (_, _, _, _) => TaskHelper.EmptyTask);

        Should.NotThrow(policy);

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryForeverAsync((_, _, _) => TimeSpan.Zero, (_, _, _, _) => TaskHelper.EmptyTask);

        Should.NotThrow(policy);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
