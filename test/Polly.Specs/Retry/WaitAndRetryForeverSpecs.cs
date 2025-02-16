namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryForeverSpecs : IDisposable
{
    public WaitAndRetryForeverSpecs() => SystemClock.Sleep = (_, _) => { };

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_without_context()
    {
        Func<int, Context, TimeSpan> sleepDurationProvider = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(sleepDurationProvider);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_not_when_sleep_duration_provider_is_not_null_with_context()
    {
        Func<int, Context, TimeSpan> sleepDurationProvider = (_, _) => TimeSpan.Zero;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(sleepDurationProvider);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_without_context_with_onretry()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(null, onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_with_context()
    {
        Action<Exception, TimeSpan, Context> onRetry = (_, _, _) => { };

        Func<int, Context, TimeSpan> sleepDurationProvider = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(sleepDurationProvider, onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_with_context_and_onretry_with_attempt()
    {
        Action<Exception, int, TimeSpan, Context> onRetry = (_, _, _, _) => { };

        Func<int, Context, TimeSpan> sleepDurationProvider = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(sleepDurationProvider, onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_without_context()
    {
        Action<Exception, TimeSpan> nullOnRetry = null!;
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(provider, nullOnRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_with_context()
    {
        Action<Exception, TimeSpan, Context> nullOnRetry = null!;
        Func<int, Context, TimeSpan> provider = (_, _) => TimeSpan.Zero;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(provider, nullOnRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(_ => default);

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>(3));
    }

    [Fact]
    public void Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryForever(_ => default);

        Should.NotThrow(() => policy.RaiseException<ArgumentException>(3));
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(provider);

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryForever(provider);

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());
    }

    [Fact]
    public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .WaitAndRetryForever(provider);

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .WaitAndRetryForever(provider);

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>());
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.FromSeconds(1);

        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .WaitAndRetryForever(provider);

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.FromSeconds(1);

        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
           .WaitAndRetryForever(provider);

        Should.NotThrow(() => policy.RaiseException<ArgumentException>());
    }

    [Fact]
    public void Should_not_sleep_if_no_retries()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.FromSeconds(1);

        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(provider);

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());

        totalTimeSlept.ShouldBe(0);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_exception()
    {
        var expectedExceptions = new[] { "Exception #1", "Exception #2", "Exception #3" };
        var retryExceptions = new List<Exception>();
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(provider, (exception, _) => retryExceptions.Add(exception));

        policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

        retryExceptions
            .Select(x => x.HelpLink)
            .ShouldBe(expectedExceptions);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
    {
        var expectedRetryCounts = new[] { 1, 2, 3 };
        var retryCounts = new List<int>();
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(provider, (_, retryCount, _) => retryCounts.Add(retryCount));

        policy.RaiseException<DivideByZeroException>(3);

        retryCounts.ShouldBe(expectedRetryCounts);
    }

    [Fact]
    public void Should_not_call_onretry_when_no_retries_are_performed()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.FromSeconds(1);
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(provider, (exception, _) => retryExceptions.Add(exception));

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>());

        retryExceptions.ShouldBeEmpty();
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_policy()
    {
        Func<int, Context, TimeSpan> provider = (_, _) => TimeSpan.FromSeconds(1);

        string? contextValue = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(
            provider,
            (_, _, context) => contextValue = context["key"].ToString());

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "original_value"));

        contextValue.ShouldBe("original_value");

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "new_value"));

        contextValue.ShouldBe("new_value");
    }

    [Fact]
    public void Should_calculate_retry_timespans_from_current_retry_attempt_and_timespan_provider()
    {
        var expectedRetryWaits = new[]
        {
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(8),
            TimeSpan.FromSeconds(16),
            TimeSpan.FromSeconds(32)
        };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (_, timeSpan) => actualRetryWaits.Add(timeSpan));

        policy.RaiseException<DivideByZeroException>(5);

        actualRetryWaits.ShouldBe(expectedRetryWaits);
    }

    [Fact]
    public void Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
    {
        Dictionary<Exception, TimeSpan> expectedRetryWaits = new Dictionary<Exception, TimeSpan>
        {
            [new DivideByZeroException()] = TimeSpan.FromSeconds(2),
            [new ArgumentNullException()] = TimeSpan.FromSeconds(4),
        };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryForever(
                (_, exc, _) => expectedRetryWaits[exc],
                (_, timeSpan, _) => actualRetryWaits.Add(timeSpan));

        using (var enumerator = expectedRetryWaits.GetEnumerator())
        {
            policy.Execute(() =>
            {
                if (enumerator.MoveNext())
                {
                    throw enumerator.Current.Key;
                }
            });
        }

        actualRetryWaits.ShouldBe(expectedRetryWaits.Values);
    }

    [Fact]
    public void Should_be_able_to_pass_retry_duration_from_execution_to_sleepDurationProvider_via_context()
    {
        var expectedRetryDuration = TimeSpan.FromSeconds(1);
        TimeSpan? actualRetryDuration = null;

        TimeSpan defaultRetryAfter = TimeSpan.FromSeconds(30);

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(
                sleepDurationProvider: (_, context) => context.ContainsKey("RetryAfter") ? (TimeSpan)context["RetryAfter"] : defaultRetryAfter, // Set sleep duration from Context, when available.
                onRetry: (_, timeSpan, _) => actualRetryDuration = timeSpan); // Capture the actual sleep duration that was used, for test verification purposes.

        bool failedOnce = false;
        policy.Execute(context =>
            {
                // Run some remote call; maybe it returns a RetryAfter header, which we can pass back to the sleepDurationProvider, via the context.
                context["RetryAfter"] = expectedRetryDuration;

                if (!failedOnce)
                {
                    failedOnce = true;
                    throw new DivideByZeroException();
                }
            },
            CreateDictionary("RetryAfter", defaultRetryAfter)); // Can also set an initial value for RetryAfter, in the Context passed into the call.

        actualRetryDuration.ShouldBe(expectedRetryDuration);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
