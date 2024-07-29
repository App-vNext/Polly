using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryAsyncSpecs : IDisposable
{
    public WaitAndRetryAsyncSpecs() => SystemClock.SleepAsync = (_, _) => TaskHelper.EmptyTask;

    [Fact]
    public void Should_throw_when_sleep_durations_is_null_without_context()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(null, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurations");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_without_context()
    {
        Action<Exception, TimeSpan> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>(), nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_exception_timespan_context_is_null_with_sleep_durations()
    {
        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>(), default(Action<Exception, TimeSpan, Context>));

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_exception_timespan_int_context_is_null_with_sleep_duration_provider_int_timespan()
    {
        Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(3, provider, default(Action<Exception, TimeSpan, int, Context>));

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_exception_timespan_int_context_is_null_with_sleep_durations()
    {
        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>(), default(Action<Exception, TimeSpan, int, Context>));

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_exception_timespan_context_is_null_with_sleep_duration_provider()
    {
        Func<int, Context, TimeSpan> provider = (_, _) => 1.Seconds();

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(3, provider, default(Action<Exception, TimeSpan, Context>));

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_exception_timespan_int_context_is_null_with_sleep_duration_provider()
    {
        Func<int, Context, TimeSpan> provider = (_, _) => 1.Seconds();

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(3, provider, default(Action<Exception, TimeSpan, int, Context>));

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(3))
              .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        await policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>(3))
              .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_thrown_less_number_of_times_than_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(2))
              .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_of_times_than_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        await policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>(2))
              .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_throw_when_specified_exception_thrown_more_times_than_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(3 + 1))
              .Should().ThrowAsync<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_than_there_are_sleep_durations()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        await policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>(3 + 1))
              .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>());

        await policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
              .Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>());

        await policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
              .Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>());

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>());

        await policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
              .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_predicate_is_satisfied()
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
    public async Task Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
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
    public async Task Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_same_number_of_times_as_there_are_sleep_durations()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        SystemClock.SleepAsync = (span, _) =>
        {
            totalTimeSlept += span.Seconds;
            return TaskHelper.EmptyTask;
        };

        await policy.RaiseExceptionAsync<DivideByZeroException>(3);

        totalTimeSlept.Should()
                      .Be(1 + 2 + 3);
    }

    [Fact]
    public async Task Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_more_number_of_times_than_there_are_sleep_durations()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        SystemClock.SleepAsync = (span, _) =>
        {
            totalTimeSlept += span.Seconds;
            return TaskHelper.EmptyTask;
        };

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(3 + 1))
              .Should().ThrowAsync<DivideByZeroException>();

        totalTimeSlept.Should()
                      .Be(1 + 2 + 3);
    }

    [Fact]
    public async Task Should_sleep_for_the_specified_duration_each_retry_when_specified_exception_thrown_less_number_of_times_than_there_are_sleep_durations()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            });

        SystemClock.SleepAsync = (span, _) =>
        {
            totalTimeSlept += span.Seconds;
            return TaskHelper.EmptyTask;
        };

        await policy.RaiseExceptionAsync<DivideByZeroException>(2);

        totalTimeSlept.Should()
                      .Be(1 + 2);
    }

    [Fact]
    public async Task Should_not_sleep_if_no_retries()
    {
        var totalTimeSlept = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>());

        SystemClock.SleepAsync = (span, _) =>
        {
            totalTimeSlept += span.Seconds;
            return TaskHelper.EmptyTask;
        };

        await policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
              .Should().ThrowAsync<NullReferenceException>();

        totalTimeSlept.Should()
                      .Be(0);
    }

    [Fact]
    public async Task Should_call_onretry_on_each_retry_with_the_current_timespan()
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
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            }, (_, timeSpan) => actualRetryWaits.Add(timeSpan));

        await policy.RaiseExceptionAsync<DivideByZeroException>(3);

        actualRetryWaits.Should()
                   .ContainInOrder(expectedRetryWaits);
    }

    [Fact]
    public async Task Should_call_onretry_on_each_retry_with_the_current_exception()
    {
        var expectedExceptions = new[] { "Exception #1", "Exception #2", "Exception #3" };
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            }, (exception, _) => retryExceptions.Add(exception));

        await policy.RaiseExceptionAsync<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

        retryExceptions
            .Select(x => x.HelpLink)
            .Should()
            .ContainInOrder(expectedExceptions);
    }

    [Fact]
    public async Task Should_call_onretry_on_each_retry_with_the_current_retry_count()
    {
        var expectedRetryCounts = new[] { 1, 2, 3 };
        var retryCounts = new List<int>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
               1.Seconds(),
               2.Seconds(),
               3.Seconds()
            }, (_, _, retryCount, _) => retryCounts.Add(retryCount));

        await policy.RaiseExceptionAsync<DivideByZeroException>(3);

        retryCounts.Should()
                   .ContainInOrder(expectedRetryCounts);
    }

    [Fact]
    public async Task Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(Enumerable.Empty<TimeSpan>(), (exception, _) => retryExceptions.Add(exception));

        await policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
              .Should().ThrowAsync<ArgumentException>();

        retryExceptions.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_create_new_state_for_each_call_to_policy()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
                1.Seconds()
            });

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().NotThrowAsync();

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_call_onretry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
                1.Seconds(),
                2.Seconds(),
                3.Seconds()
            }, (_, _, context) => contextData = context);

        await policy.RaiseExceptionAsync<DivideByZeroException>(
            new Dictionary<string, object> { { "key1", "value1" }, { "key2", "value2" } });

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_data()
    {
        Context? capturedContext = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
                1.Seconds(),
                2.Seconds(),
                3.Seconds()
            }, (_, _, context) => capturedContext = context);
        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>()).Should().NotThrowAsync();

        capturedContext.Should()
                       .BeEmpty();
    }

    [Fact]
    public async Task Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[]
            {
                1.Seconds()
            },
            (_, _, context) => contextValue = context["key"].ToString());

        await policy.RaiseExceptionAsync<DivideByZeroException>(
            new Dictionary<string, object> { { "key", "original_value" } });

        contextValue.Should().Be("original_value");

        await policy.RaiseExceptionAsync<DivideByZeroException>(
            new Dictionary<string, object> { { "key", "new_value" } });

        contextValue.Should().Be("new_value");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_without_context()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(-1, _ => TimeSpan.Zero, onRetry);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
              .ParamName.Should().Be("retryCount");
    }

    [Fact]
    public void Should_throw_when_sleep_duration_provider_is_null_without_context()
    {
        Action<Exception, TimeSpan> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(1, null, onRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("sleepDurationProvider");
    }

    [Fact]
    public void Should_throw_when_onretry_action_is_null_without_context_when_using_provider_overload()
    {
        Action<Exception, TimeSpan> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .WaitAndRetryAsync(1, _ => TimeSpan.Zero, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public async Task Should_calculate_retry_timespans_from_current_retry_attempt_and_timespan_provider()
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
            .WaitAndRetryAsync(5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (_, timeSpan) => actualRetryWaits.Add(timeSpan));

        await policy.RaiseExceptionAsync<DivideByZeroException>(5);

        actualRetryWaits.Should()
                   .ContainInOrder(expectedRetryWaits);
    }

    [Fact]
    public async Task Should_be_able_to_pass_handled_exception_to_sleepdurationprovider()
    {
        object? capturedExceptionInstance = null;

        DivideByZeroException exceptionInstance = new DivideByZeroException();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(5,
                sleepDurationProvider: (_, ex, _) =>
                {
                    capturedExceptionInstance = ex;
                    return TimeSpan.FromMilliseconds(0);
                },
                onRetryAsync: (_, _, _, _) => TaskHelper.EmptyTask);

        await policy.RaiseExceptionAsync(exceptionInstance);

        capturedExceptionInstance.Should().Be(exceptionInstance);
    }

    [Fact]
    public async Task Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
    {
        Dictionary<Exception, TimeSpan> expectedRetryWaits = new Dictionary<Exception, TimeSpan>
        {
            {new DivideByZeroException(), 2.Seconds()},
            {new ArgumentNullException(), 4.Seconds()},
        };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(2,
                sleepDurationProvider: (_, exc, _) => expectedRetryWaits[exc],
                onRetryAsync: (_, timeSpan, _, _) =>
                {
                    actualRetryWaits.Add(timeSpan);
                    return TaskHelper.EmptyTask;
                });

        using (var enumerator = expectedRetryWaits.GetEnumerator())
        {
            await policy.ExecuteAsync(() => enumerator.MoveNext()
                ? throw enumerator.Current.Key
                : TaskHelper.EmptyTask);
        }

        actualRetryWaits.Should().ContainInOrder(expectedRetryWaits.Values);
    }

    [Fact]
    public async Task Should_be_able_to_pass_retry_duration_from_execution_to_sleepDurationProvider_via_context()
    {
        var expectedRetryDuration = 1.Seconds();
        TimeSpan? actualRetryDuration = null;

        TimeSpan defaultRetryAfter = 30.Seconds();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(1,
                sleepDurationProvider: (_, context) => context.ContainsKey("RetryAfter") ? (TimeSpan)context["RetryAfter"] : defaultRetryAfter, // Set sleep duration from Context, when available.
                onRetry: (_, timeSpan, _) => actualRetryDuration = timeSpan); // Capture the actual sleep duration that was used, for test verification purposes.

        bool failedOnce = false;
        await policy.ExecuteAsync(async (context, _) =>
        {
            await TaskHelper.EmptyTask; // Run some remote call; maybe it returns a RetryAfter header, which we can pass back to the sleepDurationProvider, via the context.
            context["RetryAfter"] = expectedRetryDuration;

            if (!failedOnce)
            {
                failedOnce = true;
                throw new DivideByZeroException();
            }
        },
            new Dictionary<string, object> { { "RetryAfter", defaultRetryAfter } }, // Can also set an initial value for RetryAfter, in the Context passed into the call.
            CancellationToken.None);

        actualRetryDuration.Should().Be(expectedRetryDuration);
    }

    [Fact]
    public async Task Should_not_call_onretry_when_retry_count_is_zero()
    {
        bool retryInvoked = false;

        Action<Exception, TimeSpan> onRetry = (_, _) => { retryInvoked = true; };

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(0, _ => TimeSpan.FromSeconds(1), onRetry);

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().ThrowAsync<DivideByZeroException>();

        retryInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task Should_wait_asynchronously_for_async_onretry_delegate()
    {
        // This test relates to https://github.com/App-vNext/Polly/issues/107.
        // An async (...) => { ... } anonymous delegate with no return type may compile to either an async void or an async Task method; which assign to an Action<...> or Func<..., Task> respectively.
        // However, if it compiles to async void (assigning tp Action<...>), then the delegate, when run, will return at the first await, and execution continues without waiting for the Action to complete, as described by Stephen Toub: https://devblogs.microsoft.com/pfxteam/potential-pitfalls-to-avoid-when-passing-around-async-lambdas/
        // If Polly were to declare only an Action<...> delegate for onRetry - but users declared async () => { } onRetry delegates - the compiler would happily assign them to the Action<...>, but the next 'try' would/could occur before onRetry execution had completed.
        // This test ensures the relevant retry policy does have a Func<..., Task> form for onRetry, and that it is awaited before the next try commences.

        TimeSpan shimTimeSpan = TimeSpan.FromSeconds(0.2); // Consider increasing shimTimeSpan if test fails transiently in different environments.

        int executeDelegateInvocations = 0;
        int executeDelegateInvocationsWhenOnRetryExits = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(1,
            _ => TimeSpan.Zero,
            async (_, _) =>
            {
                await Task.Delay(shimTimeSpan);
                executeDelegateInvocationsWhenOnRetryExits = executeDelegateInvocations;
            });

        await policy.Awaiting(p => p.ExecuteAsync(async () =>
        {
            executeDelegateInvocations++;
            await TaskHelper.EmptyTask;
            throw new DivideByZeroException();
        })).Should().ThrowAsync<DivideByZeroException>();

        while (executeDelegateInvocationsWhenOnRetryExits == 0)
        {
            // Wait for the onRetry delegate to complete.
        }

        executeDelegateInvocationsWhenOnRetryExits.Should().Be(1); // If the async onRetry delegate is genuinely awaited, only one execution of the .Execute delegate should have occurred by the time onRetry completes.  If the async onRetry delegate were instead assigned to an Action<...>, then onRetry will return, and the second action execution will commence, before await Task.Delay() completes, leaving executeDelegateInvocationsWhenOnRetryExits == 2.
        executeDelegateInvocations.Should().Be(2);
    }

    [Fact]
    public async Task Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().NotThrowAsync();
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public async Task Should_execute_all_tries_when_faulting_and_cancellationToken_not_cancelled()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().ThrowAsync<DivideByZeroException>();
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public async Task Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var ex = await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(0);
    }

    [Fact]
    public async Task Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 2,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(2);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 2,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(2);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_last_retry_execution_when_user_delegate_does_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1 + 3,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public async Task Should_report_faulting_from_faulting_last_retry_execution_when_user_delegate_does_not_observe_cancellation_raised_during_last_retry()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1 + 3,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().ThrowAsync<DivideByZeroException>();
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public async Task Should_honour_cancellation_immediately_during_wait_phase_of_waitandretry()
    {
        SystemClock.SleepAsync = Task.Delay;

        TimeSpan shimTimeSpan = TimeSpan.FromSeconds(1); // Consider increasing shimTimeSpan if test fails transiently in different environments.
        TimeSpan retryDelay = shimTimeSpan + shimTimeSpan + shimTimeSpan;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .WaitAndRetryAsync(new[] { retryDelay });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Stopwatch watch = new Stopwatch();
        watch.Start();

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 1,
            AttemptDuringWhichToCancel = null, // Cancellation invoked after delay - see below.
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            cancellationTokenSource.CancelAfter(shimTimeSpan);

            var ex = await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                    .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        watch.Stop();

        attemptsInvoked.Should().Be(1);

        watch.Elapsed.Should().BeLessThan(retryDelay);
        watch.Elapsed.Should().BeCloseTo(shimTimeSpan, precision: TimeSpan.FromMilliseconds((int)shimTimeSpan.TotalMilliseconds / 2));  // Consider increasing shimTimeSpan, or loosening precision, if test fails transiently in different environments.
    }

    [Fact]
    public async Task Should_report_cancellation_after_faulting_action_execution_and_cancel_further_retries_if_onRetry_invokes_cancellation()
    {
        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = null, // Cancellation during onRetry instead - see below.
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() },
                (_, _) =>
                {
                    cancellationTokenSource.Cancel();
                });

            var ex = await policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public async Task Should_execute_func_returning_value_when_cancellationToken_not_cancelled()
    {
        var policy = Policy
           .Handle<DivideByZeroException>()
           .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Func<AsyncRetryPolicy, Task> action = async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            await policy.Awaiting(action)
                .Should().NotThrowAsync();
        }

        result.Should().BeTrue();

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public async Task Should_honour_and_report_cancellation_during_func_execution()
    {
        var policy = Policy
           .Handle<DivideByZeroException>()
           .WaitAndRetryAsync(new[] { 1.Seconds(), 2.Seconds(), 3.Seconds() });

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Func<AsyncRetryPolicy, Task> action = async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            var ex = await policy.Awaiting(action).Should().ThrowAsync<OperationCanceledException>();
            ex.And.CancellationToken.Should().Be(cancellationToken);
        }

        result.Should().Be(null);

        attemptsInvoked.Should().Be(1);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
