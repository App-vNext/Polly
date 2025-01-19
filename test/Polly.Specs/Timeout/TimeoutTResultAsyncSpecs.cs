﻿namespace Polly.Specs.Timeout;

[Collection(Constants.SystemClockDependentTestCollection)]
public class TimeoutTResultAsyncSpecs : TimeoutSpecsBase
{
    #region Configuration

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;
        Func<Context, TimeSpan> timeoutProvider = (_) => TimeSpan.Zero;
        TimeoutStrategy timeoutStrategy = TimeoutStrategy.Optimistic;
        Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync = (_, _, _, _) => Task.CompletedTask;

        var instance = Activator.CreateInstance(
            typeof(AsyncTimeoutPolicy<EmptyStruct>),
            flags,
            null,
            [timeoutProvider, timeoutStrategy, onTimeoutAsync],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_timeout_is_zero_by_timespan()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.Zero);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("timeout");
    }

    [Fact]
    public void Should_throw_when_timeout_is_zero_by_seconds()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(0);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("seconds");
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_timespan()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(-TimeSpan.FromHours(1));

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("timeout");
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_seconds()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(-10);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("seconds");
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_timespan()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMilliseconds(1));

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(3);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_maxvalue()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.MaxValue);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_seconds_is_maxvalue()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(int.MaxValue);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(System.Threading.Timeout.InfiniteTimeSpan);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_timeoutstrategy()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(System.Threading.Timeout.InfiniteTimeSpan, TimeoutStrategy.Optimistic);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_ontimeout()
    {
        Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, _, _) => TaskHelper.EmptyTask;
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(System.Threading.Timeout.InfiniteTimeSpan, doNothingAsync);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_timeoutstrategy_and_ontimeout()
    {
        Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, _, _) => TaskHelper.EmptyTask;
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(System.Threading.Timeout.InfiniteTimeSpan, TimeoutStrategy.Optimistic, doNothingAsync);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timespan()
    {
        Func<Context, TimeSpan, Task, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMinutes(0.5), onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timespan_with_full_argument_list_onTimeout()
    {
        Func<Context, TimeSpan, Task, Exception, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMinutes(0.5), onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_seconds()
    {
        Func<Context, TimeSpan, Task, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(30, onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_seconds_with_full_argument_list_onTimeout()
    {
        Func<Context, TimeSpan, Task, Exception, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(30, onTimeout);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_timeoutProvider_is_null()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>((Func<TimeSpan>)null!);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("timeoutProvider");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timeoutprovider()
    {
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = null!;
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(30), onTimeoutAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timeoutprovider_for_full_argument_list_onTimeout()
    {
        Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync = null!;
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(30), onTimeoutAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onTimeoutAsync");
    }

    [Fact]
    public void Should_be_able_to_configure_with_timeout_func()
    {
        Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(1));

        Should.NotThrow(policy);
    }

    #endregion

    #region Timeout operation - pessimistic

    [Fact]
    public async Task Should_throw_when_timeout_is_less_than_execution_duration__pessimistic()
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(50);

        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken);
            return ResultPrimitive.WhateverButTooLate;
        }));
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_execution_duration__pessimistic()
    {
        var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

        ResultPrimitive result = ResultPrimitive.Undefined;

        Func<Task> act = async () => result = await policy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good));

        Should.NotThrowAsync(act);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_throw_timeout_after_correct_duration__pessimistic()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(1);
        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

        TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

        Stopwatch watch = Stopwatch.StartNew();

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(10), CancellationToken);
            return ResultPrimitive.WhateverButTooLate;
        }));

        watch.Stop();
        watch.Elapsed.ShouldBe(timeout, tolerance);
    }

    [Fact]
    public async Task Should_rethrow_exception_from_inside_delegate__pessimistic()
    {
        var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

        await Should.ThrowAsync<NotSupportedException>(() => policy.ExecuteAsync(() => throw new NotSupportedException()));
    }

    #endregion

    #region Timeout operation - optimistic

    [Fact]
    public async Task Should_throw_when_timeout_is_less_than_execution_duration__optimistic()
    {
        var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMilliseconds(50), TimeoutStrategy.Optimistic);
        var userCancellationToken = CancellationToken;

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
            return ResultPrimitive.WhateverButTooLate;
        }, userCancellationToken));
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_execution_duration__optimistic()
    {
        var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic);

        ResultPrimitive result = ResultPrimitive.Undefined;
        var userCancellationToken = CancellationToken;

        Func<Task> act = async () => result = await policy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Good), userCancellationToken);

        Should.NotThrowAsync(act);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_throw_timeout_after_correct_duration__optimistic()
    {
        TimeSpan timeout = TimeSpan.FromSeconds(1);
        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic);
        var userCancellationToken = CancellationToken;

        TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

        Stopwatch watch = Stopwatch.StartNew();
        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(10), ct);
            return ResultPrimitive.WhateverButTooLate;
        }, userCancellationToken));

        watch.Stop();
        watch.Elapsed.ShouldBe(timeout, tolerance);
    }

    [Fact]
    public async Task Should_rethrow_exception_from_inside_delegate__optimistic()
    {
        var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Optimistic);

        await Should.ThrowAsync<NotSupportedException>(() => policy.ExecuteAsync(() => throw new NotSupportedException()));
    }

    #endregion

    #region Non-timeout cancellation - pessimistic (user-delegate does not observe cancellation)

    [Fact]
    public async Task Should_not_be_able_to_cancel_with_unobserved_user_cancellation_token_before_timeout__pessimistic()
    {
        int timeout = 5;
        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

        using CancellationTokenSource userTokenSource = new CancellationTokenSource();
        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async
            _ =>
        {
            userTokenSource.Cancel(); // User token cancels in the middle of execution ...
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(timeout * 2),
                CancellationToken); // ... but if the executed delegate does not observe it
            return ResultPrimitive.WhateverButTooLate;
        }, userTokenSource.Token));
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_if_user_cancellationToken_cancelled_before_delegate_reached__pessimistic()
    {
        var policy = Policy.TimeoutAsync<ResultPrimitive>(10, TimeoutStrategy.Pessimistic);

        bool executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            await Should.ThrowAsync<OperationCanceledException>(() => policy.ExecuteAsync(async _ =>
            {
                executed = true;
                await TaskHelper.EmptyTask;
                return ResultPrimitive.WhateverButTooLate;
            }, cts.Token));
        }

        executed.ShouldBeFalse();
    }

    #endregion

    #region Non-timeout cancellation - optimistic (user-delegate observes cancellation)

    [Fact]
    public async Task Should_be_able_to_cancel_with_user_cancellation_token_before_timeout__optimistic()
    {
        int timeout = 10;
        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic);
        using CancellationTokenSource userTokenSource = new CancellationTokenSource();
        await Should.ThrowAsync<OperationCanceledException>(() => policy.ExecuteAsync(
            ct =>
            {
                userTokenSource.Cancel();
                ct.ThrowIfCancellationRequested();   // Simulate cancel in the middle of execution
                return Task.FromResult(ResultPrimitive.WhateverButTooLate);
            }, userTokenSource.Token)); // ... with user token.
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_if_user_cancellationToken_cancelled_before_delegate_reached__optimistic()
    {
        var policy = Policy.TimeoutAsync<ResultPrimitive>(10, TimeoutStrategy.Optimistic);

        bool executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            await Should.ThrowAsync<OperationCanceledException>(() => policy.ExecuteAsync(async _ =>
            {
                executed = true;
                await TaskHelper.EmptyTask;
                return ResultPrimitive.WhateverButTooLate;
            }, cts.Token));
        }

        executed.ShouldBeFalse();
    }

    #endregion

    #region onTimeout overload - pessimistic

    [Fact]
    public async Task Should_call_ontimeout_with_configured_timeout__pessimistic()
    {
        TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

        TimeSpan? timeoutPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (_, span, _) =>
        {
            timeoutPassedToOnTimeout = span;
            return TaskHelper.EmptyTask;
        };

        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken);
            return ResultPrimitive.WhateverButTooLate;
        }));

        timeoutPassedToOnTimeout.ShouldBe(timeoutPassedToConfiguration);
    }

    [Fact]
    public async Task Should_call_ontimeout_with_passed_context__pessimistic()
    {
        string operationKey = "SomeKey";
        Context contextPassedToExecute = new Context(operationKey);

        Context? contextPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, _, _) =>
        {
            contextPassedToOnTimeout = ctx;
            return TaskHelper.EmptyTask;
        };

        TimeSpan timeout = TimeSpan.FromMilliseconds(250);
        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async _ =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken);
                return ResultPrimitive.WhateverButTooLate;
            }, contextPassedToExecute));

        contextPassedToOnTimeout!.ShouldNotBeNull();
        contextPassedToOnTimeout!.OperationKey.ShouldBe(operationKey);
        contextPassedToOnTimeout!.ShouldBeSameAs(contextPassedToExecute);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__pessimistic(int programaticallyControlledDelay)
    {
        Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay);

        TimeSpan? timeoutPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (_, span, _) =>
        {
            timeoutPassedToOnTimeout = span;
            return TaskHelper.EmptyTask;
        };

        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutFunc, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken);
            return ResultPrimitive.WhateverButTooLate;
        }));

        timeoutPassedToOnTimeout.ShouldBe(timeoutFunc());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func_influenced_by_context__pessimistic(int programaticallyControlledDelay)
    {
        Func<Context, TimeSpan> timeoutProvider = ctx => (TimeSpan)ctx["timeout"];

        TimeSpan? timeoutPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (_, span, _) =>
        {
            timeoutPassedToOnTimeout = span;
            return TaskHelper.EmptyTask;
        };

        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutProvider, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        // Supply a programatically-controlled timeout, via the execution context.
        Context context = new Context("SomeOperationKey") { ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay) };

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async _ =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken);
            return ResultPrimitive.WhateverButTooLate;
        }, context));

        timeoutPassedToOnTimeout.ShouldBe(timeoutProvider(context));
    }

    [Fact]
    public async Task Should_call_ontimeout_with_task_wrapping_abandoned_action__pessimistic()
    {
        Task? taskPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (_, _, task) =>
        {
            taskPassedToOnTimeout = task;
            return TaskHelper.EmptyTask;
        };

        TimeSpan timeout = TimeSpan.FromMilliseconds(250);
        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken);
            return ResultPrimitive.WhateverButTooLate;
        }));

        Assert.NotNull(taskPassedToOnTimeout);
    }

    [Fact]
    public async Task Should_call_ontimeout_with_task_wrapping_abandoned_action_allowing_capture_of_otherwise_unobserved_exception__pessimistic()
    {
        SystemClock.Reset(); // This is the only test which cannot work with the artificial SystemClock of TimeoutSpecsBase.  We want the invoked delegate to continue as far as: throw exceptionToThrow, to genuinely check that the walked-away-from task throws that, and that we pass it to onTimeoutAsync.

        // That means we can't use the SystemClock.SleepAsync(...) within the executed delegate to artificially trigger the timeout cancellation (as for example the test above does).
        // In real execution, it is the .WhenAny() in the timeout implementation which throws for the timeout.  We don't want to go as far as abstracting Task.WhenAny() out into SystemClock, so we let this test run at real-world speed, not abstracted-clock speed.

        Exception exceptionToThrow = new DivideByZeroException();

        Exception? exceptionObservedFromTaskPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (_, _, task) =>
        {
            task.ContinueWith(t => exceptionObservedFromTaskPassedToOnTimeout = t.Exception!.InnerException); // Intentionally not awaited: we want to assign the continuation, but let it run in its own time when the executed delegate eventually completes.
            return TaskHelper.EmptyTask;
        };

        TimeSpan shimTimespan = TimeSpan.FromSeconds(1); // Consider increasing shimTimeSpan if test fails transiently in different environments.
        TimeSpan thriceShimTimeSpan = shimTimespan + shimTimespan + shimTimespan;
        var policy = Policy.TimeoutAsync<ResultPrimitive>(shimTimespan, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(thriceShimTimeSpan, CancellationToken);
            throw exceptionToThrow;
        }));

        await SystemClock.SleepAsync(thriceShimTimeSpan, CancellationToken);
        exceptionObservedFromTaskPassedToOnTimeout.ShouldNotBeNull();
        exceptionObservedFromTaskPassedToOnTimeout.ShouldBe(exceptionToThrow);

    }

    [Fact]
    public async Task Should_call_ontimeout_with_timing_out_exception__pessimistic()
    {
        TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

        Exception? exceptionPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync = (_, _, _, exception) =>
        {
            exceptionPassedToOnTimeout = exception;
            return TaskHelper.EmptyTask;
        };

        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken);
            return ResultPrimitive.WhateverButTooLate;
        }));

        exceptionPassedToOnTimeout.ShouldNotBeNull();
        exceptionPassedToOnTimeout.ShouldBeOfType<OperationCanceledException>();
    }

    #endregion

    #region onTimeout overload - optimistic

    [Fact]
    public async Task Should_call_ontimeout_with_configured_timeout__optimistic()
    {
        TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

        TimeSpan? timeoutPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (_, span, _) =>
        {
            timeoutPassedToOnTimeout = span;
            return TaskHelper.EmptyTask;
        };

        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken;

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
            return ResultPrimitive.WhateverButTooLate;
        }, userCancellationToken));

        timeoutPassedToOnTimeout.ShouldBe(timeoutPassedToConfiguration);
    }

    [Fact]
    public async Task Should_call_ontimeout_with_passed_context__optimistic()
    {
        string operationKey = "SomeKey";
        Context contextPassedToExecute = new Context(operationKey);

        Context? contextPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, _, _) =>
        {
            contextPassedToOnTimeout = ctx;
            return TaskHelper.EmptyTask;
        };

        TimeSpan timeout = TimeSpan.FromMilliseconds(250);
        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken;

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async (_, ct) =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
            return ResultPrimitive.WhateverButTooLate;
        }, contextPassedToExecute, userCancellationToken));

        contextPassedToOnTimeout.ShouldNotBeNull();
        contextPassedToOnTimeout.OperationKey.ShouldBe(operationKey);
        contextPassedToOnTimeout.ShouldBeSameAs(contextPassedToExecute);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__optimistic(int programaticallyControlledDelay)
    {
        Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay);

        TimeSpan? timeoutPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (_, span, _) =>
        {
            timeoutPassedToOnTimeout = span;
            return TaskHelper.EmptyTask;
        };

        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutFunc, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken;

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
            return ResultPrimitive.WhateverButTooLate;
        }, userCancellationToken));

        timeoutPassedToOnTimeout.ShouldBe(timeoutFunc());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func_influenced_by_context__optimistic(int programaticallyControlledDelay)
    {
        Func<Context, TimeSpan> timeoutProvider = ctx => (TimeSpan)ctx["timeout"];

        TimeSpan? timeoutPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (_, span, _) =>
        {
            timeoutPassedToOnTimeout = span;
            return TaskHelper.EmptyTask;
        };

        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutProvider, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken;

        // Supply a programatically-controlled timeout, via the execution context.
        Context context = new Context("SomeOperationKey")
        {
            ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay)
        };

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async (_, ct) =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
            return ResultPrimitive.WhateverButTooLate;
        }, context, userCancellationToken));

        timeoutPassedToOnTimeout.ShouldBe(timeoutProvider(context));
    }

    [Fact]
    public async Task Should_call_ontimeout_but_not_with_task_wrapping_abandoned_action__optimistic()
    {
        Task? taskPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (_, _, task) =>
        {
            taskPassedToOnTimeout = task;
            return TaskHelper.EmptyTask;
        };

        TimeSpan timeout = TimeSpan.FromMilliseconds(250);
        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken;

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
            return ResultPrimitive.WhateverButTooLate;
        }, userCancellationToken));

        taskPassedToOnTimeout.ShouldBeNull();
    }

    [Fact]
    public async Task Should_call_ontimeout_with_timing_out_exception__optimistic()
    {
        TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

        Exception? exceptionPassedToOnTimeout = null;
        Func<Context, TimeSpan, Task, Exception, Task> onTimeoutAsync = (_, _, _, exception) =>
        {
            exceptionPassedToOnTimeout = exception;
            return TaskHelper.EmptyTask;
        };

        var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken;

        await Should.ThrowAsync<TimeoutRejectedException>(() => policy.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
            return ResultPrimitive.WhateverButTooLate;
        }, userCancellationToken));

        exceptionPassedToOnTimeout.ShouldNotBeNull();
        exceptionPassedToOnTimeout.ShouldBeOfType<OperationCanceledException>();
    }

    #endregion
}
