using System;

namespace Polly.Specs.Timeout;

[Collection(Constants.SystemClockDependentTestCollection)]
public class TimeoutAsyncSpecs : TimeoutSpecsBase
{
    #region Configuration

    [Fact]
    public void Should_throw_when_timeout_is_zero_by_timespan()
    {
        Action policy = () => Policy.TimeoutAsync(TimeSpan.Zero);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("timeout");
    }

    [Fact]
    public void Should_throw_when_timeout_is_zero_by_seconds()
    {
        Action policy = () => Policy.TimeoutAsync(0);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("seconds");
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_timespan()
    {
        Action policy = () => Policy.TimeoutAsync(-TimeSpan.FromHours(1));

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("timeout");
    }

    [Fact]
    public void Should_throw_when_timeout_is_less_than_zero_by_seconds()
    {
        Action policy = () => Policy.TimeoutAsync(-10);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("seconds");
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_timespan()
    {
        Action policy = () => Policy.TimeoutAsync(TimeSpan.FromMilliseconds(1));

        policy.Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds()
    {
        Action policy = () => Policy.TimeoutAsync(3);

        policy.Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_maxvalue()
    {
        Action policy = () => Policy.TimeoutAsync(TimeSpan.MaxValue);

        policy.Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_timeout_seconds_is_maxvalue()
    {
        Action policy = () => Policy.TimeoutAsync(int.MaxValue);

        policy.Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan()
    {
        Action policy = () => Policy.TimeoutAsync(System.Threading.Timeout.InfiniteTimeSpan);

        policy.Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_timeoutstrategy()
    {
        Action policy = () => Policy.TimeoutAsync(System.Threading.Timeout.InfiniteTimeSpan, TimeoutStrategy.Optimistic);

        policy.Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_ontimeout()
    {
        Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, _, _) => TaskHelper.EmptyTask;
        Action policy = () => Policy.TimeoutAsync(System.Threading.Timeout.InfiniteTimeSpan, doNothingAsync);

        policy.Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_infinitetimespan_with_timeoutstrategy_and_ontimeout()
    {
        Func<Context, TimeSpan, Task, Task> doNothingAsync = (_, _, _) => TaskHelper.EmptyTask;
        Action policy = () => Policy.TimeoutAsync(System.Threading.Timeout.InfiniteTimeSpan, TimeoutStrategy.Optimistic, doNothingAsync);

        policy.Should().NotThrow();
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timespan()
    {
        Func<Context, TimeSpan, Task, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync(TimeSpan.FromMinutes(0.5), onTimeout);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timespan_with_full_argument_list_onTimeout()
    {
        Func<Context, TimeSpan, Task, Exception, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync(TimeSpan.FromMinutes(0.5), onTimeout);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_seconds()
    {
        Func<Context, TimeSpan, Task, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync(30, onTimeout);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_seconds_with_full_argument_list_onTimeout()
    {
        Func<Context, TimeSpan, Task, Exception, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync(30, onTimeout);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_timeoutProvider_is_null()
    {
        Action policy = () => Policy.TimeoutAsync((Func<TimeSpan>)null!);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("timeoutProvider");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timeoutprovider()
    {
        Func<Context, TimeSpan, Task, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync(() => TimeSpan.FromSeconds(30), onTimeout);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onTimeoutAsync");
    }

    [Fact]
    public void Should_throw_when_onTimeout_is_null_with_timeoutprovider_with_full_argument_list_onTimeout()
    {
        Func<Context, TimeSpan, Task, Exception, Task> onTimeout = null!;
        Action policy = () => Policy.TimeoutAsync(() => TimeSpan.FromSeconds(30), onTimeout);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onTimeoutAsync");
    }

    [Fact]
    public void Should_be_able_to_configure_with_timeout_func()
    {
        Action policy = () => Policy.TimeoutAsync(() => TimeSpan.FromSeconds(1));

        policy.Should().NotThrow();
    }

    #endregion

    #region Timeout operation - pessimistic

    [Fact]
    public async Task Should_throw_when_timeout_is_less_than_execution_duration__pessimistic()
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(50);

        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic);

        await policy.Awaiting(p => p.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None);

        })).Should().ThrowAsync<TimeoutRejectedException>();
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_execution_duration__pessimistic()
    {
        var policy = Policy.TimeoutAsync(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

        ResultPrimitive result = ResultPrimitive.Undefined;

        Func<Task> act = async () =>
        {
            result = await policy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good));
        };

        act.Should().NotThrowAsync();
        result.Should().Be(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_throw_timeout_after_correct_duration__pessimistic()
    {
        Stopwatch watch = new Stopwatch();

        TimeSpan timeout = TimeSpan.FromSeconds(1);
        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic);

        TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

        watch.Start();
        await policy.Awaiting(p => p.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(10), CancellationToken.None);

        }))
            .Should().ThrowAsync<TimeoutRejectedException>();
        watch.Stop();

        watch.Elapsed.Should().BeCloseTo(timeout, TimeSpan.FromMilliseconds(tolerance.TotalMilliseconds));
    }

    [Fact]
    public async Task Should_rethrow_exception_from_inside_delegate__pessimistic()
    {
        var policy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

        await policy.Awaiting(p => p.ExecuteAsync(() => throw new NotSupportedException())).Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task Should_cancel_downstream_token_on_timeout__pessimistic()
    {
        // It seems that there's a difference in the mocked clock vs. the real time clock.
        // This test does not fail when using the mocked timer.
        // In the TimeoutSpecsBase we actually cancel the combined token. Which hides
        // the fact that it doesn't actually cancel irl.
        SystemClock.Reset();

        var policy = Policy.TimeoutAsync(TimeSpan.FromMilliseconds(200), TimeoutStrategy.Pessimistic);
        bool isCancelled = false;

        var act = () => policy.ExecuteAsync(async (combinedToken) =>
        {
            combinedToken.Register(() => isCancelled = true);
            await SystemClock.SleepAsync(TimeSpan.FromMilliseconds(1000), combinedToken);
        }, CancellationToken.None);

        await act.Should().ThrowAsync<TimeoutRejectedException>();

        isCancelled.Should().BeTrue();
    }

    #endregion

    #region Timeout operation - optimistic

    [Fact]
    public async Task Should_throw_when_timeout_is_less_than_execution_duration__optimistic()
    {
        TimeSpan timeout = TimeSpan.FromMilliseconds(50);

        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic);
        var userCancellationToken = CancellationToken.None;

        await policy.Awaiting(p => p.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);

        }, userCancellationToken))
        .Should().ThrowAsync<TimeoutRejectedException>();
    }

    [Fact]
    public void Should_not_throw_when_timeout_is_greater_than_execution_duration__optimistic()
    {
        var policy = Policy.TimeoutAsync(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic);
        var result = ResultPrimitive.Undefined;
        var userCancellationToken = CancellationToken.None;

        Func<Task> act = async () =>
        {
            result = await policy.ExecuteAsync(async ct =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromMilliseconds(500), ct);
                    return ResultPrimitive.Good;
                }, userCancellationToken);
        };

        act.Should().NotThrowAsync<TimeoutRejectedException>();
        result.Should().Be(ResultPrimitive.Good);
    }

    [Fact]
    public async Task Should_throw_timeout_after_correct_duration__optimistic()
    {
        Stopwatch watch = new Stopwatch();

        TimeSpan timeout = TimeSpan.FromSeconds(1);
        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic);
        var userCancellationToken = CancellationToken.None;

        TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

        watch.Start();
        await policy.Awaiting(p => p.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(10), ct);
        }, userCancellationToken))
            .Should().ThrowAsync<TimeoutRejectedException>();
        watch.Stop();

        watch.Elapsed.Should().BeCloseTo(timeout, TimeSpan.FromMilliseconds(tolerance.TotalMilliseconds));
    }

    [Fact]
    public async Task Should_rethrow_exception_from_inside_delegate__optimistic()
    {
        var policy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10), TimeoutStrategy.Optimistic);

        await policy.Awaiting(p => p.ExecuteAsync(() => throw new NotSupportedException())).Should().ThrowAsync<NotSupportedException>();
    }

    #endregion

    #region Non-timeout cancellation - pessimistic (user-delegate does not observe cancellation)

    [Fact]
    public async Task Should_not_be_able_to_cancel_with_unobserved_user_cancellation_token_before_timeout__pessimistic()
    {
        int timeout = 5;
        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic);

        using CancellationTokenSource userTokenSource = new CancellationTokenSource();
        await policy.Awaiting(p => p.ExecuteAsync(async
            _ =>
        {
            userTokenSource.Cancel(); // User token cancels in the middle of execution ...
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(timeout * 2),
                CancellationToken.None); // ... but if the executed delegate does not observe it
        }, userTokenSource.Token)).Should().ThrowAsync<TimeoutRejectedException>(); // ... it's still the timeout we expect.
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_if_user_cancellationToken_cancelled_before_delegate_reached__pessimistic()
    {
        var policy = Policy.TimeoutAsync(10, TimeoutStrategy.Pessimistic);

        bool executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            await policy.Awaiting(p => p.ExecuteAsync(_ =>
            {
                executed = true;
                return TaskHelper.EmptyTask;
            }, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
        }

        executed.Should().BeFalse();
    }

    #endregion

    #region Non-timeout cancellation - optimistic (user-delegate observes cancellation)

    [Fact]
    public async Task Should_be_able_to_cancel_with_user_cancellation_token_before_timeout__optimistic()
    {
        int timeout = 10;
        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic);

        using CancellationTokenSource userTokenSource = new CancellationTokenSource();
        await policy.Awaiting(p => p.ExecuteAsync(
            ct =>
            {
                userTokenSource.Cancel();
                ct.ThrowIfCancellationRequested();   // Simulate cancel in the middle of execution
                return TaskHelper.EmptyTask;
            }, userTokenSource.Token)) // ... with user token.
           .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Should_not_execute_user_delegate_if_user_cancellationToken_cancelled_before_delegate_reached__optimistic()
    {
        var policy = Policy.TimeoutAsync(10, TimeoutStrategy.Optimistic);

        bool executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            await policy.Awaiting(p => p.ExecuteAsync(_ =>
            {
                executed = true;
                return TaskHelper.EmptyTask;
            }, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
        }

        executed.Should().BeFalse();
    }

    [Fact]
    public async Task Should_not_mask_user_exception_if_user_exception_overlaps_with_timeout()
    {
        var userException = new Exception();
        var shimTimeSpan = TimeSpan.FromSeconds(0.2);
        var policy = Policy.TimeoutAsync(shimTimeSpan, TimeoutStrategy.Optimistic);

        var thrown = await policy.Awaiting(p => p.ExecuteAsync(async _ =>
            {
                try
                {
                    await SystemClock.SleepAsync(shimTimeSpan + shimTimeSpan, CancellationToken.None);
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

            }, CancellationToken.None))
            .Should()
            .ThrowAsync<Exception>();

        thrown.NotBeOfType<OperationCanceledException>();
        thrown.NotBeOfType<TimeoutRejectedException>();
        thrown.NotBeOfType<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(userException);
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

        var policy = Policy.TimeoutAsync(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await policy.Awaiting(p => p.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None);
        }))
        .Should().ThrowAsync<TimeoutRejectedException>();

        timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
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
        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await policy.Awaiting(p => p.ExecuteAsync(async _ =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None);
            }, contextPassedToExecute))
            .Should().ThrowAsync<TimeoutRejectedException>();

        contextPassedToOnTimeout!.Should().NotBeNull();
        contextPassedToOnTimeout!.OperationKey.Should().Be(operationKey);
        contextPassedToOnTimeout!.Should().BeSameAs(contextPassedToExecute);
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

        var policy = Policy.TimeoutAsync(timeoutFunc, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await policy.Awaiting(p => p.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None);
        }))
        .Should().ThrowAsync<TimeoutRejectedException>();

        timeoutPassedToOnTimeout.Should().Be(timeoutFunc());
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

        var policy = Policy.TimeoutAsync(timeoutProvider, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        // Supply a programatically-controlled timeout, via the execution context.
        Context context = new Context("SomeOperationKey") { ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay) };

        await policy.Awaiting(p => p.ExecuteAsync(async _ =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None);
        }, context))
        .Should().ThrowAsync<TimeoutRejectedException>();

        timeoutPassedToOnTimeout.Should().Be(timeoutProvider(context));
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
        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await policy.Awaiting(p => p.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None);
        }))
        .Should().ThrowAsync<TimeoutRejectedException>();

        taskPassedToOnTimeout.Should().NotBeNull();
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
        var policy = Policy.TimeoutAsync(shimTimespan, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await policy.Awaiting(p => p.ExecuteAsync(async () =>
        {
            await SystemClock.SleepAsync(thriceShimTimeSpan, CancellationToken.None);
            throw exceptionToThrow;
        }))
        .Should().ThrowAsync<TimeoutRejectedException>();

        await SystemClock.SleepAsync(thriceShimTimeSpan, CancellationToken.None);
        exceptionObservedFromTaskPassedToOnTimeout.Should().NotBeNull();
        exceptionObservedFromTaskPassedToOnTimeout.Should().Be(exceptionToThrow);

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

        var policy = Policy.TimeoutAsync(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeoutAsync);

        await policy.Awaiting(p => p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None);
            }))
            .Should().ThrowAsync<TimeoutRejectedException>();

        exceptionPassedToOnTimeout.Should().NotBeNull();
        exceptionPassedToOnTimeout.Should().BeOfType(typeof(OperationCanceledException));
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

        var policy = Policy.TimeoutAsync(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken.None;

        await policy.Awaiting(p => p.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
        }, userCancellationToken))
        .Should().ThrowAsync<TimeoutRejectedException>();

        timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
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
        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken.None;

        await policy.Awaiting(p => p.ExecuteAsync(async (_, ct) =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
            }, contextPassedToExecute, userCancellationToken))
            .Should().ThrowAsync<TimeoutRejectedException>();

        contextPassedToOnTimeout!.Should().NotBeNull();
        contextPassedToOnTimeout!.OperationKey.Should().Be(operationKey);
        contextPassedToOnTimeout!.Should().BeSameAs(contextPassedToExecute);
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

        var policy = Policy.TimeoutAsync(timeoutFunc, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken.None;

        await policy.Awaiting(p => p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
            }, userCancellationToken))
            .Should().ThrowAsync<TimeoutRejectedException>();

        timeoutPassedToOnTimeout.Should().Be(timeoutFunc());
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

        var policy = Policy.TimeoutAsync(timeoutProvider, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken.None;

        // Supply a programatically-controlled timeout, via the execution context.
        Context context = new Context("SomeOperationKey")
        {
            ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay)
        };

        await policy.Awaiting(p => p.ExecuteAsync(async (_, ct) =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);

        }, context, userCancellationToken))
            .Should().ThrowAsync<TimeoutRejectedException>();

        timeoutPassedToOnTimeout.Should().Be(timeoutProvider(context));
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
        var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken.None;

        await policy.Awaiting(p => p.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
        }, userCancellationToken))
        .Should().ThrowAsync<TimeoutRejectedException>();

        taskPassedToOnTimeout.Should().BeNull();
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

        var policy = Policy.TimeoutAsync(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeoutAsync);
        var userCancellationToken = CancellationToken.None;

        await policy.Awaiting(p => p.ExecuteAsync(async ct =>
        {
            await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct);
        }, userCancellationToken))
        .Should().ThrowAsync<TimeoutRejectedException>();

        exceptionPassedToOnTimeout.Should().NotBeNull();
        exceptionPassedToOnTimeout.Should().BeOfType(typeof(OperationCanceledException));
    }

    #endregion
}
