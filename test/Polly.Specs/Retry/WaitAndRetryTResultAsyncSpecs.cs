﻿namespace Polly.Specs.Retry;

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
        Action configure = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(
                -1,
                (_) => TimeSpan.Zero,
                (_, _, _, _) => TaskHelper.EmptyTask);

        Should.Throw<ArgumentOutOfRangeException>(configure).ParamName.ShouldBe("retryCount");

        configure = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(
                -1,
                (_, _, _) => TimeSpan.Zero,
                (_, _, _, _) => TaskHelper.EmptyTask);

        Should.Throw<ArgumentOutOfRangeException>(configure).ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_not_throw_when_retry_count_is_zero()
    {
        Action configure = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(
                0,
                (_) => TimeSpan.Zero,
                (_, _, _, _) => TaskHelper.EmptyTask);

        Should.NotThrow(configure);

        configure = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(
                0,
                (_, _, _) => TimeSpan.Zero,
                (_, _, _, _) => TaskHelper.EmptyTask);

        Should.NotThrow(configure);
    }

    [Fact]
    public void Should_throw_when_onRetryAsync_is_null()
    {
        Action configure = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(
                2,
                (_, _, _) => default,
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

        Action<DelegateResult<ResultPrimitive>, TimeSpan, int, Context> onRetryAttemptsResult = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(1, (_) => TimeSpan.Zero, onRetryAttemptsResult);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onRetryResult = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(1, (_, _) => TimeSpan.Zero, onRetryResult);

        Should.Throw<ArgumentNullException>(policy).ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_not_throw_arguments_valid()
    {
        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(1, (_) => TimeSpan.Zero, (_, _, _, _) => { });

        Should.NotThrow(policy);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
