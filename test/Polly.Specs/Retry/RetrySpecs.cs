namespace Polly.Specs.Retry;

public class RetrySpecs
{
    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_without_context()
    {
        Action<Exception, int> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .Retry(-1, onRetry);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
              .ParamName.Should().Be("retryCount");
    }

    [Fact]
    public void Should_throw_when_onretry_action_without_context_is_null()
    {
        Action<Exception, int> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .Retry(1, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_with_context()
    {
        Action<Exception, int, Context> onRetry = (_, _, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .Retry(-1, onRetry);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
              .ParamName.Should().Be("retryCount");
    }

    [Fact]
    public void Should_throw_when_onretry_action_with_context_is_null()
    {
        Action<Exception, int, Context> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .Retry(1, nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>(3))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Retry(3);

        policy.Invoking(x => x.RaiseException<ArgumentException>(3))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_thrown_less_number_of_times_than_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_of_times_than_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Retry(3);

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().NotThrow();
    }

    [Fact]
    public void Should_throw_when_specified_exception_thrown_more_times_then_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>(3 + 1))
              .Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_then_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Retry(3);

        policy.Invoking(x => x.RaiseException<ArgumentException>(3 + 1))
              .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry();

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Retry();

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Retry();

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .Retry();

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Retry();

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .Retry();

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().NotThrow();
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
    {
        var expectedRetryCounts = new[] { 1, 2, 3 };
        var retryCounts = new List<int>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3, (_, retryCount) => retryCounts.Add(retryCount));

        policy.RaiseException<DivideByZeroException>(3);

        retryCounts.Should()
                   .ContainInOrder(expectedRetryCounts);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_exception()
    {
        var expectedExceptions = new[] { "Exception #1", "Exception #2", "Exception #3" };
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3, (exception, _) => retryExceptions.Add(exception));

        policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

        retryExceptions
            .Select(x => x.HelpLink)
            .Should()
            .ContainInOrder(expectedExceptions);
    }

    [Fact]
    public void Should_call_onretry_with_a_handled_innerexception()
    {
        Exception? passedToOnRetry = null;

        var policy = Policy
            .HandleInner<DivideByZeroException>()
            .Retry(3, (exception, _) => passedToOnRetry = exception);

        Exception toRaiseAsInner = new DivideByZeroException();
        Exception withInner = new AggregateException(toRaiseAsInner);

        policy.RaiseException(withInner);

        passedToOnRetry.Should().BeSameAs(toRaiseAsInner);
    }

    [Fact]
    public void Should_call_onretry_with_handled_exception_nested_in_aggregate_as_first_exception()
    {
        Exception? passedToOnRetry = null;

        var policy = Policy
            .HandleInner<DivideByZeroException>()
            .Retry(3, (exception, _) => passedToOnRetry = exception);

        Exception toRaiseAsInner = new DivideByZeroException();

        Exception aggregateException = new AggregateException(
            new Exception("First: With Inner Exception",
                toRaiseAsInner),
            new Exception("Second: Without Inner Exception"));

        policy.RaiseException(aggregateException);

        passedToOnRetry.Should().BeSameAs(toRaiseAsInner);
    }

    [Fact]
    public void Should_call_onretry_with_handled_exception_nested_in_aggregate_as_second_exception()
    {
        Exception? passedToOnRetry = null;

        var policy = Policy
            .HandleInner<DivideByZeroException>()
            .Retry(3, (exception, _) => passedToOnRetry = exception);

        Exception toRaiseAsInner = new DivideByZeroException();

        Exception aggregateException = new AggregateException(
            new Exception("First: Without Inner Exception"),
            new Exception("Second: With Inner Exception",
                toRaiseAsInner));

        policy.RaiseException(aggregateException);

        passedToOnRetry.Should().BeSameAs(toRaiseAsInner);
    }

    [Fact]
    public void Should_call_onretry_with_handled_exception_nested_in_aggregate_inside_another_aggregate_as_first_exception()
    {
        Exception? passedToOnRetry = null;

        var policy = Policy
            .HandleInner<DivideByZeroException>()
            .Retry(3, (exception, _) => passedToOnRetry = exception);

        Exception toRaiseAsInner = new DivideByZeroException();

        Exception aggregateException = new AggregateException(
            new AggregateException(
                new Exception("First: With Inner Exception",
                    toRaiseAsInner),
                new Exception("Second: Without Inner Exception")),
            new Exception("Exception"));

        policy.RaiseException(aggregateException);

        passedToOnRetry.Should().BeSameAs(toRaiseAsInner);
    }

    [Fact]
    public void Should_call_onretry_with_handled_exception_nested_in_aggregate_inside_another_aggregate_as_second_exception()
    {
        Exception? passedToOnRetry = null;

        var policy = Policy
            .HandleInner<DivideByZeroException>()
            .Retry(3, (exception, _) => passedToOnRetry = exception);

        Exception toRaiseAsInner = new DivideByZeroException();

        Exception aggregateException = new AggregateException(
            new Exception("Exception"),
            new AggregateException(
                new Exception("First: Without Inner Exception"),
                new Exception("Second: With Inner Exception",
                    toRaiseAsInner)));

        policy.RaiseException(aggregateException);

        passedToOnRetry.Should().BeSameAs(toRaiseAsInner);
    }

    [Fact]
    public void Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryCounts = new List<int>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, retryCount) => retryCounts.Add(retryCount));

        policy.Invoking(x => x.RaiseException<ArgumentException>())
            .Should().Throw<ArgumentException>();

        retryCounts.Should()
            .BeEmpty();
    }

    [Fact]
    public void Should_call_onretry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => contextData = context);

        policy.RaiseException<DivideByZeroException>(
            new { key1 = "value1", key2 = "value2" }.AsDictionary());

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Should_call_onretry_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => contextData = context);

        policy.Invoking(p => p.ExecuteAndCapture(_ => { throw new DivideByZeroException(); },
            new { key1 = "value1", key2 = "value2" }.AsDictionary()))
            .Should().NotThrow();

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => capturedContext = context);

        policy.RaiseException<DivideByZeroException>();

        capturedContext.Should()
                       .BeEmpty();
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => contextValue = context["key"].ToString());

        policy.RaiseException<DivideByZeroException>(
            new { key = "original_value" }.AsDictionary());

        contextValue.Should().Be("original_value");

        policy.RaiseException<DivideByZeroException>(
            new { key = "new_value" }.AsDictionary());

        contextValue.Should().Be("new_value");
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute_and_capture()
    {
        string? contextValue = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => contextValue = context["key"].ToString());

        policy.Invoking(p => p.ExecuteAndCapture(_ => throw new DivideByZeroException(),
            new { key = "original_value" }.AsDictionary()))
            .Should().NotThrow();

        contextValue.Should().Be("original_value");

        policy.Invoking(p => p.ExecuteAndCapture(_ => throw new DivideByZeroException(),
            new { key = "new_value" }.AsDictionary()))
            .Should().NotThrow();

        contextValue.Should().Be("new_value");
    }

    [Fact]
    public void Should_create_new_state_for_each_call_to_policy()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry();

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().NotThrow();

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().NotThrow();
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_without_context()
    {
        bool retryInvoked = false;

        Action<Exception, int> onRetry = (_, _) => { retryInvoked = true; };

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(0, onRetry);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().Throw<DivideByZeroException>();

        retryInvoked.Should().BeFalse();
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_with_context()
    {
        bool retryInvoked = false;

        Action<Exception, int, Context> onRetry = (_, _, _) => { retryInvoked = true; };

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(0, onRetry);

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().Throw<DivideByZeroException>();

        retryInvoked.Should().BeFalse();
    }

    #region Sync cancellation tests

    [Fact]
    public void Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().NotThrow();
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_execute_all_tries_when_faulting_and_cancellationToken_not_cancelled()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = null,
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().Throw<DivideByZeroException>();
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 2,
            ActionObservesCancellation = true
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 2,
            ActionObservesCancellation = false
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1 + 3,
            ActionObservesCancellation = true
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = 1 + 3,
            ActionObservesCancellation = false
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
            .Should().Throw<DivideByZeroException>();
        }

        attemptsInvoked.Should().Be(1 + 3);
    }

    [Fact]
    public void Should_report_cancellation_after_faulting_action_execution_and_cancel_further_retries_if_onRetry_invokes_cancellation()
    {
        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 1 + 3,
            AttemptDuringWhichToCancel = null, // Cancellation during onRetry instead - see below.
            ActionObservesCancellation = false
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3, (_, _) =>
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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.Invoking(x => result = x.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
            .Should().Throw<OperationCanceledException>().And.CancellationToken.Should().Be(cancellationToken);
        }

        result.Should().Be(null);

        attemptsInvoked.Should().Be(1);
    }

    #endregion
}
