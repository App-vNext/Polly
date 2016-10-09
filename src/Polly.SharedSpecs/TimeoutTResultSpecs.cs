using FluentAssertions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Polly.Specs.Helpers;
using Polly.Timeout;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs
{
    public class TimeoutTResultSpecs
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_timespan()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(TimeSpan.Zero);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_seconds()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(0);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_timespan()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(-TimeSpan.FromHours(1));

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_seconds()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(-10);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_timespan()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(TimeSpan.FromMilliseconds(1));

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(3);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_maxvalue()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(TimeSpan.MaxValue);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_seconds_is_maxvalue()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(int.MaxValue);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timespan()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(TimeSpan.FromMinutes(0.5), null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeout");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_seconds()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(30, null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeout");
        }

        [Fact]
        public void Should_throw_when_timeoutProvider_is_null()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>((Func<TimeSpan>)null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("timeoutProvider");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timeoutprovider()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(() => TimeSpan.FromSeconds(30), null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeout");
        }

        [Fact]
        public void Should_be_able_to_configure_with_timeout_func()
        {
            Action policy = () => Policy.Timeout<ResultPrimitive>(() => TimeSpan.FromSeconds(1));

            policy.ShouldNotThrow();
        }

        #endregion

        #region Timeout operation

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(50);

            var policy = Policy.Timeout<ResultPrimitive>(timeout);

            policy.Invoking(p => p.Execute(() =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None);
                return ResultPrimitive.WhateverButTooLate;
            })).ShouldThrow<TimeoutRejectedException>();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_execution_duration()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(1));

            ResultPrimitive result = ResultPrimitive.Undefined;
            policy.Invoking(p =>
            {
                result = p.Execute(() => ResultPrimitive.Good);
            }).ShouldNotThrow();

            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public void Should_throw_timeout_after_correct_duration()
        {
            Stopwatch watch = new Stopwatch();

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            var policy = Policy.Timeout<ResultPrimitive>(timeout);

            TimeSpan tolerance = TimeSpan.FromSeconds(1); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Invoking(p => p.Execute(() =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(5), CancellationToken.None);
                return ResultPrimitive.WhateverButTooLate;
            }))
                .ShouldThrow<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        #endregion

        #region Non-timeout cancellation

        [Fact]
        public void Should_be_able_to_cancel_with_user_cancellation_token_before_timeout()
        {
            Stopwatch watch = new Stopwatch();

            int timeout = 10;
            var policy = Policy.Timeout<ResultPrimitive>(timeout);

            TimeSpan tolerance = TimeSpan.FromSeconds(1); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            TimeSpan userTokenExpiry = TimeSpan.FromSeconds(1); // Use of time-based token irrelevant to timeout policy; we just need some user token that cancels independently of policy's internal token.
            using (CancellationTokenSource userTokenSource = new CancellationTokenSource(userTokenExpiry))
            {
                watch.Start();
                policy.Invoking(p => p.Execute(
                    ct => {
                        SystemClock.Sleep(TimeSpan.FromSeconds(timeout + 2), ct);  // Simulate cancel in the middle of execution
                        return ResultPrimitive.WhateverButTooLate;
                    }, userTokenSource.Token) // ... with user token.
                   ).ShouldThrow<OperationCanceledException>();
                watch.Stop();
            }

            watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(timeout * 0.8));
            watch.Elapsed.Should().BeCloseTo(userTokenExpiry, ((int)tolerance.TotalMilliseconds));

        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationtoken_cancelled_before_delegate_reached()
        {
            var policy = Policy.Timeout<ResultPrimitive>(10);

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Invoking(p => p.Execute(ct =>
                {
                    executed = true;
                    return ResultPrimitive.WhateverButTooLate;
                }, cts.Token))
                .ShouldThrow<OperationCanceledException>();
            }

            executed.Should().BeFalse();
        }

        #endregion

        #region onTimeout overload

        [Fact]
        public void Should_call_ontimeout_with_configured_timeout()
        {
            TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };

            var policy = Policy.Timeout<ResultPrimitive>(timeoutPassedToConfiguration, onTimeout);

            policy.Invoking(p => p.Execute(() =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(1), CancellationToken.None);
                return ResultPrimitive.WhateverButTooLate;
            }))
            .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
        }

        [Fact]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func()
        {
            for (int i = 1; i <= 3; i++)
            {
                Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25 * i);

                TimeSpan? timeoutPassedToOnTimeout = null;
                Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };

                var policy = Policy.Timeout<ResultPrimitive>(timeoutFunc, onTimeout);

                policy.Invoking(p => p.Execute(() =>
                {
                    SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None);
                    return ResultPrimitive.WhateverButTooLate;
                }))
                .ShouldThrow<TimeoutRejectedException>();

                timeoutPassedToOnTimeout.Should().Be(timeoutFunc());
            }
        }

        [Fact]
        public void Should_call_ontimeout_with_passed_context()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context contextPassedToExecute = new Context(executionKey);

            Context contextPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { contextPassedToOnTimeout = ctx; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout<ResultPrimitive>(timeout, onTimeout);

            policy.Invoking(p => p.Execute(() =>
                {
                    SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None);
                    return ResultPrimitive.WhateverButTooLate;
                }, contextPassedToExecute))
                .ShouldThrow<TimeoutRejectedException>();

            contextPassedToOnTimeout.Should().NotBeNull();
            contextPassedToOnTimeout.ExecutionKey.Should().Be(executionKey);
            contextPassedToOnTimeout.Should().BeSameAs(contextPassedToExecute);
        }

        [Fact]
        public void Should_call_ontimeout_with_task_wrapping_abandoned_action()
        {
            Task taskPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { taskPassedToOnTimeout = task; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout<ResultPrimitive>(timeout, onTimeout);

            policy.Invoking(p => p.Execute(() =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None);
                return ResultPrimitive.WhateverButTooLate;
            }))
            .ShouldThrow<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().NotBeNull();
        }

        [Fact]
        public void Should_call_ontimeout_with_task_wrapping_abandoned_action_allowing_capture_of_otherwise_unobserved_exception()
        {
            Exception exceptionToThrow = new DivideByZeroException();

            Exception exceptionObservedFromTaskPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) =>
            {
                task.ContinueWith(t => exceptionObservedFromTaskPassedToOnTimeout = t.Exception.InnerException);
            };

            TimeSpan shimTimespan = TimeSpan.FromMilliseconds(250);
            TimeSpan thriceShimTimeSpan = shimTimespan + shimTimespan + shimTimespan;
            var policy = Policy.Timeout<ResultPrimitive>(shimTimespan, onTimeout);

            policy.Invoking(p => p.Execute(() =>
            {
                SystemClock.Sleep(thriceShimTimeSpan, CancellationToken.None);
                throw exceptionToThrow;
            }))
            .ShouldThrow<TimeoutRejectedException>();

            SystemClock.Sleep(thriceShimTimeSpan, CancellationToken.None);
            exceptionObservedFromTaskPassedToOnTimeout.Should().NotBeNull();
            exceptionObservedFromTaskPassedToOnTimeout.Should().Be(exceptionToThrow);

        }

        #endregion

    }
}