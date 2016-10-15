using FluentAssertions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Polly.Timeout;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs
{
    public class TimeoutSpecs
    {
        #region Configuration

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_timespan()
        {
            Action policy = () => Policy.Timeout(TimeSpan.Zero);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_zero_by_seconds()
        {
            Action policy = () => Policy.Timeout(0);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_timespan()
        {
            Action policy = () => Policy.Timeout(-TimeSpan.FromHours(1));

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("timeout");
        }

        [Fact]
        public void Should_throw_when_timeout_is_less_than_zero_by_seconds()
        {
            Action policy = () => Policy.Timeout(-10);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                .ParamName.Should().Be("seconds");
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_timespan()
        {
            Action policy = () => Policy.Timeout(TimeSpan.FromMilliseconds(1));

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_zero_by_seconds()
        {
            Action policy = () => Policy.Timeout(3);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_maxvalue()
        {
            Action policy = () => Policy.Timeout(TimeSpan.MaxValue);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_timeout_seconds_is_maxvalue()
        {
            Action policy = () => Policy.Timeout(int.MaxValue);

            policy.ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timespan()
        {
            Action policy = () => Policy.Timeout(TimeSpan.FromMinutes(0.5), null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeout");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_seconds()
        {
            Action policy = () => Policy.Timeout(30, null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeout");
        }

        [Fact]
        public void Should_throw_when_timeoutProvider_is_null()
        {
            Action policy = () => Policy.Timeout((Func<TimeSpan>) null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("timeoutProvider");
        }

        [Fact]
        public void Should_throw_when_onTimeout_is_null_with_timeoutprovider()
        {
            Action policy = () => Policy.Timeout(() => TimeSpan.FromSeconds(30), null);

            policy.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("onTimeout");
        }

        [Fact]
        public void Should_be_able_to_configure_with_timeout_func()
        {
            Action policy = () => Policy.Timeout(() => TimeSpan.FromSeconds(1));

            policy.ShouldNotThrow();
        }

        #endregion

        #region Timeout operation - pessimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__pessimistic()
        {
            var policy = Policy.Timeout(TimeSpan.FromMilliseconds(50), TimeoutStrategy.Pessimistic);

            policy.Invoking(p => p.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None)))
                .ShouldThrow<TimeoutRejectedException>();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_execution_duration__pessimistic()
        {
            var policy = Policy
                .Timeout(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

            policy.Invoking(p => p.Execute(() => { })).ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_timeout_after_correct_duration__pessimistic()
        {
            Stopwatch watch = new Stopwatch();

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            var policy = Policy.Timeout(timeout, TimeoutStrategy.Pessimistic);

            TimeSpan tolerance = TimeSpan.FromSeconds(1); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Invoking(p => p.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(5), CancellationToken.None)))
                .ShouldThrow<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        #endregion

        #region Timeout operation - optimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__optimistic()
        {
            var policy = Policy.Timeout(TimeSpan.FromMilliseconds(50), TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(3), ct), userCancellationToken)) // Delegate observes cancellation token, so permitting optimistic cancellation. 
                .ShouldThrow<TimeoutRejectedException>();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_execution_duration__optimistic()
        {
            var policy = Policy
                .Timeout(TimeSpan.FromSeconds(1));
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct => { }, userCancellationToken)).ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_timeout_after_correct_duration__optimistic()
        {
            Stopwatch watch = new Stopwatch();

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            var policy = Policy.Timeout(timeout);
            var userCancellationToken = CancellationToken.None;

            TimeSpan tolerance = TimeSpan.FromSeconds(1); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Invoking(p => p.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(5), ct), userCancellationToken)) // Delegate observes cancellation token, so permitting optimistic cancellation. 
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
            var policy = Policy.Timeout(timeout);

            TimeSpan tolerance = TimeSpan.FromSeconds(1); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            TimeSpan userTokenExpiry = TimeSpan.FromSeconds(1); // Use of time-based token irrelevant to timeout policy; we just need some user token that cancels independently of policy's internal token.
            using (CancellationTokenSource userTokenSource = new CancellationTokenSource(userTokenExpiry))
            {
                watch.Start();
                policy.Invoking(p => p.Execute(
                    ct => SystemClock.Sleep(TimeSpan.FromSeconds(timeout + 2), ct) // Simulate cancel in the middle of execution
                    , userTokenSource.Token) // ... with user token.
                   ).ShouldThrow<OperationCanceledException>();
                watch.Stop();
            }

            watch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(timeout*0.8));
            watch.Elapsed.Should().BeCloseTo(userTokenExpiry, ((int)tolerance.TotalMilliseconds));

        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationtoken_cancelled_before_delegate_reached()
        {
            var policy = Policy.Timeout(10);

            bool executed = false;

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                policy.Invoking(p => p.Execute(ct => { executed = true; }, cts.Token))
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
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };

            var policy = Policy.Timeout(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeout);

            policy.Invoking(p => p.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(1), CancellationToken.None)))
                .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
        }

        [Fact]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__pessimistic()
        {
            for (int i = 1; i <= 3; i++)
            {
                Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25*i);

                TimeSpan? timeoutPassedToOnTimeout = null;
                Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };

                var policy = Policy.Timeout(timeoutFunc, TimeoutStrategy.Pessimistic, onTimeout);

                policy.Invoking(p => p.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None)))
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
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { contextPassedToOnTimeout = ctx; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout(timeout, TimeoutStrategy.Pessimistic, onTimeout);

            policy.Invoking(p => p.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None), contextPassedToExecute))
                .ShouldThrow<TimeoutRejectedException>();

            contextPassedToOnTimeout.Should().NotBeNull();
            contextPassedToOnTimeout.ExecutionKey.Should().Be(executionKey);
            contextPassedToOnTimeout.Should().BeSameAs(contextPassedToExecute);
        }

        [Fact]
        public void Should_call_ontimeout_with_task_wrapping_abandoned_action__pessimistic()
        {
            Task taskPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { taskPassedToOnTimeout = task; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout(timeout, TimeoutStrategy.Pessimistic, onTimeout);

            policy.Invoking(p => p.Execute(() => SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None)))
                .ShouldThrow<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().NotBeNull();
        }

        [Fact]
        public void Should_call_ontimeout_with_task_wrapping_abandoned_action_allowing_capture_of_otherwise_unobserved_exception__pessimistic()
        {
            Exception exceptionToThrow = new DivideByZeroException();

            Exception exceptionObservedFromTaskPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) =>
            {
                task.ContinueWith(t => exceptionObservedFromTaskPassedToOnTimeout = t.Exception.InnerException);
            };

            TimeSpan shimTimespan = TimeSpan.FromSeconds(1);
            TimeSpan thriceShimTimeSpan = shimTimespan + shimTimespan + shimTimespan;
            var policy = Policy.Timeout(shimTimespan, TimeoutStrategy.Pessimistic, onTimeout);

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

        #region onTimeout overload - optimistic

        [Fact]
        public void Should_call_ontimeout_with_configured_timeout__optimistic()
        {
            TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };

            var policy = Policy.Timeout(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeout);
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(1), ct), userCancellationToken))
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
                Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };

                var policy = Policy.Timeout(timeoutFunc, TimeoutStrategy.Optimistic, onTimeout);
                var userCancellationToken = CancellationToken.None;

                policy.Invoking(p => p.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(3), ct), userCancellationToken))
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
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { contextPassedToOnTimeout = ctx; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout(timeout, TimeoutStrategy.Optimistic, onTimeout);
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(3), ct), contextPassedToExecute, userCancellationToken))
                .ShouldThrow<TimeoutRejectedException>();

            contextPassedToOnTimeout.Should().NotBeNull();
            contextPassedToOnTimeout.ExecutionKey.Should().Be(executionKey);
            contextPassedToOnTimeout.Should().BeSameAs(contextPassedToExecute);
        }

        [Fact]
        public void Should_call_ontimeout_but_not_with_task_wrapping_abandoned_action__optimistic()
        {
            Task taskPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { taskPassedToOnTimeout = task; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout(timeout, TimeoutStrategy.Optimistic, onTimeout);
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct => SystemClock.Sleep(TimeSpan.FromSeconds(3), ct), userCancellationToken))
                .ShouldThrow<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().BeNull();
        }
        

        #endregion
    }
}