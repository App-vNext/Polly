using FluentAssertions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Polly.Specs.Helpers;
using Polly.Timeout;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.Timeout
{
    [Collection(Polly.Specs.Helpers.Constants.SystemClockDependentTestCollection)]
    public class TimeoutAsyncSpecs : TimeoutSpecsBase
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_timespan()
        {
            Action policy = () => Policy.TimeoutAsync(TimeSpan.Zero);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_seconds()
        {
            Action policy = () => Policy.TimeoutAsync(0);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_timespan()
        {
            Action policy = () => Policy.TimeoutAsync(-TimeSpan.FromHours(1));

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_seconds()
        {
            Action policy = () => Policy.TimeoutAsync(-10);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_timespan()
        {
            Action policy = () => Policy.TimeoutAsync(TimeSpan.FromMilliseconds(1));

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds()
        {
            Action policy = () => Policy.TimeoutAsync(3);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_maxvalue()
        {
            Action policy = () => Policy.TimeoutAsync(TimeSpan.MaxValue);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_seconds_is_maxvalue()
        {
            Action policy = () => Policy.TimeoutAsync(int.MaxValue);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timespan()
        {
            Action policy = () => Policy.TimeoutAsync(TimeSpan.FromMinutes(0.5), null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_seconds()
        {
            Action policy = () => Policy.TimeoutAsync(30, null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_throw_when_timeoutProvider_is_null()
        {
            Action policy = () => Policy.TimeoutAsync((Func<TimeSpan>)null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("timeoutProvider");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timeoutprovider()
        {
            Action policy = () => Policy.TimeoutAsync(() => TimeSpan.FromSeconds(30), null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_be_able_to_configure_with_timeout_func()
        {
            Action policy = () => Policy.TimeoutAsync(() => TimeSpan.FromSeconds(1));

            policy.ShouldNotThrow();
        }

        #endregion

        #region Timeout operation - pessimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__pessimistic()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(50);

            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);

            })).ShouldThrow<TimeoutRejectedException>();
        }

        [Fact]
        public async Task Should_not_throw_when_timeout_is_greater_than_execution_duration__pessimistic()
        {
            var policy = Policy.TimeoutAsync(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

            ResultPrimitive result = ResultPrimitive.Undefined;
            Exception ex = null;

            try
            {
                result = await policy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good))
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            ex.Should().BeNull();
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public void Should_throw_timeout_after_correct_duration__pessimistic()
        {
            Stopwatch watch = new Stopwatch();

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic);

            TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(10), CancellationToken.None).ConfigureAwait(false);

            }))
                .ShouldThrow<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        [Fact]
        public void Should_rethrow_exception_from_inside_delegate__pessimistic()
        {
            var policy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

            policy.Awaiting(p => p.ExecuteAsync(() => { throw new NotImplementedException(); })).ShouldThrow<NotImplementedException>();
        }

        #endregion


        #region Timeout operation - optimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__optimistic()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(50);

            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                
            }, userCancellationToken).ConfigureAwait(false))
            .ShouldThrow<TimeoutRejectedException>();
        }

        [Fact]
        public async Task Should_not_throw_when_timeout_is_greater_than_execution_duration__optimistic()
        {
            var policy = Policy.TimeoutAsync(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic);

            ResultPrimitive result = ResultPrimitive.Undefined;
            Exception ex = null;
            var userCancellationToken = CancellationToken.None;

            try
            {
                result = await policy.ExecuteAsync(ct => Task.FromResult(ResultPrimitive.Good), userCancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ex = e;
            }

            ex.Should().BeNull();
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public void Should_throw_timeout_after_correct_duration__optimistic()
        {
            Stopwatch watch = new Stopwatch();

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(10), ct).ConfigureAwait(false);
                
            }, userCancellationToken).ConfigureAwait(false))
                .ShouldThrow<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        [Fact]
        public void Should_rethrow_exception_from_inside_delegate__optimistic()
        {
            var policy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10), TimeoutStrategy.Optimistic);

            policy.Awaiting(p => p.ExecuteAsync(() => { throw new NotImplementedException(); })).ShouldThrow<NotImplementedException>();
        }

        #endregion

        #region Non-timeout cancellation - pessimistic (user-delegate does not observe cancellation)

        [Fact]
        public void Should_not_be_able_to_cancel_with_unobserved_user_cancellation_token_before_timeout__pessimistic()
        {
            int timeout = 5;
            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic);

            using (CancellationTokenSource userTokenSource = new CancellationTokenSource())
            {
                policy.Awaiting(async p => await p.ExecuteAsync(async
                    _ => {
                        userTokenSource.Cancel(); // User token cancels in the middle of execution ...
                        await SystemClock.SleepAsync(TimeSpan.FromSeconds(timeout * 2), 
                            CancellationToken.None // ... but if the executed delegate does not observe it
                            ).ConfigureAwait(false); 
                    }, userTokenSource.Token)
                   ).ShouldThrow<TimeoutRejectedException>(); // ... it's still the timeout we expect.
            }
        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationtoken_cancelled_before_delegate_reached__pessimistic()
        {
            var policy = Policy.TimeoutAsync(10, TimeoutStrategy.Pessimistic);

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => await p.ExecuteAsync(ct =>
                {
                    executed = true;
                    return TaskHelper.EmptyTask;
                }, cts.Token))
                .ShouldThrow<OperationCanceledException>();
            }

            executed.Should().BeFalse();
        }

        #endregion

        #region Non-timeout cancellation - optimistic (user-delegate observes cancellation)

        [Fact]
        public void Should_be_able_to_cancel_with_user_cancellation_token_before_timeout__optimistic()
        {
            int timeout = 10;
            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic);

            using (CancellationTokenSource userTokenSource = new CancellationTokenSource())
            {
                policy.Awaiting(async p => await p.ExecuteAsync(
                    ct => {
                        userTokenSource.Cancel(); ct.ThrowIfCancellationRequested();   // Simulate cancel in the middle of execution
                        return TaskHelper.EmptyTask;
                    }, userTokenSource.Token) // ... with user token.
                   ).ShouldThrow<OperationCanceledException>();
            }
        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationtoken_cancelled_before_delegate_reached__optimistic()
        {
            var policy = Policy.TimeoutAsync(10, TimeoutStrategy.Optimistic);

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => await p.ExecuteAsync(ct =>
                {
                    executed = true;
                    return TaskHelper.EmptyTask;
                }, cts.Token))
                .ShouldThrow<OperationCanceledException>();
            }

            executed.Should().BeFalse();
        }

        #endregion

        #region onTimeout overload - pessimistic

        [Fact]
        public void Should_call_ontimeout_with_configured_timeout__pessimistic()
        {
            TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                
            }))
            .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
        }

        [Fact]
        public void Should_call_ontimeout_with_passed_context__pessimistic()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context contextPassedToExecute = new Context(executionKey);

            Context contextPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                contextPassedToOnTimeout = ctx;
                return TaskHelper.EmptyTask;
            };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                
                }, contextPassedToExecute))
                .ShouldThrow<TimeoutRejectedException>();

            contextPassedToOnTimeout.Should().NotBeNull();
            contextPassedToOnTimeout.ExecutionKey.Should().Be(executionKey);
            contextPassedToOnTimeout.Should().BeSameAs(contextPassedToExecute);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__pessimistic(int programaticallyControlledDelay)
        {
            Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25*programaticallyControlledDelay);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync(timeoutFunc, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
            }))
            .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutFunc());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func_influenced_by_context__pessimistic(int programaticallyControlledDelay)
        {
            Func<Context, TimeSpan> timeoutProvider = ctx => (TimeSpan)ctx["timeout"];

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync(timeoutProvider, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            // Supply a programatically-controlled timeout, via the execution context.
            Context context = new Context("SomeExecutionKey") { ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay) };

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
            }, context))
            .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutProvider(context));
        }

        [Fact]
        public void Should_call_ontimeout_with_task_wrapping_abandoned_action__pessimistic()
        {
            Task taskPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                taskPassedToOnTimeout = task;
                return TaskHelper.EmptyTask;
            };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                
            }))
            .ShouldThrow<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_call_ontimeout_with_task_wrapping_abandoned_action_allowing_capture_of_otherwise_unobserved_exception__pessimistic()
        {
            SystemClock.Reset(); // This is the only test which cannot work with the artificial SystemClock of TimeoutSpecsBase.  We want the invoked delegate to continue as far as: throw exceptionToThrow, to genuinely check that the walked-away-from task throws that, and that we pass it to onTimeoutAsync.  
            // That means we can't use the SystemClock.SleepAsync(...) within the executed delegate to artificially trigger the timeout cancellation (as for example the test above does).
            // In real execution, it is the .WhenAny() in the timeout implementation which throws for the timeout.  We don't want to go as far as abstracting Task.WhenAny() out into SystemClock, so we let this test run at real-world speed, not abstracted-clock speed.

            Exception exceptionToThrow = new DivideByZeroException();

            Exception exceptionObservedFromTaskPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                task.ContinueWith(t => exceptionObservedFromTaskPassedToOnTimeout = t.Exception.InnerException); // Intentionally not awaited: we want to assign the continuation, but let it run in its own time when the executed delegate eventually completes.
                return TaskHelper.EmptyTask;
            };

            TimeSpan shimTimespan = TimeSpan.FromSeconds(1); // Consider increasing shimTimeSpan if test fails transiently in different environments.
            TimeSpan thriceShimTimeSpan = shimTimespan + shimTimespan + shimTimespan;
            var policy = Policy.TimeoutAsync(shimTimespan, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(thriceShimTimeSpan, CancellationToken.None).ConfigureAwait(false);
                throw exceptionToThrow;
            }))
            .ShouldThrow<TimeoutRejectedException>();

            await SystemClock.SleepAsync(thriceShimTimeSpan, CancellationToken.None).ConfigureAwait(false);
            exceptionObservedFromTaskPassedToOnTimeout.Should().NotBeNull();
            exceptionObservedFromTaskPassedToOnTimeout.Should().Be(exceptionToThrow);

        }

        #endregion


        #region onTimeout overload - optimistic

        [Fact]
        public void Should_call_ontimeout_with_configured_timeout__optimistic()
        {
            TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);

            }, userCancellationToken).ConfigureAwait(false))
            .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
        }

        [Fact]
        public void Should_call_ontimeout_with_passed_context__optimistic()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context contextPassedToExecute = new Context(executionKey);

            Context contextPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                contextPassedToOnTimeout = ctx;
                return TaskHelper.EmptyTask;
            };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);

                }, contextPassedToExecute, userCancellationToken).ConfigureAwait(false))
                .ShouldThrow<TimeoutRejectedException>();

            contextPassedToOnTimeout.Should().NotBeNull();
            contextPassedToOnTimeout.ExecutionKey.Should().Be(executionKey);
            contextPassedToOnTimeout.Should().BeSameAs(contextPassedToExecute);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__optimistic(int programaticallyControlledDelay)
        {
            Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25* programaticallyControlledDelay);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync(timeoutFunc, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);

                }, userCancellationToken).ConfigureAwait(false))
                .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutFunc());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func_influenced_by_context__optimistic(int programaticallyControlledDelay)
        {
            Func<Context, TimeSpan> timeoutProvider = ctx => (TimeSpan)ctx["timeout"];

            TimeSpan? timeoutPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                timeoutPassedToOnTimeout = span;
                return TaskHelper.EmptyTask;
            };

            var policy = Policy.TimeoutAsync(timeoutProvider, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            // Supply a programatically-controlled timeout, via the execution context.
            Context context = new Context("SomeExecutionKey")
            {
                ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay)
            };

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);

            }, context, userCancellationToken).ConfigureAwait(false))
                .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutProvider(context));
        }

        [Fact]
        public void Should_call_ontimeout_but_not_with_task_wrapping_abandoned_action__optimistic()
        {
            Task taskPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                taskPassedToOnTimeout = task;
                return TaskHelper.EmptyTask;
            };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);

            }, userCancellationToken).ConfigureAwait(false))
            .ShouldThrow<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().BeNull();
        }
        
        #endregion

    }
}