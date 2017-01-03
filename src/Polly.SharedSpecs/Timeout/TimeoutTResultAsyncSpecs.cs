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
    public class TimeoutTResultAsyncSpecs
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_timespan()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.Zero);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_seconds()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(0);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_timespan()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(-TimeSpan.FromHours(1));

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_seconds()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(-10);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_timespan()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMilliseconds(1));

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(3);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_maxvalue()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.MaxValue);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_seconds_is_maxvalue()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(int.MaxValue);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timespan()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMinutes(0.5), null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_seconds()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(30, null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_throw_when_timeoutProvider_is_null()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>((Func<TimeSpan>)null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("timeoutProvider");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timeoutprovider()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(30), null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeoutAsync");
        }

        [Fact]
        public void Should_be_able_to_configure_with_timeout_func()
        {
            Action policy = () => Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(1));

            policy.ShouldNotThrow();
        }

        #endregion

        #region Timeout operation - pessimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__pessimistic()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(50);

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }).ConfigureAwait(false)).ShouldThrow<TimeoutRejectedException>();
        }

        [Fact]
        public async void Should_not_throw_when_timeout_is_greater_than_execution_duration__pessimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

            ResultPrimitive result = ResultPrimitive.Undefined;
            Exception ex = null;

            try
            {
                result = await policy.ExecuteAsync(() => TaskHelper.FromResult(ResultPrimitive.Good))
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
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

            TimeSpan tolerance = TimeSpan.FromSeconds(1); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(5), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }))
                .ShouldThrow<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        #endregion

        #region Timeout operation - optimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__optimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromMilliseconds(50), TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken).ConfigureAwait(false)).ShouldThrow<TimeoutRejectedException>();
        }

        [Fact]
        public async void Should_not_throw_when_timeout_is_greater_than_execution_duration__optimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic);

            ResultPrimitive result = ResultPrimitive.Undefined;
            Exception ex = null;
            var userCancellationToken = CancellationToken.None;

            try
            {
                result = await policy.ExecuteAsync(ct => TaskHelper.FromResult(ResultPrimitive.Good), userCancellationToken)
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
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            TimeSpan tolerance = TimeSpan.FromSeconds(1); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(5), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken).ConfigureAwait(false))
                .ShouldThrow<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        #endregion

        #region Non-timeout cancellation - pessimistic (user-delegate does not observe cancellation)

        [Fact]
        public void Should_not_be_able_to_cancel_with_user_cancellation_token_before_timeout__pessimistic()
        {
            Stopwatch watch = new Stopwatch();

            int timeout = 5;
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

            TimeSpan tolerance = TimeSpan.FromSeconds(1); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            TimeSpan userTokenExpiry = TimeSpan.FromSeconds(1); // Use of time-based token irrelevant to timeout policy; we just need some user token that cancels independently of policy's internal token.
            using (CancellationTokenSource userTokenSource = new CancellationTokenSource(userTokenExpiry))
            {
                watch.Start();
                policy.Awaiting(async p => await p.ExecuteAsync(async
                    _ => {
                        await SystemClock.SleepAsync(TimeSpan.FromSeconds(timeout + 2), CancellationToken.None).ConfigureAwait(false);  // Simulate cancel in the middle of execution
                        return ResultPrimitive.WhateverButTooLate;
                    }, userTokenSource.Token) // ... with user token.
                   ).ShouldThrow<TimeoutRejectedException>();
                watch.Stop();
            }

            watch.Elapsed.Should().BeCloseTo(TimeSpan.FromSeconds(timeout), ((int)tolerance.TotalMilliseconds));

        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationtoken_cancelled_before_delegate_reached__pessimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(10, TimeoutStrategy.Pessimistic);

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
                {
                    executed = true;
                    await TaskHelper.EmptyTask.ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
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
            Stopwatch watch = new Stopwatch();

            int timeout = 10;
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic);

            TimeSpan tolerance = TimeSpan.FromSeconds(1); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            TimeSpan userTokenExpiry = TimeSpan.FromSeconds(1); // Use of time-based token irrelevant to timeout policy; we just need some user token that cancels independently of policy's internal token.
            using (CancellationTokenSource userTokenSource = new CancellationTokenSource(userTokenExpiry))
            {
                watch.Start();
                policy.Awaiting(async p => await p.ExecuteAsync(async 
                    ct => {
                        await SystemClock.SleepAsync(TimeSpan.FromSeconds(timeout + 2), ct).ConfigureAwait(false);  // Simulate cancel in the middle of execution
                        return ResultPrimitive.WhateverButTooLate;
                    }, userTokenSource.Token) // ... with user token.
                   ).ShouldThrow<OperationCanceledException>();
                watch.Stop();
            }

            watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(timeout * 0.8));
            watch.Elapsed.Should().BeCloseTo(userTokenExpiry, ((int)tolerance.TotalMilliseconds));

        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationtoken_cancelled_before_delegate_reached__optimistic()
        {
            var policy = Policy.TimeoutAsync<ResultPrimitive>(10, TimeoutStrategy.Optimistic);

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
                {
                    executed = true;
                    await TaskHelper.EmptyTask.ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
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

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }))
            .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
        }

        [Fact]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__pessimistic()
        {
            for (int i = 1; i <= 3; i++)
            {
                Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25 * i);

                TimeSpan? timeoutPassedToOnTimeout = null;
                Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
                {
                    timeoutPassedToOnTimeout = span;
                    return TaskHelper.EmptyTask;
                };

                var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutFunc, TimeoutStrategy.Pessimistic, onTimeoutAsync);

                policy.Awaiting(async p => await p.ExecuteAsync(async () =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }))
                .ShouldThrow<TimeoutRejectedException>();

                timeoutPassedToOnTimeout.Should().Be(timeoutFunc());
            }
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
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, contextPassedToExecute))
                .ShouldThrow<TimeoutRejectedException>();

            contextPassedToOnTimeout.Should().NotBeNull();
            contextPassedToOnTimeout.ExecutionKey.Should().Be(executionKey);
            contextPassedToOnTimeout.Should().BeSameAs(contextPassedToExecute);
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
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic, onTimeoutAsync);

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), CancellationToken.None).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }))
            .ShouldThrow<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().NotBeNull();
        }

        [Fact]
        public async void Should_call_ontimeout_with_task_wrapping_abandoned_action_allowing_capture_of_otherwise_unobserved_exception__pessimistic()
        {
            Exception exceptionToThrow = new DivideByZeroException();

            Exception exceptionObservedFromTaskPassedToOnTimeout = null;
            Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
            {
                task.ContinueWith(t => exceptionObservedFromTaskPassedToOnTimeout = t.Exception.InnerException); // Intentionally not awaited: we want to assign the continuation, but let it run in its own time when the executed delegate eventually completes.
                return TaskHelper.EmptyTask;
            };

            TimeSpan shimTimespan = TimeSpan.FromSeconds(1); // Consider increasing shimTimeSpan if test fails transiently in different environments.
            TimeSpan thriceShimTimeSpan = shimTimespan + shimTimespan + shimTimespan;
            var policy = Policy.TimeoutAsync<ResultPrimitive>(shimTimespan, TimeoutStrategy.Pessimistic, onTimeoutAsync);

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

            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken).ConfigureAwait(false))
            .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
        }

        [Fact]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__optimistic()
        {
            for (int i = 1; i <= 3; i++)
            {
                Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25 * i);

                TimeSpan? timeoutPassedToOnTimeout = null;
                Func<Context, TimeSpan, Task, Task> onTimeoutAsync = (ctx, span, task) =>
                {
                    timeoutPassedToOnTimeout = span;
                    return TaskHelper.EmptyTask;
                };

                var policy = Policy.TimeoutAsync<ResultPrimitive>(timeoutFunc, TimeoutStrategy.Optimistic, onTimeoutAsync);
                var userCancellationToken = CancellationToken.None;

                policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
                {
                    await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                    return ResultPrimitive.WhateverButTooLate;
                }, userCancellationToken).ConfigureAwait(false))
                .ShouldThrow<TimeoutRejectedException>();

                timeoutPassedToOnTimeout.Should().Be(timeoutFunc());
            }
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
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, contextPassedToExecute, userCancellationToken).ConfigureAwait(false))
                .ShouldThrow<TimeoutRejectedException>();

            contextPassedToOnTimeout.Should().NotBeNull();
            contextPassedToOnTimeout.ExecutionKey.Should().Be(executionKey);
            contextPassedToOnTimeout.Should().BeSameAs(contextPassedToExecute);
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
            var policy = Policy.TimeoutAsync<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic, onTimeoutAsync);
            var userCancellationToken = CancellationToken.None;

            policy.Awaiting(async p => await p.ExecuteAsync(async ct =>
            {
                await SystemClock.SleepAsync(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken).ConfigureAwait(false))
            .ShouldThrow<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().BeNull();
        }


        #endregion

    }
}