﻿using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.Retry;

public class RetryAsyncSpecs
{
    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;
        var policyBuilder = new PolicyBuilder(exception => exception);
        Func<Exception, TimeSpan, int, Context, Task> onRetryAsync = (_, _, _, _) => Task.CompletedTask;
        int permittedRetryCount = int.MaxValue;
        IEnumerable<TimeSpan>? sleepDurationsEnumerable = null;
        Func<int, Exception, Context, TimeSpan> sleepDurationProvider = null!;

        var instance = Activator.CreateInstance(
            typeof(AsyncRetryPolicy),
            flags,
            null,
            [policyBuilder, onRetryAsync, permittedRetryCount, sleepDurationsEnumerable, sleepDurationProvider],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });
        var generic = methodInfo.MakeGenericMethod(typeof(EmptyStruct));

        var func = () => generic.Invoke(instance, [action, new Context(), CancellationToken.None, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_without_context()
    {
        Action<Exception, int> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .RetryAsync(-1, onRetry);

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>(3));
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryAsync(3);

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<ArgumentException>(3));
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_thrown_less_number_of_times_than_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_of_times_than_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryAsync(3);

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<ArgumentException>());
    }

    [Fact]
    public async Task Should_throw_when_specified_exception_thrown_more_times_then_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

        await Should.ThrowAsync<DivideByZeroException>(() => policy.RaiseExceptionAsync<DivideByZeroException>(3 + 1));
    }

    [Fact]
    public async Task Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_then_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryAsync(3);

        await Should.ThrowAsync<ArgumentException>(() => policy.RaiseExceptionAsync<ArgumentException>(3 + 1));
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync();

        await Should.ThrowAsync<NullReferenceException>(() => policy.RaiseExceptionAsync<NullReferenceException>());
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryAsync();

        await Should.ThrowAsync<NullReferenceException>(() => policy.RaiseExceptionAsync<NullReferenceException>());
    }

    [Fact]
    public async Task Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .RetryAsync();

        await Should.ThrowAsync<DivideByZeroException>(() => policy.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .RetryAsync();

        await Should.ThrowAsync<ArgumentException>(() => policy.RaiseExceptionAsync<ArgumentException>());
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .RetryAsync();

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .RetryAsync();

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<ArgumentException>());
    }

    [Fact]
    public void Should_throw_when_onRetry_is_null()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3, (Action<Exception, int>)null!);

        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_when_onRetryAsync_is_null()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3, (Func<Exception, int, Task>)null!);

        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe("onRetryAsync");
    }

    [Fact]
    public async Task Should_call_onretry_on_each_retry_with_the_current_retry_count()
    {
        var expectedRetryCounts = new[] { 1, 2, 3 };
        var retryCounts = new List<int>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3, (_, retryCount) => retryCounts.Add(retryCount));

        await policy.RaiseExceptionAsync<DivideByZeroException>(3);

        retryCounts.ShouldBe(expectedRetryCounts);
    }

    [Fact]
    public async Task Should_call_onretry_on_each_retry_with_the_current_exception()
    {
        var expectedExceptions = new[] { "Exception #1", "Exception #2", "Exception #3" };
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3, (exception, _) => retryExceptions.Add(exception));

        await policy.RaiseExceptionAsync<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

        retryExceptions
            .Select(x => x.HelpLink)
            .ShouldBe(expectedExceptions);
    }

    [Fact]
    public async Task Should_call_onretry_with_a_handled_innerexception()
    {
        Exception? passedToOnRetry = null;

        var policy = Policy
            .HandleInner<DivideByZeroException>()
            .RetryAsync(3, (exception, _) => passedToOnRetry = exception);

        Exception toRaiseAsInner = new DivideByZeroException();
        Exception withInner = new AggregateException(toRaiseAsInner);

        await policy.RaiseExceptionAsync(withInner);

        passedToOnRetry.ShouldBeSameAs(toRaiseAsInner);
    }

    [Fact]
    public async Task Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryCounts = new List<int>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync((_, retryCount) => retryCounts.Add(retryCount));

        await Should.ThrowAsync<ArgumentException>(() => policy.RaiseExceptionAsync<ArgumentException>());

        retryCounts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_create_new_state_for_each_call_to_policy()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync();

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>());
        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public void Should_call_onretry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync((_, _, context) => contextData = context);

        policy.RaiseExceptionAsync<DivideByZeroException>(
            CreateDictionary("key1", "value1", "key2", "value2"));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_onretry_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync((_, _, context) => contextData = context);

        await Should.NotThrowAsync(
            () =>
                policy.ExecuteAndCaptureAsync(
                    _ => { throw new DivideByZeroException(); },
                    CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_data()
    {
        Context? capturedContext = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync((_, _, context) => capturedContext = context);

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>());

        capturedContext.ShouldNotBeNull();
        capturedContext.ShouldBeEmpty();
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync((_, _, context) => contextValue = context["key"].ToString());

        policy.RaiseExceptionAsync<DivideByZeroException>(
            CreateDictionary("key", "original_value"));

        contextValue.ShouldBe("original_value");

        policy.RaiseExceptionAsync<DivideByZeroException>(
            CreateDictionary("key", "new_value"));

        contextValue.ShouldBe("new_value");
    }

    [Fact]
    public async Task Should_create_new_context_for_each_call_to_execute_and_capture()
    {
        string? contextValue = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync((_, _, context) => contextValue = context["key"].ToString());

        await Should.NotThrowAsync(
            () =>
                policy.ExecuteAndCaptureAsync(
                    _ => throw new DivideByZeroException(),
                    CreateDictionary("key", "original_value")));

        contextValue.ShouldBe("original_value");

        await Should.NotThrowAsync(
            () =>
                policy.ExecuteAndCaptureAsync(
                    _ => throw new DivideByZeroException(),
                    CreateDictionary("key", "new_value")));

        contextValue.ShouldBe("new_value");
    }

    [Fact]
    public async Task Should_not_call_onretry_when_retry_count_is_zero()
    {
        bool retryInvoked = false;

        Action<Exception, int> onRetry = (_, _) => { retryInvoked = true; };

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(0, onRetry);

        await Should.ThrowAsync<DivideByZeroException>(() => policy.RaiseExceptionAsync<DivideByZeroException>());

        retryInvoked.ShouldBeFalse();
    }

    #region Async and cancellation tests

    [Fact]
    public async Task Should_wait_asynchronously_for_async_onretry_delegate()
    {
        // This test relates to https://github.com/App-vNext/Polly/issues/107.
        // An async (...) => { ... } anonymous delegate with no return type may compile to either an async void or an async Task method; which assign to an Action<...> or Func<..., Task> respectively.
        // However, if it compiles to async void (assigning to Action<...>), then the delegate, when run, will return at the first await, and execution continues without waiting for the Action to complete, as described by Stephen Toub: https://devblogs.microsoft.com/pfxteam/potential-pitfalls-to-avoid-when-passing-around-async-lambdas/
        // If Polly were to declare only an Action<...> delegate for onRetry - but users declared async () => { } onRetry delegates - the compiler would happily assign them to the Action<...>, but the next 'try' of the retry policy would/could occur before onRetry execution had completed.
        // This test ensures the relevant retry policy does have a Func<..., Task> form for onRetry, and that it is awaited before the next try commences.

        TimeSpan shimTimeSpan = TimeSpan.FromSeconds(0.2);

        int executeDelegateInvocations = 0;
        int executeDelegateInvocationsWhenOnRetryExits = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(async (_, _) =>
            {
                await Task.Delay(shimTimeSpan);
                executeDelegateInvocationsWhenOnRetryExits = executeDelegateInvocations;
            });

        await Should.ThrowAsync<DivideByZeroException>(() => policy.ExecuteAsync(async () =>
        {
            executeDelegateInvocations++;
            await TaskHelper.EmptyTask;
            throw new DivideByZeroException();
        }));

        while (executeDelegateInvocationsWhenOnRetryExits == 0)
        {
            // Wait for the onRetry delegate to complete.
        }

        executeDelegateInvocationsWhenOnRetryExits.ShouldBe(1); // If the async onRetry delegate is genuinely awaited, only one execution of the .Execute delegate should have occurred by the time onRetry completes.  If the async onRetry delegate were instead assigned to an Action<...>, then onRetry will return, and the second action execution will commence, before await Task.Delay() completes, leaving executeDelegateInvocationsWhenOnRetryExits == 2.
        executeDelegateInvocations.ShouldBe(2);
    }

    [Fact]
    public async Task Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            await Should.NotThrowAsync(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_execute_all_tries_when_faulting_and_cancellationToken_not_cancelled()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            await Should.ThrowAsync<DivideByZeroException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1 + 3);
    }

    [Fact]
    public async Task Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);
    }

    [Fact]
    public async Task Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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
            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(2);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(2);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_last_retry_execution_when_user_delegate_does_observe_cancellationToken()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1 + 3);
    }

    [Fact]
    public async Task Should_report_faulting_from_faulting_last_retry_execution_when_user_delegate_does_not_observe_cancellation_raised_during_last_retry()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            await Should.ThrowAsync<DivideByZeroException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1 + 3);
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
                .RetryAsync(3, (_, _) =>
                {
                    cancellationTokenSource.Cancel();
                });

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_execute_func_returning_value_when_cancellationToken_not_cancelled()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Func<Task> action = async () => result = await policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            await Should.NotThrowAsync(action);
        }

        result.ShouldNotBeNull();
        result.Value.ShouldBeTrue();

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_honour_and_report_cancellation_during_func_execution()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryAsync(3);

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

            Func<Task> action = async () => result = await policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            var ex = await Should.ThrowAsync<OperationCanceledException>(action);
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        result.ShouldBeNull();

        attemptsInvoked.ShouldBe(1);
    }

    #endregion
}
