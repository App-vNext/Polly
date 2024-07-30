using static Polly.Specs.DictionaryHelpers;
using Scenario = Polly.Specs.Helpers.PolicyTResultExtensions.ResultAndOrCancellationScenario;

namespace Polly.Specs.Retry;

public class RetryTResultSpecs
{
    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, EmptyStruct> action = null!;
        var policyBuilder = new PolicyBuilder<EmptyStruct>(exception => exception);
        Action<DelegateResult<EmptyStruct>, TimeSpan, int, Context> onRetry = (_, _, _, _) => { };
        int permittedRetryCount = int.MaxValue;
        IEnumerable<TimeSpan>? sleepDurationsEnumerable = null;
        Func<int, DelegateResult<EmptyStruct>, Context, TimeSpan>? sleepDurationProvider = null;

        var instance = Activator.CreateInstance(
            typeof(RetryPolicy<EmptyStruct>),
            flags,
            null,
            [policyBuilder, onRetry, permittedRetryCount, sleepDurationsEnumerable, sleepDurationProvider],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "EmptyStruct" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None]);

        var exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.And.InnerException.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("action");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_without_context()
    {
        Action<DelegateResult<ResultPrimitive>, int> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .Retry(-1, onRetry);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
              .ParamName.Should().Be("retryCount");
    }

    [Fact]
    public void Should_throw_when_onretry_action_without_context_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, int> nullOnRetry = null!;

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .Retry(1, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_with_context()
    {
        Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, _, _) => { };

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .Retry(-1, onRetry);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
              .ParamName.Should().Be("retryCount");
    }

    [Fact]
    public void Should_throw_when_onretry_action_with_context_is_null()
    {
        Action<DelegateResult<ResultPrimitive>, int, Context> nullOnRetry = null!;

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .Retry(1, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_not_return_handled_result_when_handled_result_raised_same_number_of_times_as_retry_count()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(3);

        ResultPrimitive result = policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good);
        result.Should().Be(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_not_return_handled_result_when_one_of_the_handled_results_raised_same_number_of_times_as_retry_count()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Retry(3);

        ResultPrimitive result = policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.FaultAgain, ResultPrimitive.Fault, ResultPrimitive.Good);
        result.Should().Be(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_not_return_handled_result_when_handled_result_raised_less_number_of_times_than_retry_count()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(3);

        ResultPrimitive result = policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Good);
        result.Should().Be(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_not_return_handled_result_when_all_of_the_handled_results_raised_less_number_of_times_than_retry_count()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Retry(3);

        ResultPrimitive result = policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.Should().Be(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_return_handled_result_when_handled_result_raised_more_times_then_retry_count()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(3);

        ResultPrimitive result = policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good);
        result.Should().Be(ResultPrimitive.Fault); // It should give up retrying after 3 retries and return the last failure, so should return Fault, not Good.
    }

    [Fact]
    public void Should_return_handled_result_when_one_of_the_handled_results_is_raised_more_times_then_retry_count()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Retry(3);

        ResultPrimitive result = policy.RaiseResultSequence(ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.Should().Be(ResultPrimitive.FaultAgain);
    }

    [Fact]
    public void Should_return_result_when_result_is_not_the_specified_handled_result()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry();

        ResultPrimitive result = policy.RaiseResultSequence(ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.Should().Be(ResultPrimitive.FaultAgain);
    }

    [Fact]
    public void Should_return_result_when_result_is_not_one_of_the_specified_handled_results()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Retry();

        ResultPrimitive result = policy.RaiseResultSequence(ResultPrimitive.FaultYetAgain, ResultPrimitive.Good);
        result.Should().Be(ResultPrimitive.FaultYetAgain);
    }

    [Fact]
    public void Should_return_result_when_specified_result_predicate_is_not_satisfied()
    {
        Policy<ResultClass> policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .Retry();

        ResultClass result = policy.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.Should().Be(ResultPrimitive.FaultAgain);
    }

    [Fact]
    public void Should_return_result_when_none_of_the_specified_result_predicates_are_satisfied()
    {
        Policy<ResultClass> policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .OrResult(r => r.ResultCode == ResultPrimitive.FaultAgain)
            .Retry();

        ResultClass result = policy.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultYetAgain), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.Should().Be(ResultPrimitive.FaultYetAgain);
    }

    [Fact]
    public void Should_not_return_handled_result_when_specified_result_predicate_is_satisfied()
    {
        Policy<ResultClass> policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .Retry();

        ResultClass result = policy.RaiseResultSequence(new ResultClass(ResultPrimitive.Fault), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.Should().Be(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_not_return_handled_result_when_one_of_the_specified_result_predicates_is_satisfied()
    {
        Policy<ResultClass> policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .OrResult(r => r.ResultCode == ResultPrimitive.FaultAgain)
            .Retry();

        ResultClass result = policy.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.Should().Be(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
    {
        var expectedRetryCounts = new[] { 1, 2, 3 };
        var retryCounts = new List<int>();

        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(3, (_, retryCount) => retryCounts.Add(retryCount));

        policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good);

        retryCounts.Should()
                   .ContainInOrder(expectedRetryCounts);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_handled_result()
    {
        var expectedFaults = new[] { "Fault #1", "Fault #2", "Fault #3" };
        var retryFaults = new List<string?>();

        Policy<ResultClass> policy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .Retry(3, (outcome, _) => retryFaults.Add(outcome.Result.SomeString));

        IList<ResultClass> resultsToRaise = expectedFaults.Select(s => new ResultClass(ResultPrimitive.Fault, s)).ToList();
        resultsToRaise.Add(new ResultClass(ResultPrimitive.Fault));

        policy.RaiseResultSequence(resultsToRaise);

        retryFaults
            .Should()
            .ContainInOrder(expectedFaults);
    }

    [Fact]
    public void Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryCounts = new List<int>();

        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, retryCount) => retryCounts.Add(retryCount));

        policy.RaiseResultSequence(ResultPrimitive.Good);

        retryCounts.Should()
            .BeEmpty();
    }

    [Fact]
    public void Should_call_onretry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _, context) => contextData = context);

        policy.RaiseResultSequence(
            CreateDictionary("key1", "value1", "key2", "value2"),
            ResultPrimitive.Fault, ResultPrimitive.Good)
            .Should().Be(ResultPrimitive.Good);

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Should_call_onretry_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _, context) => contextData = context);

        PolicyResult<ResultPrimitive> result = policy.RaiseResultSequenceOnExecuteAndCapture(
            CreateDictionary("key1", "value1", "key2", "value2"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        result.Should().BeEquivalentTo(new
        {
            Outcome = OutcomeType.Successful,
            FinalException = (Exception?)null,
            ExceptionType = (ExceptionType?)null,
            FaultType = (FaultType?)null,
            FinalHandledResult = default(ResultPrimitive),
            Result = ResultPrimitive.Good
        });

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;

        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _, context) => capturedContext = context);

        policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Good);

        capturedContext.Should()
                       .BeEmpty();
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _, context) => contextValue = context["key"].ToString());

        policy.RaiseResultSequence(
            CreateDictionary("key", "original_value"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        contextValue.Should().Be("original_value");

        policy.RaiseResultSequence(
            CreateDictionary("key", "new_value"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        contextValue.Should().Be("new_value");
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute_and_capture()
    {
        string? contextValue = null;

        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _, context) => contextValue = context["key"].ToString());

        policy.RaiseResultSequenceOnExecuteAndCapture(
            CreateDictionary("key", "original_value"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        contextValue.Should().Be("original_value");

        policy.RaiseResultSequenceOnExecuteAndCapture(
            CreateDictionary("key", "new_value"),
            ResultPrimitive.Fault, ResultPrimitive.Good);

        contextValue.Should().Be("new_value");
    }

    [Fact]
    public void Should_create_new_state_for_each_call_to_policy()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(1);

        policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Good).Should().Be(ResultPrimitive.Good);

        policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Good).Should().Be(ResultPrimitive.Good);

    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_without_context()
    {
        bool retryInvoked = false;

        Action<DelegateResult<ResultPrimitive>, int> onRetry = (_, _) => { retryInvoked = true; };

        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(0, onRetry);

        policy.RaiseResultSequence(ResultPrimitive.Fault, ResultPrimitive.Good).Should().Be(ResultPrimitive.Fault);

        retryInvoked.Should().BeFalse();
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_with_context()
    {
        bool retryInvoked = false;

        Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, _, _) => { retryInvoked = true; };

        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(0, onRetry);

        policy.RaiseResultSequence(
            CreateDictionary("key", "value"),
            ResultPrimitive.Fault, ResultPrimitive.Good).Should().Be(ResultPrimitive.Fault);

        retryInvoked.Should().BeFalse();
    }

    #region Sync cancellation tests

    [Fact]
    public void Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good)
            .Should().Be(ResultPrimitive.Good);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_execute_all_tries_when_faulting_and_cancellationToken_not_cancelled()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
                ResultPrimitive.Fault,
                ResultPrimitive.Fault,
                ResultPrimitive.Fault,
                ResultPrimitive.Good)
                .Should().Be(ResultPrimitive.Good);
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(3);

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

            policy.Invoking(x => x.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(0);
    }

    [Fact]
    public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry(3);

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

            policy.Invoking(x => x.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
               ResultPrimitive.Good,
               ResultPrimitive.Good,
               ResultPrimitive.Good,
               ResultPrimitive.Good))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
           .HandleResult(ResultPrimitive.Fault)
           .Retry(3);

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

            policy.Invoking(x => x.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Good))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
          .HandleResult(ResultPrimitive.Fault)
          .Retry(3);

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

            policy.Invoking(x => x.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Good))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
          .HandleResult(ResultPrimitive.Fault)
          .Retry(3);

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

            policy.Invoking(x => x.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Good))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(2);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
          .HandleResult(ResultPrimitive.Fault)
          .Retry(3);

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

            policy.Invoking(x => x.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Good))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(2);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_last_retry_execution_when_user_delegate_does_observe_cancellationToken()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
                   .HandleResult(ResultPrimitive.Fault)
                   .Retry(3);

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

            policy.Invoking(x => x.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Good))
            .Should().Throw<OperationCanceledException>()
            .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public void Should_report_faulting_from_faulting_last_retry_execution_when_user_delegate_does_not_observe_cancellation_raised_during_last_retry()
    {
        RetryPolicy<ResultPrimitive> policy = Policy
                   .HandleResult(ResultPrimitive.Fault)
                   .Retry(3);

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

            policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Fault,
               ResultPrimitive.Good)
           .Should().Be(ResultPrimitive.Fault);
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public void Should_report_cancellation_after_faulting_action_execution_and_cancel_further_retries_if_onRetry_invokes_cancellation()
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

            RetryPolicy<ResultPrimitive> policy = Policy
           .HandleResult(ResultPrimitive.Fault)
           .Retry(3, (_, _) =>
           {
               cancellationTokenSource.Cancel();
           });

            policy.Invoking(x => x.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    #endregion
}
