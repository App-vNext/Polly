using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.Retry;

public class RetryForeverAsyncSpecs
{
    [Fact]
    public void Should_not_throw_if_arguments_are_valid()
    {
        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForeverAsync((DelegateResult<ResultPrimitive> _, Context _) => { });

        Should.NotThrow(policy);
    }

    [Fact]
    public async Task Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync();

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>(3));
    }

    [Fact]
    public async Task Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryForeverAsync();

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<ArgumentException>(3));
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync();

        await Should.ThrowAsync<NullReferenceException>(() => policy.RaiseExceptionAsync<NullReferenceException>());
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryForeverAsync();

        await Should.ThrowAsync<NullReferenceException>(() => policy.RaiseExceptionAsync<NullReferenceException>());
    }

    [Fact]
    public async Task Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .RetryForeverAsync();

        await Should.ThrowAsync<DivideByZeroException>(() => policy.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .RetryForeverAsync();

        await Should.ThrowAsync<ArgumentException>(() => policy.RaiseExceptionAsync<ArgumentException>());
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .RetryForeverAsync();

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .RetryForeverAsync();

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<ArgumentException>());
    }

    [Fact]
    public void Should_throw_when_onretry_exception_is_null()
    {
        var policyBuilder = Policy.Handle<Exception>();
        Assert.Throws<ArgumentNullException>("onRetry", () => policyBuilder.RetryForeverAsync(default(Action<Exception>)));
    }

    [Fact]
    public void Should_throw_when_onretry_exception_int_is_null()
    {
        var policyBuilder = Policy.Handle<Exception>();
        Assert.Throws<ArgumentNullException>("onRetry", () => policyBuilder.RetryForeverAsync(default(Action<Exception, int>)));
    }

    [Fact]
    public void Should_throw_when_onretry_is_null()
    {
        Action<DelegateResult<ResultPrimitive>> onRetry = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForeverAsync(onRetry);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, int> onRetryAttempts = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForeverAsync(onRetryAttempts);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_with_int_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, int> onRetry = null!;

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .RetryForeverAsync(onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretryasync_is_null()
    {
        Func<DelegateResult<ResultPrimitive>, Task> onRetryAsync = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForeverAsync(onRetryAsync);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetryAsync");

        Func<DelegateResult<ResultPrimitive>, int, Task> onRetryAsyncAttempts = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForeverAsync(onRetryAsyncAttempts);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetryAsync");

        Func<Exception, int, Task> onRetryAsyncExceptionAttempts = null!;

        policy = () => Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync(onRetryAsyncExceptionAttempts);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetryAsync");

        Func<Exception, Context, Task> onRetryAsyncExceptionContext = null!;

        policy = () => Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync(onRetryAsyncExceptionContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetryAsync");

        Func<Exception, int, Context, Task> onRetryAsyncExceptionAttemptsContext = null!;

        policy = () => Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync(onRetryAsyncExceptionAttemptsContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetryAsync");

        Func<DelegateResult<ResultPrimitive>, Context, Task> onRetryAsyncContext = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForeverAsync(onRetryAsyncContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetryAsync");

        Func<DelegateResult<ResultPrimitive>, int, Context, Task> onRetryAsyncAttemptsContext = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForeverAsync(onRetryAsyncAttemptsContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetryAsync");
    }

    [Fact]
    public void Should_throw_when_onretry_with_context_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, Context> onRetry = null!;

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .RetryForeverAsync(onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_with_int_and_context_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = null!;

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .RetryForeverAsync(onRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_exception_context_is_null()
    {
        var policyBuilder = Policy.Handle<Exception>();
        Assert.Throws<ArgumentNullException>("onRetry", () => policyBuilder.RetryForeverAsync(default(Action<Exception, Context>)));
    }

    [Fact]
    public void Should_throw_when_onretry_exception_int_context_is_null()
    {
        var policyBuilder = Policy.Handle<Exception>();
        Assert.Throws<ArgumentNullException>("onRetry", () => policyBuilder.RetryForeverAsync(default(Action<Exception, int, Context>)));
    }

    [Fact]
    public void Should_throw_when_onretry_exception_timespan_context_is_null()
    {
        var policyBuilder = Policy.Handle<Exception>();
        Assert.Throws<ArgumentNullException>("onRetry", () => policyBuilder.WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: _ => TimeSpan.FromSeconds(1),
            onRetry: default(Action<Exception, TimeSpan, Context>)));
    }

    [Fact]
    public async Task Should_call_onretry_on_each_retry_with_the_current_exception()
    {
        var expectedExceptions = new[] { "Exception #1", "Exception #2", "Exception #3" };
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync(retryExceptions.Add);

        await policy.RaiseExceptionAsync<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

        retryExceptions
            .Select(x => x.HelpLink)
            .ShouldBe(expectedExceptions);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync((_, context) => contextData = context);

        policy.RaiseExceptionAsync<DivideByZeroException>(
            CreateDictionary("key1", "value1", "key2", "value2"),
            cancellationToken: TestCancellation.Token);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
    {
        var expectedRetryCounts = new[] { 1, 2, 3 };
        var retryCounts = new List<int>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync((_, retryCount) => retryCounts.Add(retryCount));

        policy.RaiseExceptionAsync<DivideByZeroException>(3);

        retryCounts.ShouldBe(expectedRetryCounts);
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_data()
    {
        Context? capturedContext = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync((_, context) => capturedContext = context);

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>());

        capturedContext.ShouldBeEmpty();
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute()
    {
        var cancellationToken = TestCancellation.Token;
        string? contextValue = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync((_, context) => contextValue = context["key"].ToString());

        policy.RaiseExceptionAsync<DivideByZeroException>(
            CreateDictionary("key", "original_value"),
            cancellationToken: cancellationToken);

        contextValue.ShouldBe("original_value");

        policy.RaiseExceptionAsync<DivideByZeroException>(
            CreateDictionary("key", "new_value"),
            cancellationToken: cancellationToken);

        contextValue.ShouldBe("new_value");
    }

    [Fact]
    public async Task Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync(retryExceptions.Add);

        await Should.ThrowAsync<ArgumentException>(() => policy.RaiseExceptionAsync<ArgumentException>());

        retryExceptions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_wait_asynchronously_for_async_onretry_delegate()
    {
        // This test relates to https://github.com/App-vNext/Polly/issues/107.
        // An async (...) => { ... } anonymous delegate with no return type may compile to either an async void or an async Task method; which assign to an Action<...> or Func<..., Task> respectively.
        // However, if it compiles to async void (assigning tp Action<...>), then the delegate, when run, will return at the first await, and execution continues without waiting for the Action to complete, as described by Stephen Toub: https://devblogs.microsoft.com/pfxteam/potential-pitfalls-to-avoid-when-passing-around-async-lambdas/
        // If Polly were to declare only an Action<...> delegate for onRetry - but users declared async () => { } onRetry delegates - the compiler would happily assign them to the Action<...>, but the next 'try' would/could occur before onRetry execution had completed.
        // This test ensures the relevant retry policy does have a Func<..., Task> form for onRetry, and that it is awaited before the next try commences.

        TimeSpan shimTimeSpan = TimeSpan.FromSeconds(0.2);

        int executeDelegateInvocations = 0;
        int executeDelegateInvocationsWhenOnRetryExits = 0;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync(async _ =>
            {
                await Task.Delay(shimTimeSpan);
                executeDelegateInvocationsWhenOnRetryExits = executeDelegateInvocations;
            });

        await Should.NotThrowAsync(() => policy.ExecuteAsync(async () =>
        {
            executeDelegateInvocations++;
            await TaskHelper.EmptyTask;
            if (executeDelegateInvocations == 1)
            {
                throw new DivideByZeroException();
            }
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
            .RetryForeverAsync();

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
    public async Task Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync();

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
            .RetryForeverAsync();

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
            .RetryForeverAsync();

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
            .RetryForeverAsync();

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
            .RetryForeverAsync();

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
            .RetryForeverAsync();

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
                .RetryForeverAsync(
                _ =>
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
            .RetryForeverAsync();

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
            .RetryForeverAsync();

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

        result.ShouldBe(null);

        attemptsInvoked.ShouldBe(1);
    }
}
