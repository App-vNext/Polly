namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryTResultSpecs : IDisposable
{
    public WaitAndRetryTResultSpecs() => SystemClock.Sleep = (_, _) => { };

    [Fact]
    public void Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
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
            .WaitAndRetry(2,
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                (_, timeSpan, _, _) => actualRetryWaits.Add(timeSpan));

        using (var enumerator = expectedRetryWaits.GetEnumerator())
        {
            policy.Execute(() => enumerator.MoveNext()
                ? enumerator.Current.Key
                : ResultPrimitive.Undefined);
        }

        actualRetryWaits.ShouldBeSubsetOf(expectedRetryWaits.Values);
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero()
    {
        Action policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(-1, (_, _, _) => default, (_, _, _, _) => { });

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(-1, (_) => default, (_, _, _, _) => { });

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_throw_when_sleepDurationProvider_is_null()
    {
        Func<int, TimeSpan> sleepDurationProvider = null!;

        Action policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(1, sleepDurationProvider, (_, _, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        Func<int, DelegateResult<ResultPrimitive>, Context, TimeSpan> sleepDurationProviderKitchenSink = null!;

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(1, sleepDurationProviderKitchenSink, (_, _, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");
    }

    [Fact]
    public void Should_not_throw_when_sleepDurationProvider_is_not_null()
    {
        Action policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(1, (_) => TimeSpan.Zero);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_retry_count_is_zero()
    {
        Action policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(0, (_, _, _) => default, (_, _, _, _) => { });

        Should.NotThrow(policy);

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(0, (_) => default, (_, _, _, _) => { });

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_throw_when_onRetry_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onRetry = null!;

        Action policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(1, (_) => TimeSpan.Zero, onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onRetryContext = null!;

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(1, (_) => TimeSpan.Zero, onRetryContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, TimeSpan, int, Context> onRetryAttempts = null!;

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(1, (_) => TimeSpan.Zero, onRetryAttempts);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(1, (_, _) => TimeSpan.Zero, onRetryContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(1, (_, _, _) => TimeSpan.Zero, onRetryContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry([], onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry([], onRetryContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry([], onRetryAttempts);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(1, (_, _, _) => TimeSpan.Zero, onRetryAttempts);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_when_sleepDurations_is_null()
    {
        IEnumerable<TimeSpan> sleepDurations = null!;

        Action policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetry(sleepDurations, (_, _, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurations");
    }

    public void Dispose() =>
        SystemClock.Reset();
}
