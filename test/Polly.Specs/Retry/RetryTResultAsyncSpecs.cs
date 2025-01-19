using Scenario = Polly.Specs.Helpers.PolicyTResultExtensionsAsync.ResultAndOrCancellationScenario;

namespace Polly.Specs.Retry;

public class RetryTResultAsyncSpecs
{
    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;
        var policyBuilder = new PolicyBuilder<EmptyStruct>(exception => exception);
        Func<DelegateResult<EmptyStruct>, TimeSpan, int, Context, Task> onRetryAsync = (_, _, _, _) => Task.CompletedTask;
        int permittedRetryCount = int.MaxValue;
        IEnumerable<TimeSpan>? sleepDurationsEnumerable = null;
        Func<int, DelegateResult<EmptyStruct>, Context, TimeSpan> sleepDurationProvider = null!;

        var instance = Activator.CreateInstance(
            typeof(AsyncRetryPolicy<EmptyStruct>),
            flags,
            null,
            [policyBuilder, onRetryAsync, permittedRetryCount, sleepDurationsEnumerable, sleepDurationProvider],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_without_context()
    {
        Action<DelegateResult<ResultPrimitive>, int> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .RetryAsync(-1, onRetry);

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_throw_when_onretry_action_without_context_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, int> nullOnRetry = null!;

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .RetryAsync(1, nullOnRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_with_context()
    {
        Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, _, _) => { };

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .RetryAsync(-1, onRetry);

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_throw_when_onretry_action_with_context_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, int, Context> nullOnRetry = null!;

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .RetryAsync(1, nullOnRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public async Task Should_not_return_handled_result_when_handled_result_raised_same_number_of_times_as_retry_count()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(3);

        ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_not_return_handled_result_when_one_of_the_handled_results_raised_same_number_of_times_as_retry_count()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .RetryAsync(3);

        ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.FaultAgain, ResultPrimitive.Fault, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_not_return_handled_result_when_handled_result_raised_less_number_of_times_than_retry_count()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(3);

        ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_not_return_handled_result_when_all_of_the_handled_results_raised_less_number_of_times_than_retry_count()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .RetryAsync(3);

        ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_return_handled_result_when_handled_result_raised_more_times_then_retry_count()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(3);

        ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Fault); // It should give up retrying after 3 retries and return the last failure, so should return Fault, not Good.
    }

    [Fact]
    public async Task Should_return_handled_result_when_one_of_the_handled_results_is_raised_more_times_then_retry_count()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .RetryAsync(3);

        ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.FaultAgain);
    }

