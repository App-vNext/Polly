namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetrySpecs : IDisposable
{
    public WaitAndRetrySpecs() => SystemClock.Sleep = (_, _) => { };

    [Fact]
    public void Should_throw_when_sleep_durations_is_null_without_context()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(null, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurations");
    }

    [Fact]
    public void Should_throw_when_sleep_durations_is_null_with_context()
    {
        Action<Exception, TimeSpan, Context> onRetry = (_, _, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(null, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurations");
    }

    [Fact]
    public void Should_throw_when_sleep_durations_is_null_with_attempts_with_context()
    {
        Action<Exception, TimeSpan, int, Context> onRetry = (_, _, _, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(null, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurations");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_without_context()
    {
        Action<Exception, TimeSpan> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(Enumerable.Empty<TimeSpan>(), nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_with_context()
    {
        Action<Exception, TimeSpan, Context> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(Enumerable.Empty<TimeSpan>(), nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_with_attempts_with_context()
    {
        Action<Exception, TimeSpan, int, Context> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(Enumerable.Empty<TimeSpan>(), nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        policy.Invoking(x => x.RaiseException<DivideByZeroException>(3))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        policy.Invoking(x => x.RaiseException<ArgumentException>(3))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_thrown_less_number_of_times_than_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        policy.Invoking(x => x.RaiseException<DivideByZeroException>(2))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_then_times_as_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        policy.Invoking(x => x.RaiseException<ArgumentException>(2))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_throw_when_specified_exception_thrown_more_times_than_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        policy.Invoking(x => x.RaiseException<DivideByZeroException>(3 + 1))
              .Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        policy.Invoking(x => x.RaiseException<ArgumentException>(3 + 1))
              .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(Enumerable.Empty<TimeSpan>());

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_the_specified_exception_type_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>());

        await policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
              .Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetry(Enumerable.Empty<TimeSpan>());

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>());

        await policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
              .Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .WaitAndRetry(Enumerable.Empty<TimeSpan>());

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .WaitAndRetry(Enumerable.Empty<TimeSpan>());

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .WaitAndRetry(new[]
            {
               1.Seconds()
            });

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().NotThrow();
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_predicate_is_satisfied_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .WaitAndRetryAsync(new[]
            {
               1.Seconds()
            });

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().NotThrowAsync();
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .WaitAndRetry(new[]
            {
               1.Seconds()
            });

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().NotThrow();
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .WaitAndRetryAsync(new[]
            {
               1.Seconds()
            });

        await policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
              .Should().NotThrowAsync();
    }

    [Fact]
    public void Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_same_number_of_times_as_there_are_sleep_durations()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        policy.RaiseException<DivideByZeroException>(3);

        totalTimeSlept.Should()
                      .Be(1 + 2 + 3);
    }

    [Fact]
    public void Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_more_number_of_times_than_there_are_sleep_durations()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        policy.Invoking(x => x.RaiseException<DivideByZeroException>(3 + 1))
              .Should().Throw<DivideByZeroException>();

        totalTimeSlept.Should()
                      .Be(1 + 2 + 3);
    }

    [Fact]
    public void Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_less_number_of_times_than_there_are_sleep_durations()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        policy.RaiseException<DivideByZeroException>(2);

        totalTimeSlept.Should()
                      .Be(1 + 2);
    }

    [Fact]
    public void Should_not_sleep_if_no_retries()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(Enumerable.Empty<TimeSpan>());

        SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();

        totalTimeSlept.Should()
                      .Be(0);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_timespan()
    {
        var expectedRetryWaits = new[]
            {
                1.Seconds(),
                2.Seconds(),
                3.Seconds()
            };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            }, (_, timeSpan) => actualRetryWaits.Add(timeSpan));

        policy.RaiseException<DivideByZeroException>(3);

        actualRetryWaits.Should()
                   .ContainInOrder(expectedRetryWaits);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_exception()
    {
        var expectedExceptions = new[] { "Exception #1", "Exception #2", "Exception #3" };
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            }, (exception, _) => retryExceptions.Add(exception));

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

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            }, (_, _, retryCount, _) => retryCounts.Add(retryCount));

        policy.RaiseException<DivideByZeroException>(3);

        retryCounts.Should()
                   .ContainInOrder(expectedRetryCounts);
    }

    [Fact]
    public void Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(Enumerable.Empty<TimeSpan>(), (exception, _) => retryExceptions.Add(exception));

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().Throw<ArgumentException>();

        retryExceptions.Should()
                   .BeEmpty();
    }

    [Fact]
    public void Should_create_new_state_for_each_call_to_policy()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
                1.Seconds()
            });

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().NotThrow();

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().NotThrow();
    }

    [Fact]
    public void Should_call_onretry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
                1.Seconds(),
                2.Seconds(),
                3.Seconds()
            }, (_, _, context) => contextData = context);

        policy.RaiseException<DivideByZeroException>(
            new Dictionary<string, object> { { "key1", "value1" }, { "key2", "value2" } });

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[]
            {
                1.Seconds()
            },
            (_, _, context) => contextValue = context["key"].ToString());

        policy.RaiseException<DivideByZeroException>(
            new Dictionary<string, object> { { "key", "original_value" } });

        contextValue.Should().Be("original_value");

        policy.RaiseException<DivideByZeroException>(
            new Dictionary<string, object> { { "key", "new_value" } });

        contextValue.Should().Be("new_value");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_without_context()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(-1, _ => default, onRetry);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
              .ParamName.Should().Be("retryCount");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_with_context()
    {
        Action<Exception, TimeSpan, Context> onRetry = (_, _, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(-1, _ => default, onRetry);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
              .ParamName.Should().Be("retryCount");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_with_attempts_with_context()
    {
        Action<Exception, TimeSpan, int, Context> onRetry = (_, _, _, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(-1, _ => default, onRetry);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
              .ParamName.Should().Be("retryCount");
    }

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_without_context()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(1, null, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_with_context()
    {
        Action<Exception, TimeSpan, Context> onRetry = (_, _, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(1, (Func<int, TimeSpan>)null!, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_with_attempts_with_context()
    {
        Action<Exception, TimeSpan, int, Context> onRetry = (_, _, _, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(1, (Func<int, TimeSpan>)null!, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_without_context_when_using_provider_overload()
    {
        Action<Exception, TimeSpan> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(1, _ => default, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_with_context_when_using_provider_overload()
    {
        Action<Exception, TimeSpan, Context> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(1, _ => default, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_with_attempts_with_context_when_using_provider_overload()
    {
        Action<Exception, TimeSpan, int, Context> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetry(1, _ => default, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
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
            .WaitAndRetry(5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (_, timeSpan) => actualRetryWaits.Add(timeSpan));

        policy.RaiseException<DivideByZeroException>(5);

        actualRetryWaits.Should()
                   .ContainInOrder(expectedRetryWaits);
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
                onRetry: (_, _, _, _) =>
                {
                });

        policy.RaiseException(exceptionInstance);

        capturedExceptionInstance.Should().Be(exceptionInstance);
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
            new Dictionary<string, object> { { "RetryAfter", defaultRetryAfter } }); // Can also set an initial value for RetryAfter, in the Context passed into the call.

        actualRetryDuration.Should().Be(expectedRetryDuration);
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_without_context()
    {
        bool retryInvoked = false;

        Action<Exception, TimeSpan> onRetry = (_, _) => { retryInvoked = true; };

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(0, _ => TimeSpan.FromSeconds(1), onRetry);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().Throw<DivideByZeroException>();

        retryInvoked.Should().BeFalse();
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_with_context()
    {
        bool retryInvoked = false;

        Action<Exception, TimeSpan, Context> onRetry = (_, _, _) => { retryInvoked = true; };

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(0, _ => TimeSpan.FromSeconds(1), onRetry);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().Throw<DivideByZeroException>();

        retryInvoked.Should().BeFalse();
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_with_attempts_with_context()
    {
        bool retryInvoked = false;

        Action<Exception, TimeSpan, int, Context> onRetry = (_, _, _, _) => { retryInvoked = true; };

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(0, _ => TimeSpan.FromSeconds(1), onRetry);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().Throw<DivideByZeroException>();

        retryInvoked.Should().BeFalse();
    }

    #region Sync cancellation tests

    [Fact]
    public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(0);
    }

    [Fact]
    public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(2);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(2);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_last_retry_execution_when_user_delegate_does_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public void Should_report_faulting_from_faulting_last_retry_execution_when_user_delegate_does_not_observe_cancellation_raised_during_last_retry()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().Throw<DivideByZeroException>();
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public void Should_honour_cancellation_immediately_during_wait_phase_of_waitandretry()
    {
        SystemClock.Sleep = (timeSpan, ct) => Task.Delay(timeSpan, ct).Wait(ct);

        TimeSpan shimTimeSpan = TimeSpan.FromSeconds(1); // Consider increasing shimTimeSpan if test fails transiently in different environments.
        TimeSpan retryDelay = shimTimeSpan + shimTimeSpan + shimTimeSpan;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetry(new[] { retryDelay });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Stopwatch watch = new Stopwatch();
        watch.Start();

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

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                    .Should().Throw<OperationCanceledException>()
                    .And.CancellationToken.Should().Be(cancellationToken);
        }

        watch.Stop();

        attemptsInvoked.Should().Be(1);

        watch.Elapsed.Should().BeLessThan(retryDelay);
        watch.Elapsed.Should().BeCloseTo(shimTimeSpan, precision: TimeSpan.FromMilliseconds((int)shimTimeSpan.TotalMilliseconds / 2));  // Consider increasing shimTimeSpan, or loosening precision, if test fails transiently in different environments.
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
                .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() },
                (_, _) =>
                {
                    cancellationTokenSource.Cancel();
                });

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_execute_func_returning_value_when_cancellationToken_not_cancelled()
    {
        var policy = Policy
           .Handle<DivideByZeroException>()
           .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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
            policy.Invoking(x => result = x.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
            .Should().NotThrow();
        }

        result.Should().BeTrue();

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_honour_and_report_cancellation_during_func_execution()
    {
        var policy = Policy
           .Handle<DivideByZeroException>()
           .WaitAndRetry(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

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

            policy.Invoking(x => result = x.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
            .Should().Throw<OperationCanceledException>().And.CancellationToken.Should().Be(cancellationToken);
        }

        result.Should().Be(null);

        attemptsInvoked.Should().Be(1);
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
