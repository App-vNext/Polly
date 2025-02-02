namespace Polly.Specs.Retry;

public class RetrySpecs
{
    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, EmptyStruct> action = null!;
        var policyBuilder = new PolicyBuilder(exception => exception);
        Action<Exception, TimeSpan, int, Context> onRetry = (_, _, _, _) => { };
        int permittedRetryCount = int.MaxValue;
        IEnumerable<TimeSpan>? sleepDurationsEnumerable = null;
        Func<int, Exception, Context, TimeSpan>? sleepDurationProvider = null;

        var instance = Activator.CreateInstance(
            typeof(RetryPolicy),
            flags,
            null,
            [policyBuilder, onRetry, permittedRetryCount, sleepDurationsEnumerable, sleepDurationProvider],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "TResult" });
        var generic = methodInfo.MakeGenericMethod(typeof(EmptyStruct));

        var func = () => generic.Invoke(instance, [action, new Context(), CancellationToken.None]);

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
                                  .Retry(-1, onRetry);

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_throw_when_onretry_action_without_context_is_null()
    {
        Action<Exception, int> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .Retry(1, nullOnRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero_with_context()
    {
        Action<Exception, int, Context> onRetry = (_, _, _) => { };

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .Retry(-1, onRetry);

        Should.Throw<ArgumentOutOfRangeException>(policy)
              .ParamName.ShouldBe("retryCount");
    }

    [Fact]
    public void Should_throw_when_onretry_action_with_context_is_null()
    {
        Action<Exception, int, Context> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .Retry(1, nullOnRetry);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3);

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>(3));
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Retry(3);

        Should.NotThrow(() => policy.RaiseException<ArgumentException>(3));
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_thrown_less_number_of_times_than_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3);

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_of_times_than_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Retry(3);

        Should.NotThrow(() => policy.RaiseException<ArgumentException>());
    }

    [Fact]
    public void Should_throw_when_specified_exception_thrown_more_times_then_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(3);

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>(3 + 1));
    }

    [Fact]
    public void Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_then_retry_count()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Retry(3);

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>(3 + 1));
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry();

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Retry();

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());
    }

    [Fact]
    public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Retry();

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .Retry();

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>());
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Retry();

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .Retry();

        Should.NotThrow(() => policy.RaiseException<ArgumentException>());
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

        retryCounts.ShouldBe(expectedRetryCounts);
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
            .ShouldBe(expectedExceptions);
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

        passedToOnRetry.ShouldBeSameAs(toRaiseAsInner);
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

        passedToOnRetry.ShouldBeSameAs(toRaiseAsInner);
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

        passedToOnRetry.ShouldBeSameAs(toRaiseAsInner);
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

        passedToOnRetry.ShouldBeSameAs(toRaiseAsInner);
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

        passedToOnRetry.ShouldBeSameAs(toRaiseAsInner);
    }

    [Fact]
    public void Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryCounts = new List<int>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, retryCount) => retryCounts.Add(retryCount));

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>());

        retryCounts.ShouldBeEmpty();
    }

    [Fact]
    public void Should_call_onretry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => contextData = context);

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key1", "value1", "key2", "value2"));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_call_onretry_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => contextData = context);

        Should.NotThrow(() => policy.ExecuteAndCapture(_ => throw new DivideByZeroException(),
            CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => capturedContext = context);

        policy.RaiseException<DivideByZeroException>();

        capturedContext.ShouldBeEmpty();
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => contextValue = context["key"].ToString());

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "original_value"));

        contextValue.ShouldBe("original_value");

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "new_value"));

        contextValue.ShouldBe("new_value");
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute_and_capture()
    {
        string? contextValue = null;

        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, context) => contextValue = context["key"].ToString());

        Should.NotThrow(() => policy.ExecuteAndCapture(_ => throw new DivideByZeroException(), CreateDictionary("key", "original_value")));

        contextValue.ShouldBe("original_value");

        Should.NotThrow(() => policy.ExecuteAndCapture(_ => throw new DivideByZeroException(), CreateDictionary("key", "new_value")));

        contextValue.ShouldBe("new_value");
    }

    [Fact]
    public void Should_create_new_state_for_each_call_to_policy()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry();

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>());
        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_without_context()
    {
        bool retryInvoked = false;

        Action<Exception, int> onRetry = (_, _) => retryInvoked = true;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(0, onRetry);

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>());

        retryInvoked.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_call_onretry_when_retry_count_is_zero_with_context()
    {
        bool retryInvoked = false;

        Action<Exception, int, Context> onRetry = (_, _, _) => retryInvoked = true;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry(0, onRetry);

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>());

        retryInvoked.ShouldBeFalse();
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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.NotThrow(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);
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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<DivideByZeroException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1 + 3);
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
            .Retry(3);

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
            .Retry(3);

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
            .Retry(3);

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
            .Retry(3);

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
            .Retry(3);

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
            .Retry(3);

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
            .Retry(3);

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3, (_, _) =>
                {
                    cancellationTokenSource.Cancel();
                });

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
            .Retry(3);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<OperationCanceledException>(() => result = policy.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        result.ShouldBeNull();

        attemptsInvoked.ShouldBe(1);
    }

    #endregion
}
