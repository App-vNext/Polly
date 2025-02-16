namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryTResultAsyncSpecs : IDisposable
{
    public WaitAndRetryTResultAsyncSpecs() => SystemClock.SleepAsync = (_, _) => TaskHelper.EmptyTask;

    [Fact]
    public async Task Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
    {
        Dictionary<ResultPrimitive, TimeSpan> expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>
        {
            [ResultPrimitive.Fault] = TimeSpan.FromSeconds(2),
            [ResultPrimitive.FaultAgain] = TimeSpan.FromSeconds(4),
        };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .WaitAndRetryAsync(2,
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                (_, timeSpan, _, _) =>
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

        actualRetryWaits.ShouldBeSubsetOf(expectedRetryWaits.Values);
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero()
    {
        var expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>
        {
            [ResultPrimitive.Fault] = TimeSpan.FromSeconds(2),
            [ResultPrimitive.FaultAgain] = TimeSpan.FromSeconds(4),
        };

        var actualRetryWaits = new List<TimeSpan>();

        Action configure = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(-1,
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                (_, timeSpan, _, _) =>
                {
                    actualRetryWaits.Add(timeSpan);
                    return TaskHelper.EmptyTask;
                });

        Should.Throw<ArgumentOutOfRangeException>(configure).ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_throw_when_onRetryAsync_is_null()
    {
        var expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>
        {
            [ResultPrimitive.Fault] = TimeSpan.FromSeconds(2),
            [ResultPrimitive.FaultAgain] = TimeSpan.FromSeconds(4),
        };

        Action configure = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(2,
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                null);

        Should.Throw<ArgumentNullException>(configure).ParamName.ShouldBe("onRetryAsync");
    }


    [Fact]
    public void Should_throw_when_onretry_action_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onRetry = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(1, _ => TimeSpan.Zero, onRetry);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetry");

        Func<DelegateResult<ResultPrimitive>, TimeSpan, Task> onRetryAsync = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(1, (_) => TimeSpan.Zero, onRetryAsync);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetryAsync");

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onRetryContext = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(1, (_) => TimeSpan.Zero, onRetryContext);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetry");

        Func<DelegateResult<ResultPrimitive>, TimeSpan, Context, Task> onRetryAsyncContext = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(1, (_) => TimeSpan.Zero, onRetryAsyncContext);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetryAsync");
    }

    public void Dispose() =>
        SystemClock.Reset();
}
