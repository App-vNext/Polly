namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetrySpecs : IDisposable
{
    public WaitAndRetrySpecs() => SystemClock.Sleep = (_, _) => { };

    [Fact]
    public void Should_throw_when_sleep_durations_is_null_without_context()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(null, onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurations");
    }

    [Fact]
    public void Should_throw_when_sleep_durations_is_null_with_context()
    {
        Action<Exception, TimeSpan, Context> onRetry = (_, _, _) => { };

        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(null, onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurations");
    }

    [Fact]
    public void Should_throw_when_sleep_durations_is_null_with_attempts_with_context()
    {
        Action<Exception, TimeSpan, int, Context> onRetry = (_, _, _, _) => { };

        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(null, onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurations");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null()
    {
        Action<Exception, TimeSpan> onRetry = null!;

        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry([], onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        Action<Exception, TimeSpan, Context> onRetryContext = null!;

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry([], onRetryContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        Action<Exception, TimeSpan, int, Context> onRetryAttemptsContext = null!;

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry([], onRetryAttemptsContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, _ => default, onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, _ => default, onRetryContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, _ => default, onRetryAttemptsContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, (_, _) => default, onRetryContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, (_, _) => default, onRetryAttemptsContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
               TimeSpan.FromSeconds(1),
               TimeSpan.FromSeconds(2),
               TimeSpan.FromSeconds(3)
            ]);

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>(3));
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetry(
            [
               TimeSpan.FromSeconds(1),
               TimeSpan.FromSeconds(2),
               TimeSpan.FromSeconds(3)
            ]);

        Should.NotThrow(() => policy.RaiseException<ArgumentException>(3));
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_thrown_less_number_of_times_than_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
               TimeSpan.FromSeconds(1),
               TimeSpan.FromSeconds(2),
               TimeSpan.FromSeconds(3)
            ]);

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>(2));
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_then_times_as_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetry(
            [
               TimeSpan.FromSeconds(1),
               TimeSpan.FromSeconds(2),
               TimeSpan.FromSeconds(3)
            ]);

        Should.NotThrow(() => policy.RaiseException<ArgumentException>(2));
    }

    [Fact]
    public void Should_throw_when_specified_exception_thrown_more_times_than_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
               TimeSpan.FromSeconds(1),
               TimeSpan.FromSeconds(2),
               TimeSpan.FromSeconds(3)
            ]);

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>(3 + 1));
    }

    [Fact]
    public void Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetry(
            [
               TimeSpan.FromSeconds(1),
               TimeSpan.FromSeconds(2),
               TimeSpan.FromSeconds(3)
            ]);

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>(3 + 1));
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry([]);

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_the_specified_exception_type_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync([]);

        await Should.ThrowAsync<NullReferenceException>(() => policy.RaiseExceptionAsync<NullReferenceException>());
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetry([]);

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryAsync([]);

        await Should.ThrowAsync<NullReferenceException>(() => policy.RaiseExceptionAsync<NullReferenceException>());
    }

    [Fact]
    public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .WaitAndRetry([]);

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .WaitAndRetry([]);

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>());
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .WaitAndRetry([TimeSpan.FromSeconds(1)]);

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_predicate_is_satisfied_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .WaitAndRetryAsync([TimeSpan.FromSeconds(1)]);

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .WaitAndRetry([TimeSpan.FromSeconds(1)]);

        Should.NotThrow(() => policy.RaiseException<ArgumentException>());
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .WaitAndRetryAsync([TimeSpan.FromSeconds(1)]);

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<ArgumentException>());
    }

    [Fact]
    public void Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_same_number_of_times_as_there_are_sleep_durations()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
               TimeSpan.FromSeconds(1),
               TimeSpan.FromSeconds(2),
               TimeSpan.FromSeconds(3)
            ]);

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        policy.RaiseException<DivideByZeroException>(3);

        totalTimeSlept.ShouldBe(1 + 2 + 3);
    }

    [Fact]
    public void Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_more_number_of_times_than_there_are_sleep_durations()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
               TimeSpan.FromSeconds(1),
               TimeSpan.FromSeconds(2),
               TimeSpan.FromSeconds(3)
            ]);

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>(3 + 1));

        totalTimeSlept.ShouldBe(1 + 2 + 3);
    }

    [Fact]
    public void Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_less_number_of_times_than_there_are_sleep_durations()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
               TimeSpan.FromSeconds(1),
               TimeSpan.FromSeconds(2),
               TimeSpan.FromSeconds(3)
            ]);

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        policy.RaiseException<DivideByZeroException>(2);

        totalTimeSlept.ShouldBe(1 + 2);
    }

    [Fact]
    public void Should_not_sleep_if_no_retries()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry([]);

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());

        totalTimeSlept.ShouldBe(0);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_timespan()
    {
        var expectedRetryWaits = new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(3)
        };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ], (_, timeSpan) => actualRetryWaits.Add(timeSpan));

        policy.RaiseException<DivideByZeroException>(3);

        actualRetryWaits.ShouldBeSubsetOf(expectedRetryWaits);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_exception()
    {
        var expectedExceptions = new[] { "Exception #1", "Exception #2", "Exception #3" };
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ], (exception, _) => retryExceptions.Add(exception));

        policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

        retryExceptions.Select((p) => p.HelpLink).ShouldBe(expectedExceptions);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
    {
        var expectedRetryCounts = new[] { 1, 2, 3 };
        var retryCounts = new List<int>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ], (_, _, retryCount, _) => retryCounts.Add(retryCount));

        policy.RaiseException<DivideByZeroException>(3);

        retryCounts.ShouldBe(expectedRetryCounts);
    }

    [Fact]
    public void Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry([], (exception, _) => retryExceptions.Add(exception));

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>());

        retryExceptions.ShouldBeEmpty();
    }

    [Fact]
    public void Should_create_new_state_for_each_call_to_policy()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry([TimeSpan.FromSeconds(1)]);

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>());
        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public void Should_call_onretry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ], (_, _, context) => contextData = context);

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key1", "value1", "key2", "value2"));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry([TimeSpan.FromSeconds(1)],
            (_, _, context) => contextValue = context["key"].ToString());

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "original_value"));

        contextValue.ShouldBe("original_value");

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "new_value"));

        contextValue.ShouldBe("new_value");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(-1, _ => default, onRetry);

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");

        Action<Exception, TimeSpan, Context> onRetryContext = (_, _, _) => { };

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(-1, _ => default, onRetryContext);

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_not_throw_if_arguments_are_valid()
    {
        var policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, _ => TimeSpan.Zero);

        Should.NotThrow(policy);

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, (_, _) => TimeSpan.Zero);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_retry_count_is_zero()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(0, _ => default, onRetry);

        Should.NotThrow(policy);

        Action<Exception, TimeSpan, Context> onRetryContext = (_, _, _) => { };

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(0, _ => default, onRetryContext);

        Should.NotThrow(policy);

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(0, (_, _, _) => TimeSpan.Zero, (_, _, _, _) => { });

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_with_attempts_with_context_and_attempt()
    {
        Func<int, Exception, Context, TimeSpan> sleepDurationProvider = (_, _, _) => TimeSpan.Zero;
        Action<Exception, TimeSpan, int, Context> onRetry = (_, _, _, _) => { };

        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(-1, sleepDurationProvider, onRetry);

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_with_attempts_with_context()
    {
        Action<Exception, TimeSpan, int, Context> onRetry = (_, _, _, _) => { };

        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(-1, _ => default, onRetry);

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null()
    {
        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, null, (_, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, (Func<int, TimeSpan>)null!, (_, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, (Func<int, TimeSpan>)null!, (_, _, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");

        policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, (Func<int, Exception, Context, TimeSpan>)null!, (_, _, _, _) => { });

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_onRetry_is_null()
    {
        Action policy = () =>
            Policy.Handle<DivideByZeroException>()
                  .WaitAndRetry(1, (_, _, _) => TimeSpan.Zero, null);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
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
            .WaitAndRetry(5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (_, timeSpan) => actualRetryWaits.Add(timeSpan));

        policy.RaiseException<DivideByZeroException>(5);

        actualRetryWaits.ShouldBe(expectedRetryWaits);
    }

    [Fact]
    public void Should_be_able_to_pass_handled_exception_to_sleepdurationprovider()
    {
        object? capturedExceptionInstance = null;

        DivideByZeroException exceptionInstance = new DivideByZeroException();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(5,
                sleepDurationProvider: (_, ex, _) =>
                {
                    capturedExceptionInstance = ex;
                    return TimeSpan.FromMilliseconds(0);
                },
                onRetry: (_, _, _, _) => { });

        policy.RaiseException(exceptionInstance);

        capturedExceptionInstance.ShouldBe(exceptionInstance);
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
            .WaitAndRetry(2,
                (_, exc, _) => expectedRetryWaits[exc],
                (_, timeSpan, _, _) => actualRetryWaits.Add(timeSpan));

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

        var defaultRetryAfter = TimeSpan.FromSeconds(30);

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(1,
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

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_without_context()
    {
        bool retryInvoked = false;

        Action<Exception, TimeSpan> onRetry = (_, _) => retryInvoked = true;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(0, _ => TimeSpan.FromSeconds(1), onRetry);

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>());

        retryInvoked.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_with_context()
    {
        bool retryInvoked = false;

        Action<Exception, TimeSpan, Context> onRetry = (_, _, _) => retryInvoked = true;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(0, _ => TimeSpan.FromSeconds(1), onRetry);

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>());

        retryInvoked.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_with_attempts_with_context()
    {
        bool retryInvoked = false;

        Action<Exception, TimeSpan, int, Context> onRetry = (_, _, _, _) => retryInvoked = true;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(0, _ => TimeSpan.FromSeconds(1), onRetry);

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>());

        retryInvoked.ShouldBeFalse();
    }

    #region Sync cancellation tests

    [Fact]
    public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);
    }

    [Fact]
    public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 2,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(2);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 2,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(2);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_last_retry_execution_when_user_delegate_does_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1 + 3,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1 + 3);
    }

    [Fact]
    public void Should_report_faulting_from_faulting_last_retry_execution_when_user_delegate_does_not_observe_cancellation_raised_during_last_retry()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1 + 3,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<DivideByZeroException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1 + 3);
    }

    [Fact]
    public void Should_honour_cancellation_immediately_during_wait_phase_of_waitandretry()
    {
        SystemClock.Sleep = (timeSpan, ct) => Task.Delay(timeSpan, ct).Wait(ct);

        TimeSpan shimTimeSpan = TimeSpan.FromSeconds(1);
        TimeSpan retryDelay = shimTimeSpan + shimTimeSpan + shimTimeSpan;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry([retryDelay]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        var watch = Stopwatch.StartNew();

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 1,
            AttemptDuringWhichToCancel = null, // Cancellation invoked after delay - see below.
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            cancellationTokenSource.CancelAfter(shimTimeSpan);

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        watch.Stop();

        attemptsInvoked.ShouldBe(1);

        watch.Elapsed.ShouldBeLessThan(retryDelay);
        watch.Elapsed.ShouldBe(shimTimeSpan, TimeSpan.FromMilliseconds(shimTimeSpan.TotalMilliseconds / 2));
    }

    [Fact]
    public void Should_report_cancellation_after_faulting_action_execution_and_cancel_further_retries_if_onRetry_invokes_cancellation()
    {
        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = null, // Cancellation during onRetry instead - see above.
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(
                [
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                ],
                (_, _) => cancellationTokenSource.Cancel());

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public void Should_execute_func_returning_value_when_cancellationToken_not_cancelled()
    {
        var policy = Policy
           .Handle<DivideByZeroException>()
           .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            Should.NotThrow(() => result = policy.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true));
        }

        result.ShouldNotBeNull();
        result.Value.ShouldBeTrue();

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public void Should_honour_and_report_cancellation_during_func_execution()
    {
        var policy = Policy
           .Handle<DivideByZeroException>()
           .WaitAndRetry(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            ]);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<OperationCanceledException>(() => result = policy.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        result.ShouldBe(null);

        attemptsInvoked.ShouldBe(1);
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
