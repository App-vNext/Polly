using static Polly.Specs.DictionaryHelpers;

namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryForeverSpecs : IDisposable
{
    public WaitAndRetryForeverSpecs() => SystemClock.Sleep = (_, _) => { };

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_without_context()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(null, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_with_context()
    {
        Action<Exception, TimeSpan, Context> onRetry = (_, _, _) => { };

        Func<int, Context, TimeSpan> sleepDurationProvider = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(sleepDurationProvider, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_without_context()
    {
        Action<Exception, TimeSpan> nullOnRetry = null!;
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(provider, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_with_context()
    {
        Action<Exception, TimeSpan, Context> nullOnRetry = null!;
        Func<int, Context, TimeSpan> provider = (_, _) => TimeSpan.Zero;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryForever(provider, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(_ => default);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>(3))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryForever(_ => default);

        policy.Invoking(x => x.RaiseException<ArgumentException>(3))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(provider);

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryForever(provider);

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .WaitAndRetryForever(provider);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .WaitAndRetryForever(provider);

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        Func<int, TimeSpan> provider = _ => 1.Seconds();

        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .WaitAndRetryForever(provider);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        Func<int, TimeSpan> provider = _ => 1.Seconds();

        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
           .WaitAndRetryForever(provider);

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_sleep_if_no_retries()
    {
        Func<int, TimeSpan> provider = _ => 1.Seconds();

        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(provider);

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();

        totalTimeSlept.Should()
                      .Be(0);
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
            .Should()
            .ContainInOrder(expectedExceptions);
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

        retryCounts.Should()
            .ContainInOrder(expectedRetryCounts);
    }

    [Fact]
    public void Should_not_call_onretry_when_no_retries_are_performed()
    {
        Func<int, TimeSpan> provider = _ => 1.Seconds();
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(provider, (exception, _) => retryExceptions.Add(exception));

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().Throw<ArgumentException>();

        retryExceptions.Should().BeEmpty();
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_policy()
    {
        Func<int, Context, TimeSpan> provider = (_, _) => 1.Seconds();

        string? contextValue = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(
            provider,
            (_, _, context) => contextValue = context["key"].ToString());

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "original_value"));

        contextValue.Should().Be("original_value");

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "new_value"));

        contextValue.Should().Be("new_value");
    }

    [Fact]
    public void Should_calculate_retry_timespans_from_current_retry_attempt_and_timespan_provider()
    {
        var expectedRetryWaits = new[]
            {
                2.Seconds(),
                4.Seconds(),
                8.Seconds(),
                16.Seconds(),
                32.Seconds()
            };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryForever(
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (_, timeSpan) => actualRetryWaits.Add(timeSpan));

        policy.RaiseException<DivideByZeroException>(5);

        actualRetryWaits.Should()
                   .ContainInOrder(expectedRetryWaits);
    }

    [Fact]
    public void Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
    {
        Dictionary<Exception, TimeSpan> expectedRetryWaits = new Dictionary<Exception, TimeSpan>
        {
            {new DivideByZeroException(), 2.Seconds()},
            {new ArgumentNullException(), 4.Seconds()},
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

        actualRetryWaits.Should().ContainInOrder(expectedRetryWaits.Values);
    }

    [Fact]
    public void Should_be_able_to_pass_retry_duration_from_execution_to_sleepDurationProvider_via_context()
    {
        var expectedRetryDuration = 1.Seconds();
        TimeSpan? actualRetryDuration = null;

        TimeSpan defaultRetryAfter = 30.Seconds();

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

        actualRetryDuration.Should().Be(expectedRetryDuration);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
