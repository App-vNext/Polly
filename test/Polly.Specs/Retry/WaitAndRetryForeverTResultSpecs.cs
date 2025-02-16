namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryForeverTResultSpecs : IDisposable
{
    public WaitAndRetryForeverTResultSpecs() => SystemClock.Sleep = (_, _) => { };

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
            .WaitAndRetryForever(
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                (_, timeSpan, _) => actualRetryWaits.Add(timeSpan));

        using (var enumerator = expectedRetryWaits.GetEnumerator())
        {
            policy.Execute(() => enumerator.MoveNext()
                ? enumerator.Current.Key
                : ResultPrimitive.Undefined);
        }

        actualRetryWaits.ShouldBeSubsetOf(expectedRetryWaits.Values);
        actualRetryWaits.ShouldBeInOrder();
    }

    [Fact]
    public void Should_throw_when_sleepDurationProvider_is_null()
    {
        Func<int, TimeSpan> sleepDurationProvider = null!;

        Action policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever(sleepDurationProvider);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        Func<int, Context, TimeSpan> sleepDurationProviderContext = null!;

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever(sleepDurationProviderContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever(sleepDurationProvider, (_, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever(sleepDurationProvider, (_, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever(sleepDurationProviderContext, (_, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever(sleepDurationProviderContext, (_, _, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        Func<int, DelegateResult<ResultPrimitive>, Context, TimeSpan> sleepDurationProviderResult = null!;

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever(sleepDurationProviderResult, (_, _, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_onRetry_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, TimeSpan> onRetry = null!;

        Action policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever((_) => TimeSpan.Zero, onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, int, TimeSpan> onRetryAttempts = null!;

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever((_) => TimeSpan.Zero, onRetryAttempts);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, int, TimeSpan, Context> onRetryContext = null!;

        policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever((_, _, _) => TimeSpan.Zero, onRetryContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_not_throw_when_sleepDurationProvider_is_not_null()
    {
        Action policy = () =>
            Policy.HandleResult(ResultPrimitive.Fault)
                  .WaitAndRetryForever((_) => TimeSpan.Zero);

        Should.NotThrow(policy);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