    [Fact]
    public async Task Should_return_result_when_result_is_not_the_specified_handled_result()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync();

        ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.FaultAgain);
    }

    [Fact]
    public async Task Should_return_result_when_result_is_not_one_of_the_specified_handled_results()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .RetryAsync();

        ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.FaultYetAgain, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.FaultYetAgain);
    }

    [Fact]
    public async Task Should_return_result_when_specified_result_predicate_is_not_satisfied()
    {
        var policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .RetryAsync();

        ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
    }

    [Fact]
    public async Task Should_return_result_when_none_of_the_specified_result_predicates_are_satisfied()
    {
        var policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .OrResult(r => r.ResultCode == ResultPrimitive.FaultAgain)
            .RetryAsync();

        ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultYetAgain), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.ShouldBe(ResultPrimitive.FaultYetAgain);
    }

    [Fact]
    public async Task Should_not_return_handled_result_when_specified_result_predicate_is_satisfied()
    {
        var policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .RetryAsync();

        ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_not_return_handled_result_when_one_of_the_specified_result_predicates_is_satisfied()
    {
        var policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .OrResult(r => r.ResultCode == ResultPrimitive.FaultAgain)
            .RetryAsync();

        ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_call_onretry_on_each_retry_with_the_current_retry_count()
    {
        var expectedRetryCounts = new[] { 1, 2, 3 };
        var retryCounts = new List<int>();

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(3, (_, retryCount) => retryCounts.Add(retryCount));

        (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good))
            .ShouldBe(ResultPrimitive.Good);

        retryCounts.ShouldBe(expectedRetryCounts);
    }

    [Fact]
    public async Task Should_call_onretry_on_each_retry_with_the_current_handled_result()
    {
        var expectedFaults = new[] { "Fault #1", "Fault #2", "Fault #3" };
        var retryFaults = new List<string?>();

        var policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .RetryAsync(3, (outcome, _) => retryFaults.Add(outcome.Result.SomeString));

        var resultsToRaise = expectedFaults.Select(s => new ResultClass(ResultPrimitive.Fault, s)).ToList();
        resultsToRaise.Add(new ResultClass(ResultPrimitive.Fault));

        (await policy.RaiseResultSequenceAsync(resultsToRaise))
            .ResultCode.ShouldBe(ResultPrimitive.Fault);

        retryFaults.ShouldBe(expectedFaults);
    }

    [Fact]
    public async Task Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryCounts = new List<int>();

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, retryCount) => retryCounts.Add(retryCount));

        (await policy.RaiseResultSequenceAsync(ResultPrimitive.Good))
            .ShouldBe(ResultPrimitive.Good);

        retryCounts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_call_onretry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, context) => contextData = context);

        (await policy.RaiseResultSequenceAsync(
            CreateDictionary("key1", "value1", "key2", "value2"),
            ResultPrimitive.Fault, ResultPrimitive.Good))
            .ShouldBe(ResultPrimitive.Good);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_onretry_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, context) => contextData = context);

        PolicyResult<ResultPrimitive> result = await policy.RaiseResultSequenceOnExecuteAndCaptureAsync(
            CreateDictionary("key1", "value1", "key2", "value2"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Successful);
        result.FinalException.ShouldBeNull();
        result.ExceptionType.ShouldBeNull();
        result.FaultType.ShouldBeNull();
        result.FinalHandledResult.ShouldBe(default);
        result.Result.ShouldBe(ResultPrimitive.Good);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, context) => capturedContext = context);

        await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good);

        capturedContext.ShouldNotBeNull();
        capturedContext.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, context) => contextValue = context["key"].ToString());

        await policy.RaiseResultSequenceAsync(
            CreateDictionary("key", "original_value"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        contextValue.ShouldBe("original_value");

        await policy.RaiseResultSequenceAsync(
            CreateDictionary("key", "new_value"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        contextValue.ShouldBe("new_value");
    }

    [Fact]
    public async Task Should_create_new_context_for_each_call_to_execute_and_capture()
    {
        string? contextValue = null;

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, context) => contextValue = context["key"].ToString());

        await policy.RaiseResultSequenceOnExecuteAndCaptureAsync(
            CreateDictionary("key", "original_value"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        contextValue.ShouldBe("original_value");

        await policy.RaiseResultSequenceOnExecuteAndCaptureAsync(
            CreateDictionary("key", "new_value"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        contextValue.ShouldBe("new_value");
    }

    [Fact]
    public async Task Should_create_new_state_for_each_call_to_policy()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(1);

        (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);

        (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);

    }

    [Fact]
    public async Task Should_not_call_onretry_when_retry_count_is_zero_without_context()
    {
        bool retryInvoked = false;

        Action<DelegateResult<ResultPrimitive>, int> onRetry = (_, _) => { retryInvoked = true; };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(0, onRetry);

        (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Fault);

        retryInvoked.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_call_onretry_when_retry_count_is_zero_with_context()
    {
        bool retryInvoked = false;

        Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, _, _) => { retryInvoked = true; };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(0, onRetry);

        (await policy.RaiseResultSequenceAsync(
            CreateDictionary("key", "value"),
            ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Fault);

        retryInvoked.ShouldBeFalse();
    }

    #region Async and cancellation tests

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
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(async (_, _) =>
            {
                await Task.Delay(shimTimeSpan);
                executeDelegateInvocationsWhenOnRetryExits = executeDelegateInvocations;
            });

        (await policy.ExecuteAsync(async () =>
        {
            executeDelegateInvocations++;
            await TaskHelper.EmptyTask;
            return ResultPrimitive.Fault;
        })).ShouldBe(ResultPrimitive.Fault);

        while (executeDelegateInvocationsWhenOnRetryExits == 0)
        {
            // Wait for the onRetry delegate to complete.
        }

        executeDelegateInvocationsWhenOnRetryExits.ShouldBe(1); // If the async onRetry delegate is genuinely awaited, only one execution of the .Execute delegate should have occurred by the time onRetry completes.  If the async onRetry delegate were instead assigned to an Action<...>, then onRetry will return, and the second action execution will commence, before await Task.Delay() completes, leaving executeDelegateInvocationsWhenOnRetryExits == 2.
        executeDelegateInvocations.ShouldBe(2);
    }

    [Fact]
    public async Task Should_execute_all_tries_when_faulting_and_cancellationToken_not_cancelled()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                ResultPrimitive.Fault,
                ResultPrimitive.Fault,
                ResultPrimitive.Fault,
                ResultPrimitive.Good))
                .ShouldBe(ResultPrimitive.Good);
        }

        attemptsInvoked.ShouldBe(1 + 3);
    }

    [Fact]
    public async Task Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => policy.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);
    }

    [Fact]
    public async Task Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => policy.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Good,
                    ResultPrimitive.Good,
                    ResultPrimitive.Good,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
           .HandleResult(ResultPrimitive.Fault)
           .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => policy.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
          .HandleResult(ResultPrimitive.Fault)
          .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => policy.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        var policy = Policy
          .HandleResult(ResultPrimitive.Fault)
          .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 2,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => policy.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(2);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        var policy = Policy
          .HandleResult(ResultPrimitive.Fault)
          .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 2,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => policy.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(2);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_last_retry_execution_when_user_delegate_does_observe_cancellationToken()
    {
        var policy = Policy
                   .HandleResult(ResultPrimitive.Fault)
                   .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1 + 3,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => policy.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1 + 3);
    }

    [Fact]
    public async Task Should_report_faulting_from_faulting_last_retry_execution_when_user_delegate_does_not_observe_cancellation_raised_during_last_retry()
    {
        var policy = Policy
                   .HandleResult(ResultPrimitive.Fault)
                   .RetryAsync(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1 + 3,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Good))
           .ShouldBe(ResultPrimitive.Fault);
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
            AttemptDuringWhichToCancel = null, // Cancellation during onRetry instead - see below.
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3, (_, _) =>
                {
                    cancellationTokenSource.Cancel();
                });

            var ex = await Should.ThrowAsync<OperationCanceledException>(
                () => policy.RaiseResultSequenceAndOrCancellationAsync(
                    scenario,
                    cancellationTokenSource,
                    onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    #endregion
}
