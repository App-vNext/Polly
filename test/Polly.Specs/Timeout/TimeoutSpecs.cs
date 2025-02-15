namespace Polly.Specs.Timeout;

[Collection(Constants.SystemClockDependentTestCollection)]
public class TimeoutSpecs : TimeoutSpecsBase
{
    #region Configuration

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, EmptyStruct> action = null!;
        Func<Context, TimeSpan> timeoutProvider = (_) => TimeSpan.Zero;
        TimeoutStrategy timeoutStrategy = TimeoutStrategy.Optimistic;
        Action<Context, TimeSpan, Task, Exception> onTimeout = (_, _, _, _) => { };

        var instance = Activator.CreateInstance(
            typeof(TimeoutPolicy),
            flags,
            null,
            [timeoutProvider, timeoutStrategy, onTimeout],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "TResult" });
        var generic = methodInfo.MakeGenericMethod(typeof(EmptyStruct));

        var func = () => generic.Invoke(instance, [action, new Context(), CancellationToken]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_timeout_is_zero_by_timespan()
    {
        Action policy = () => Policy.Timeout(TimeSpan.Zero);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("timeout");
    }

    [Fact]
    public void Should_throw_when_timeout_is_zero_by_seconds()
    {
        Action policy = () => Policy.Timeout(0);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("seconds");
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_timespan()
    {
        Action policy = () => Policy.Timeout(-TimeSpan.FromHours(1));

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("timeout");
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_seconds()
    {
        Action policy = () => Policy.Timeout(-10);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("seconds");
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_timespan()
    {
        Action policy = () => Policy.Timeout(TimeSpan.FromMilliseconds(1));

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds()
    {
        Action policy = () => Policy.Timeout(3);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_maxvalue()
    {
        Action policy = () => Policy.Timeout(TimeSpan.MaxValue);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_seconds_is_maxvalue()
    {
        Action policy = () => Policy.Timeout(int.MaxValue);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan()
    {
        Action policy = () => Policy.Timeout(System.Threading.Timeout.InfiniteTimeSpan);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_seconds_with_timeoutstrategy()
    {
        Action policy = () => Policy.Timeout(-10, TimeoutStrategy.Optimistic);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("seconds");
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds_with_timeoutstrategy()
    {
        Action policy = () => Policy.Timeout(3, TimeoutStrategy.Optimistic);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_timeoutstrategy()
    {
        Action policy = () => Policy.Timeout(System.Threading.Timeout.InfiniteTimeSpan, TimeoutStrategy.Optimistic);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_seconds_with_ontimeout()
    {
        Action<Context, TimeSpan, Task> doNothing = (_, _, _) => { };
        Action policy = () => Policy.Timeout(-10, doNothing);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("seconds");
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds_with_ontimeout()
    {
        Action<Context, TimeSpan, Task> doNothing = (_, _, _) => { };
        Action policy = () => Policy.Timeout(3, doNothing);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_ontimeout()
    {
        Action<Context, TimeSpan, Task> doNothing = (_, _, _) => { };
        Action policy = () => Policy.Timeout(System.Threading.Timeout.InfiniteTimeSpan, doNothing);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_ontimeout_overload()
    {
        Action<Context, TimeSpan, Task, Exception> doNothing = (_, _, _, _) => { };
        Action policy = () => Policy.Timeout(System.Threading.Timeout.InfiniteTimeSpan, doNothing);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_timeoutstrategy_and_ontimeout()
    {
        Action<Context, TimeSpan, Task> doNothing = (_, _, _) => { };
        Action policy = () => Policy.Timeout(System.Threading.Timeout.InfiniteTimeSpan, TimeoutStrategy.Optimistic, doNothing);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_timeoutstrategy_and_ontimeout_overload()
    {
        Action<Context, TimeSpan, Task, Exception> doNothing = (_, _, _, _) => { };
        Action policy = () => Policy.Timeout(System.Threading.Timeout.InfiniteTimeSpan, TimeoutStrategy.Optimistic, doNothing);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timespan()
    {
        Action<Context, TimeSpan, Task> onTimeout = null!;
        Action policy = () => Policy.Timeout(TimeSpan.FromMinutes(0.5), onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeout");
    }

    [Fact]
    public void Should_throw_when_onTimeout_overload_is_null_with_timespan()
    {
        Action<Context, TimeSpan, Task, Exception> onTimeout = null!;
        Action policy = () => Policy.Timeout(TimeSpan.FromMinutes(0.5), onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeout");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_seconds()
    {
        Action<Context, TimeSpan, Task> onTimeout = null!;
        Action policy = () => Policy.Timeout(30, onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeout");
    }

    [Fact]
    public void Should_throw_when_onTimeout_overload_is_null_with_seconds()
    {
        Action<Context, TimeSpan, Task, Exception> onTimeout = null!;
        Action policy = () => Policy.Timeout(30, onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeout");
    }

    [Fact]
    public void Should_throw_when_timeoutProvider_is_null()
    {
        Action policy = () => Policy.Timeout((Func<TimeSpan>)null!);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("timeoutProvider");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timeoutprovider()
    {
        Action<Context, TimeSpan, Task> onTimeout = null!;
        Action policy = () => Policy.Timeout(() => TimeSpan.FromSeconds(30), onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeout");
    }

    [Fact]
    public void Should_throw_when_onTimeout_overload_is_null_with_timeoutprovider()
    {
        Action<Context, TimeSpan, Task, Exception> onTimeout = null!;
        Action policy = () => Policy.Timeout(() => TimeSpan.FromSeconds(30), onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeout");
    }

    [Fact]
    public void Should_be_able_to_configure_with_timeout_func()
    {
        Action policy = () => Policy.Timeout(() => TimeSpan.FromSeconds(1));

        Should.NotThrow(policy);
    }

    #endregion

    #region Timeout operation - pessimistic

    [Fact]
    public void Should_throw_when_timeout_is_less_than_execution_duration__pessimistic()
    {
        var policy = Policy.Timeout(TimeSpan.FromMilliseconds(50), TimeoutStrategy.Pessimistic);

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken)));
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_execution_duration__pessimistic()
    {
        var policy = Policy.Timeout(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

        var result = ResultPrimitive.Undefined;
        var userCancellationToken = CancellationToken;

        Action act = () =>
        {
            result = policy.Execute(ct =>
            {
                SystemClock.Sleep(TimeSpan.FromMilliseconds(500), ct);
                return ResultPrimitive.Good;
            }, userCancellationToken);
        };

        Should.NotThrow(act);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_throw_timeout_after_correct_duration__pessimistic()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(1);
        var policy = Policy.Timeout(timeout, TimeoutStrategy.Pessimistic);

        TimeSpan tolerance = TimeSpan.FromSeconds(3);

        var watch = Stopwatch.StartNew();
        Should.Throw<TimeoutRejectedException>(() => policy.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(10), CancellationToken)));
        watch.Stop();

        watch.Elapsed.ShouldBe(timeout, TimeSpan.FromMilliseconds(tolerance.TotalMilliseconds));
    }

    [Fact]
    public void Should_rethrow_exception_from_inside_delegate__pessimistic()
    {
        var policy = Policy.Timeout(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

        Should.Throw<NotSupportedException>(() => policy.Execute(() => throw new NotSupportedException()));
    }

    [Fact]
    public void Should_rethrow_aggregate_exception_from_inside_delegate__pessimistic_with_full_stacktrace()
    {
        var policy = Policy.Timeout(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);
        var msg = "Aggregate Exception thrown from the delegate";

        // Check to see if nested aggregate exceptions are unwrapped correctly
        AggregateException exception = new AggregateException(msg, new NotImplementedException());

        var actual = Should.Throw<AggregateException>(() => policy.Execute(() => Helper_ThrowException(exception)));
        actual.Message.ShouldBe(exception.Message);
        actual.InnerException.ShouldBeOfType<NotImplementedException>();
        actual.StackTrace.ShouldNotBeNull();
        actual.StackTrace.ShouldContain(nameof(Helper_ThrowException));
    }

    [Fact]
    public void Should_rethrow_aggregate_exception_with_multiple_exceptions_from_inside_delegate__pessimistic()
    {
        var policy = Policy.Timeout(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);
        var msg = "Aggregate Exception thrown from the delegate";

        Exception innerException1 = new NotImplementedException();
        Exception innerException2 = new DivideByZeroException();
        AggregateException aggregateException = new AggregateException(msg, innerException1, innerException2);
        Action action = () => throw aggregateException;

        // Whether executing the delegate directly, or through the policy, exception behavior should be the same.
        var exception = Should.Throw<AggregateException>(action);
        exception.Message.ShouldBe(aggregateException.Message);
        exception.InnerExceptions.ShouldBe([innerException1, innerException2]);

        exception = Should.Throw<AggregateException>(() => policy.Execute(action));
        exception.Message.ShouldBe(aggregateException.Message);
        exception.InnerExceptions.ShouldBe([innerException1, innerException2]);
    }

    [Fact]
    public void Should_rethrow_aggregate_exception_with_example_cause_of_multiple_exceptions_from_inside_delegate__pessimistic()
    {
        var policy = Policy.Timeout(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

        Exception innerException1 = new NotImplementedException();
        Exception innerException2 = new DivideByZeroException();
        Action action = () =>
        {
            Task task1 = Task.Run(() => throw innerException1);
            Task task2 = Task.Run(() => throw innerException2);
            Task.WhenAll(task1, task2).Wait();
        };

        // Whether executing the delegate directly, or through the policy, exception behavior should be the same.
        var exception = Should.Throw<AggregateException>(action);
        exception.InnerExceptions.ShouldContain(innerException1);
        exception.InnerExceptions.ShouldContain(innerException2);

        exception = Should.Throw<AggregateException>(() => policy.Execute(action));
        exception.InnerExceptions.ShouldContain(innerException1);
        exception.InnerExceptions.ShouldContain(innerException2);
    }

    [Fact]
    public void Should_rethrow_aggregate_exception_with_another_example_cause_of_multiple_exceptions_from_inside_delegate__pessimistic()
    {
        var policy = Policy.Timeout(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

        Exception innerException1 = new NotImplementedException();
        Exception innerException2 = new DivideByZeroException();
        Action action = () =>
        {
            Action action1 = () => throw innerException1;
            Action action2 = () => throw innerException2;
            Parallel.Invoke(action1, action2);
        };

        // Whether executing the delegate directly, or through the policy, exception behavior should be the same.
        var ex = Should.Throw<AggregateException>(action);
        ex.InnerExceptions.ShouldContain(innerException1);
        ex.InnerExceptions.ShouldContain(innerException2);

        ex = Should.Throw<AggregateException>(() => policy.Execute(action));
        ex.InnerExceptions.ShouldContain(innerException1);
        ex.InnerExceptions.ShouldContain(innerException2);
    }

    #endregion

    #region Timeout operation - optimistic

    [Fact]
    public void Should_throw_when_timeout_is_less_than_execution_duration__optimistic()
    {
        var policy = Policy.Timeout(TimeSpan.FromMilliseconds(50), TimeoutStrategy.Optimistic);
        var userCancellationToken = CancellationToken;

        // Delegate observes cancellation token, so permitting optimistic cancellation.
        Should.Throw<TimeoutRejectedException>(() => policy.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(3), ct), userCancellationToken));
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_execution_duration__optimistic()
    {
        var policy = Policy.Timeout(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic);
        var result = ResultPrimitive.Undefined;
        var userCancellationToken = CancellationToken;

        Action act = () =>
        {
            result = policy.Execute(ct =>
                {
                    SystemClock.Sleep(TimeSpan.FromMilliseconds(500), ct);
                    return ResultPrimitive.Good;
                }, userCancellationToken);
        };

        Should.NotThrow(act);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_throw_timeout_after_correct_duration__optimistic()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(1);
        var policy = Policy.Timeout(timeout);
        var userCancellationToken = CancellationToken;

        TimeSpan tolerance = TimeSpan.FromSeconds(3);

        var watch = Stopwatch.StartNew();

        // Delegate observes cancellation token, so permitting optimistic cancellation.
        Should.Throw<TimeoutRejectedException>(() => policy.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(10), ct), userCancellationToken));
        watch.Stop();

        watch.Elapsed.ShouldBe(timeout, TimeSpan.FromMilliseconds(tolerance.TotalMilliseconds));
    }

    [Fact]
    public void Should_rethrow_exception_from_inside_delegate__optimistic()
    {
        var policy = Policy.Timeout(TimeSpan.FromSeconds(10), TimeoutStrategy.Optimistic);

        Should.Throw<NotSupportedException>(() => policy.Execute(() => throw new NotSupportedException()));
    }

    #endregion

    #region Non-timeout cancellation - pessimistic (user-delegate does not observe cancellation)

    [Fact]
    public void Should_not_be_able_to_cancel_with_unobserved_user_cancellation_token_before_timeout__pessimistic()
    {
        int timeout = 5;
        var policy = Policy.Timeout(timeout, TimeoutStrategy.Pessimistic);

        using CancellationTokenSource userTokenSource = new CancellationTokenSource();
        Should.Throw<TimeoutRejectedException>(() => policy.Execute(
            _ =>
            {
                userTokenSource.Cancel(); // User token cancels in the middle of execution ...
                SystemClock.Sleep(TimeSpan.FromSeconds(timeout * 2),
                    CancellationToken); // ... but if the executed delegate does not observe it
            },
            userTokenSource.Token)); // ... it's still the timeout we expect.
    }

    [Fact]
    public void Should_not_execute_user_delegate_if_user_cancellationToken_cancelled_before_delegate_reached__pessimistic()
    {
        var policy = Policy.Timeout(10, TimeoutStrategy.Pessimistic);

        bool executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            Should.Throw<OperationCanceledException>(() => policy.Execute(_ => executed = true, cts.Token));
        }

        executed.ShouldBeFalse();
    }

    #endregion

    #region Non-timeout cancellation - optimistic (user-delegate observes cancellation)

    [Fact]
    public void Should_be_able_to_cancel_with_user_cancellation_token_before_timeout__optimistic()
    {
        int timeout = 10;
        var policy = Policy.Timeout(timeout, TimeoutStrategy.Optimistic);

        using CancellationTokenSource userTokenSource = new CancellationTokenSource();

        // Not a TimeoutRejectedException; i.e. policy can distinguish user cancellation from timeout cancellation.
        Should.Throw<OperationCanceledException>(() => policy.Execute(
            ct =>
            {
                userTokenSource.Cancel();

                // Simulate cancel in the middle of execution
                ct.ThrowIfCancellationRequested();
            },
            userTokenSource.Token)); // ... with user token.
    }

    [Fact]
    public void Should_not_execute_user_delegate_if_user_cancellationToken_cancelled_before_delegate_reached__optimistic()
    {
        var policy = Policy.Timeout(10, TimeoutStrategy.Optimistic);

        bool executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            Should.Throw<OperationCanceledException>(() => policy.Execute(_ => executed = true, cts.Token));
        }

        executed.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_mask_user_exception_if_user_exception_overlaps_with_timeout()
    {
        var userException = new Exception();
        var shimTimeSpan = TimeSpan.FromSeconds(0.2);
        var policy = Policy.Timeout(shimTimeSpan, TimeoutStrategy.Optimistic);

        var thrown = Should.Throw<Exception>(() => policy.Execute(_ =>
            {
                try
                {
                    SystemClock.Sleep(shimTimeSpan + shimTimeSpan, CancellationToken);
                }
                catch
                {
                    // Throw a different exception - this exception should not be masked.
                    // The issue of more interest for issue 620 is an edge-case race condition where a user exception is thrown
                    // quasi-simultaneously to timeout (rather than in a manual catch of a timeout as here), but this is a good way to simulate it.
                    // A real-world case can be if timeout occurs while code is marshalling a user-exception into or through the catch block in TimeoutEngine.
                    throw userException;
                }

                throw new InvalidOperationException("This exception should not be thrown. Test should throw for timeout earlier.");

            }, CancellationToken));

        thrown.ShouldNotBeOfType<OperationCanceledException>();
        thrown.ShouldNotBeOfType<TimeoutRejectedException>();
        thrown.ShouldNotBeOfType<InvalidOperationException>();
        thrown.ShouldBeSameAs(userException);
    }

    #endregion

    #region onTimeout overload - pessimistic

    [Fact]
    public void Should_call_ontimeout_with_configured_timeout__pessimistic()
    {
        TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

        TimeSpan? timeoutPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (_, span, _) => timeoutPassedToOnTimeout = span;

        var policy = Policy.Timeout(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeout);

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken)));

        timeoutPassedToOnTimeout.ShouldBe(timeoutPassedToConfiguration);
    }

    [Fact]
    public void Should_call_ontimeout_with_passed_context__pessimistic()
    {
        string operationKey = "SomeKey";
        Context contextPassedToExecute = new Context(operationKey);

        Context? contextPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (ctx, _, _) => contextPassedToOnTimeout = ctx;

        TimeSpan timeout = TimeSpan.FromMilliseconds(250);
        var policy = Policy.Timeout(timeout, TimeoutStrategy.Pessimistic, onTimeout);

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(_ => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken), contextPassedToExecute));

        contextPassedToOnTimeout!.ShouldNotBeNull();
        contextPassedToOnTimeout!.OperationKey.ShouldBe(operationKey);
        contextPassedToOnTimeout!.ShouldBeSameAs(contextPassedToExecute);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__pessimistic(int programaticallyControlledDelay)
    {
        Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay);

        TimeSpan? timeoutPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (_, span, _) => timeoutPassedToOnTimeout = span;

        var policy = Policy.Timeout(timeoutFunc, TimeoutStrategy.Pessimistic, onTimeout);

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken)));

        timeoutPassedToOnTimeout.ShouldBe(timeoutFunc());
    }

    [Fact]
    public void Should_throw_when_timeoutProvider_is_null_with_strategy()
    {
        // Arrange
        Func<TimeSpan> timeoutProvider = null!;
        Action policy = () => Policy.Timeout(timeoutProvider, TimeoutStrategy.Pessimistic);

        // Act
        var exception = Should.Throw<ArgumentNullException>(policy);

        // Assert
        exception.ParamName.ShouldBe("timeoutProvider");
    }

    [Fact]
    public void Should_throw_when_timeoutProvider_is_null_with_strategy_and_ontimeout()
    {
        // Arrange
        Func<TimeSpan> timeoutProvider = null!;
        Action<Context, TimeSpan, Task> onTimeout = (_, _, _) => { };
        Action policy = () => Policy.Timeout(timeoutProvider, TimeoutStrategy.Pessimistic, onTimeout);

        // Act
        var exception = Should.Throw<ArgumentNullException>(policy);

        // Assert
        exception.ParamName.ShouldBe("timeoutProvider");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func_influenced_by_context__pessimistic(int programaticallyControlledDelay)
    {
        Func<Context, TimeSpan> timeoutProvider = ctx => (TimeSpan)ctx["timeout"];

        TimeSpan? timeoutPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (_, span, _) => timeoutPassedToOnTimeout = span;
        var policy = Policy.Timeout(timeoutProvider, TimeoutStrategy.Pessimistic, onTimeout);

        // Supply a programatically-controlled timeout, via the execution context.
        Context context = new Context("SomeOperationKey") { ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay) };

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(_ => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken), context));

        timeoutPassedToOnTimeout.ShouldBe(timeoutProvider(context));
    }

    [Fact]
    public void Should_call_ontimeout_with_task_wrapping_abandoned_action__pessimistic()
    {
        Task? taskPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (_, _, task) => taskPassedToOnTimeout = task;

        TimeSpan timeout = TimeSpan.FromMilliseconds(250);
        var policy = Policy.Timeout(timeout, TimeoutStrategy.Pessimistic, onTimeout);

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken)));

        taskPassedToOnTimeout.ShouldNotBeNull();
    }

    [Fact]
    public void Should_call_ontimeout_with_task_wrapping_abandoned_action_allowing_capture_of_otherwise_unobserved_exception__pessimistic()
    {
        SystemClock.Reset(); // This is the only test which cannot work with the artificial SystemClock of TimeoutSpecsBase.  We want the invoked delegate to continue as far as: throw exceptionToThrow, to genuinely check that the walked-away-from task throws that, and that we pass it to onTimeout.

        // That means we can't use the SystemClock.Sleep(...) within the executed delegate to artificially trigger the timeout cancellation (as for example the test above does).
        // In real execution, it is the .Wait(timeoutCancellationTokenSource.Token) in the timeout implementation which throws for the timeout.  We don't want to go as far as abstracting Task.Wait() out into SystemClock, so we let this test run at real-world speed, not abstracted-clock speed.

        Exception exceptionToThrow = new DivideByZeroException();

        Exception? exceptionObservedFromTaskPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (_, _, task) =>
        {
            task.ContinueWith(t => exceptionObservedFromTaskPassedToOnTimeout = t.Exception!.InnerException);
        };

        TimeSpan shimTimespan = TimeSpan.FromSeconds(1);
        TimeSpan thriceShimTimeSpan = shimTimespan + shimTimespan + shimTimespan;
        var policy = Policy.Timeout(shimTimespan, TimeoutStrategy.Pessimistic, onTimeout);

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(() =>
        {
            SystemClock.Sleep(thriceShimTimeSpan, CancellationToken);
            throw exceptionToThrow;
        }));

        SystemClock.Sleep(thriceShimTimeSpan, CancellationToken);
        exceptionObservedFromTaskPassedToOnTimeout.ShouldNotBeNull();
        exceptionObservedFromTaskPassedToOnTimeout.ShouldBe(exceptionToThrow);

    }

    [Fact]
    public void Should_call_ontimeout_with_timing_out_exception__pessimistic()
    {
        TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

        Exception? exceptionPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task, Exception> onTimeout = (_, _, _, exception) => exceptionPassedToOnTimeout = exception;

        var policy = Policy.Timeout(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeout);

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken)));

        exceptionPassedToOnTimeout.ShouldNotBeNull();
        exceptionPassedToOnTimeout.ShouldBeOfType<OperationCanceledException>();
    }

    #endregion

    #region onTimeout overload - optimistic

    [Fact]
    public void Should_call_ontimeout_with_configured_timeout__optimistic()
    {
        TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

        TimeSpan? timeoutPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (_, span, _) => timeoutPassedToOnTimeout = span;

        var policy = Policy.Timeout(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeout);
        var userCancellationToken = CancellationToken;

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(1), ct), userCancellationToken));

        timeoutPassedToOnTimeout.ShouldBe(timeoutPassedToConfiguration);
    }

    [Fact]
    public void Should_call_ontimeout_with_passed_context__optimistic()
    {
        string operationKey = "SomeKey";
        Context contextPassedToExecute = new Context(operationKey);

        Context? contextPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (ctx, _, _) => contextPassedToOnTimeout = ctx;

        TimeSpan timeout = TimeSpan.FromMilliseconds(250);
        var policy = Policy.Timeout(timeout, TimeoutStrategy.Optimistic, onTimeout);
        var userCancellationToken = CancellationToken;

        Should.Throw<TimeoutRejectedException>(() => policy.Execute((_, ct) => SystemClock.Sleep(TimeSpan.FromSeconds(3), ct), contextPassedToExecute, userCancellationToken));

        contextPassedToOnTimeout!.ShouldNotBeNull();
        contextPassedToOnTimeout!.OperationKey.ShouldBe(operationKey);
        contextPassedToOnTimeout!.ShouldBeSameAs(contextPassedToExecute);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__optimistic(int programaticallyControlledDelay)
    {
        Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay);

        TimeSpan? timeoutPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (_, span, _) => timeoutPassedToOnTimeout = span;

        var policy = Policy.Timeout(timeoutFunc, TimeoutStrategy.Optimistic, onTimeout);
        var userCancellationToken = CancellationToken;

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(3), ct), userCancellationToken));

        timeoutPassedToOnTimeout.ShouldBe(timeoutFunc());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func_influenced_by_context__optimistic(int programaticallyControlledDelay)
    {
        Func<Context, TimeSpan> timeoutProvider = ctx => (TimeSpan)ctx["timeout"];

        TimeSpan? timeoutPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (_, span, _) => timeoutPassedToOnTimeout = span;
        var policy = Policy.Timeout(timeoutProvider, TimeoutStrategy.Optimistic, onTimeout);

        var userCancellationToken = CancellationToken;

        // Supply a programatically-controlled timeout, via the execution context.
        Context context = new Context("SomeOperationKey")
        {
            ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay)
        };

        Should.Throw<TimeoutRejectedException>(() => policy.Execute((_, ct) => SystemClock.Sleep(TimeSpan.FromSeconds(3), ct), context, userCancellationToken));

        timeoutPassedToOnTimeout.ShouldBe(timeoutProvider(context));
    }

    [Fact]
    public void Should_call_ontimeout_but_not_with_task_wrapping_abandoned_action__optimistic()
    {
        Task? taskPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task> onTimeout = (_, _, task) => taskPassedToOnTimeout = task;

        TimeSpan timeout = TimeSpan.FromMilliseconds(250);
        var policy = Policy.Timeout(timeout, TimeoutStrategy.Optimistic, onTimeout);
        var userCancellationToken = CancellationToken;

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(3), ct), userCancellationToken));

        taskPassedToOnTimeout.ShouldBeNull();
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_seconds_with_ontimeout_with_exception()
    {
        Action<Context, TimeSpan, Task, Exception> onTimeout = (_, _, _, _) => { };
        Action policy = () => Policy.Timeout(-10, onTimeout);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("seconds");
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds_with_ontimeout_with_exception()
    {
        Action<Context, TimeSpan, Task, Exception> onTimeout = (_, _, _, _) => { };
        Action policy = () => Policy.Timeout(3, onTimeout);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_seconds_with_ontimeout_with_exception_and_strategy()
    {
        Action<Context, TimeSpan, Task, Exception> onTimeout = (_, _, _, _) => { };
        Action policy = () => Policy.Timeout(-10, TimeoutStrategy.Pessimistic, onTimeout);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("seconds");
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds_with_ontimeout_with_exception_and_strategy()
    {
        Action<Context, TimeSpan, Task, Exception> onTimeout = (_, _, _, _) => { };
        Action policy = () => Policy.Timeout(3, TimeoutStrategy.Pessimistic, onTimeout);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_call_ontimeout_with_timing_out_exception__optimistic()
    {
        TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

        Exception? exceptionPassedToOnTimeout = null;
        Action<Context, TimeSpan, Task, Exception> onTimeout = (_, _, _, exception) => exceptionPassedToOnTimeout = exception;

        var policy = Policy.Timeout(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeout);
        var userCancellationToken = CancellationToken;

        Should.Throw<TimeoutRejectedException>(() => policy.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(1), ct), userCancellationToken));

        exceptionPassedToOnTimeout.ShouldNotBeNull();
        exceptionPassedToOnTimeout.ShouldBeOfType<OperationCanceledException>();
    }

    #endregion
}
