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
    public class TimeoutTResultSpecs : TimeoutSpecsBase
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

        #region Timeout operation - pessimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__pessimistic()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(50);

            var policy = Policy.Timeout<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

            policy.Invoking(p => p.Execute(() =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None);
                return ResultPrimitive.WhateverButTooLate;
            })).ShouldThrow<TimeoutRejectedException>();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_execution_duration__pessimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Pessimistic);

            ResultPrimitive result = ResultPrimitive.Undefined;
            policy.Invoking(p =>
            {
                result = p.Execute(() => ResultPrimitive.Good);
            }).ShouldNotThrow();

            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public void Should_throw_timeout_after_correct_duration__pessimistic()
        {
            Stopwatch watch = new Stopwatch();

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            var policy = Policy.Timeout<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

            TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Invoking(p => p.Execute(() =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(10), CancellationToken.None);
                return ResultPrimitive.WhateverButTooLate;
            }))
                .ShouldThrow<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        [Fact]
        public void Should_rethrow_exception_from_inside_delegate__pessimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

            policy.Invoking(p => p.Execute(() => { throw new NotImplementedException(); })).ShouldThrow<NotImplementedException>();
        }

        [Fact]
        public void Should_rethrow_aggregate_exception_from_inside_delegate__pessimistic_with_full_stacktrace()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);
            var msg = "Aggregate Exception thrown from the delegate";

            // Check to see if nested aggregate exceptions are unwrapped correctly
            AggregateException exception = new AggregateException(msg, new NotImplementedException());

            policy.Invoking(p => p.Execute(() => { Helper_ThrowException(exception); return ResultPrimitive.WhateverButTooLate; }))
                .ShouldThrow<AggregateException>()
                .WithMessage(exception.Message)
                .WithInnerException<NotImplementedException>()
                .And.StackTrace.Should().Contain("Helper_ThrowException");
        }

        [Fact]
        public void Should_rethrow_aggregate_exception_with_multiple_exceptions_from_inside_delegate__pessimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);
            var msg = "Aggregate Exception thrown from the delegate";

            Exception innerException1 = new NotImplementedException();
            Exception innerException2 = new DivideByZeroException();
            AggregateException aggregateException = new AggregateException(msg, innerException1, innerException2);
            Func<ResultPrimitive> func = () => { Helper_ThrowException(aggregateException); return ResultPrimitive.WhateverButTooLate; };
            Action action = () => { ResultPrimitive throwAway = func(); }; // Helper, because .ShouldThrow<>() does not exist in FluentAssertions on Func<T>.  See https://github.com/fluentassertions/fluentassertions/issues/422

            // Whether executing the delegate directly, or through the policy, exception behavior should be the same.
            action.ShouldThrow<AggregateException>()
                .WithMessage(aggregateException.Message)
                .And.InnerExceptions.Should().BeEquivalentTo<Exception>(new[] { innerException1, innerException2 });

            policy.Invoking(p => p.Execute(func)).ShouldThrow<AggregateException>()
                .WithMessage(aggregateException.Message)
                .And.InnerExceptions.Should().BeEquivalentTo<Exception>(new[] { innerException1, innerException2 });
        }

        [Fact]
        public void Should_rethrow_aggregate_exception_with_example_cause_of_multiple_exceptions_from_inside_delegate__pessimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

            Exception innerException1 = new NotImplementedException();
            Exception innerException2 = new DivideByZeroException();
            Func<ResultPrimitive> func = () =>
            {
                Task task1 = Task.Run(() => { throw innerException1; });
                Task task2 = Task.Run(() => { throw innerException2; });
                Task.WhenAll(task1, task2).Wait();
                return ResultPrimitive.WhateverButTooLate;
            };
            Action action = () => { ResultPrimitive throwAway = func(); }; // Helper, because .ShouldThrow<>() does not exist in FluentAssertions on Func<T>.  See https://github.com/fluentassertions/fluentassertions/issues/422

            // Whether executing the delegate directly, or through the policy, exception behavior should be the same.
            action.ShouldThrow<AggregateException>()
                .And.InnerExceptions.Should().BeEquivalentTo<Exception>(new[] { innerException1, innerException2 });

            policy.Invoking(p => p.Execute(func)).ShouldThrow<AggregateException>()
                .And.InnerExceptions.Should().BeEquivalentTo<Exception>(new[] { innerException1, innerException2 });
        }

        [Fact]
        public void Should_rethrow_aggregate_exception_with_another_example_cause_of_multiple_exceptions_from_inside_delegate__pessimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

            Exception innerException1 = new NotImplementedException();
            Exception innerException2 = new DivideByZeroException();
            Func<ResultPrimitive> func = () =>
            {
                Action action1 = () => { throw innerException1; };
                Action action2 = () => { throw innerException2; };
                Parallel.Invoke(action1, action2);
                return ResultPrimitive.WhateverButTooLate;
            };
            Action action = () => { ResultPrimitive throwAway = func(); }; // Helper, because .ShouldThrow<>() does not exist in FluentAssertions on Func<T>.  See https://github.com/fluentassertions/fluentassertions/issues/422

            // Whether executing the delegate directly, or through the policy, exception behavior should be the same.
            action.ShouldThrow<AggregateException>()
                .And.InnerExceptions.Should().BeEquivalentTo<Exception>(new[] { innerException1, innerException2 });

            policy.Invoking(p => p.Execute(func)).ShouldThrow<AggregateException>()
                .And.InnerExceptions.Should().BeEquivalentTo<Exception>(new[] { innerException1, innerException2 });
        }

        #endregion

        #region Timeout operation - optimistic

        [Fact]
        public void Should_throw_when_timeout_is_less_than_execution_duration__optimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromMilliseconds(50), TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(3), ct);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken))
            .ShouldThrow<TimeoutRejectedException>();
        }

        [Fact]
        public void Should_not_throw_when_timeout_is_greater_than_execution_duration__optimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            ResultPrimitive result = ResultPrimitive.Undefined;
            policy.Invoking(p =>
            {
                result = p.Execute(ct => ResultPrimitive.Good, userCancellationToken);
            }).ShouldNotThrow();

            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public void Should_throw_timeout_after_correct_duration__optimistic()
        {
            Stopwatch watch = new Stopwatch();

            TimeSpan timeout = TimeSpan.FromSeconds(1);
            var policy = Policy.Timeout<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic);
            var userCancellationToken = CancellationToken.None;

            TimeSpan tolerance = TimeSpan.FromSeconds(3); // Consider increasing tolerance, if test fails transiently in different test/build environments.

            watch.Start();
            policy.Invoking(p => p.Execute(ct =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(10), ct);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken))
                .ShouldThrow<TimeoutRejectedException>();
            watch.Stop();

            watch.Elapsed.Should().BeCloseTo(timeout, ((int)tolerance.TotalMilliseconds));
        }

        [Fact]
        public void Should_rethrow_exception_from_inside_delegate__optimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(10), TimeoutStrategy.Optimistic);

            policy.Invoking(p => p.Execute(() => { throw new NotImplementedException(); })).ShouldThrow<NotImplementedException>();
        }

        #endregion

        #region Non-timeout cancellation - pessimistic (user-delegate does not observe cancellation)

        [Fact]
        public void Should_not_be_able_to_cancel_with_unobserved_user_cancellation_token_before_timeout__pessimistic()
        {
            int timeout = 5;
            var policy = Policy.Timeout<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic);

            using (CancellationTokenSource userTokenSource = new CancellationTokenSource())
            {
                policy.Invoking(p => p.Execute(
                    _ => {
                        userTokenSource.Cancel(); // User token cancels in the middle of execution ...
                        SystemClock.Sleep(TimeSpan.FromSeconds(timeout * 2),
                            CancellationToken.None // ... but if the executed delegate does not observe it
                           );
                        return ResultPrimitive.WhateverButTooLate;
                    }, userTokenSource.Token) 
                   ).ShouldThrow<TimeoutRejectedException>(); // ... it's still the timeout we expect.
            }
        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationtoken_cancelled_before_delegate_reached__pessimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(10, TimeoutStrategy.Pessimistic);

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

        #region Non-timeout cancellation - optimistic (user-delegate observes cancellation)

        [Fact]
        public void Should_be_able_to_cancel_with_user_cancellation_token_before_timeout__optimistic()
        {
            int timeout = 10;
            var policy = Policy.Timeout<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic);
            using (CancellationTokenSource userTokenSource = new CancellationTokenSource())
            {
                policy.Invoking(p => p.Execute(
                    ct => {
                        userTokenSource.Cancel(); ct.ThrowIfCancellationRequested(); // Simulate cancel in the middle of execution
                        return ResultPrimitive.WhateverButTooLate;
                    }, userTokenSource.Token) // ... with user token.
                   ).ShouldThrow<OperationCanceledException>(); // Not a TimeoutRejectedException; i.e. policy can distinguish user cancellation from timeout cancellation.
            }
        }

        [Fact]
        public void Should_not_execute_user_delegate_if_user_cancellationtoken_cancelled_before_delegate_reached__optimistic()
        {
            var policy = Policy.Timeout<ResultPrimitive>(10, TimeoutStrategy.Optimistic);

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

        #region onTimeout overload - pessimistic

        [Fact]
        public void Should_call_ontimeout_with_configured_timeout__pessimistic()
        {
            TimeSpan timeoutPassedToConfiguration = TimeSpan.FromMilliseconds(250);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };

            var policy = Policy.Timeout<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Pessimistic, onTimeout);

            policy.Invoking(p => p.Execute(() =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None);
                return ResultPrimitive.WhateverButTooLate;
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
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { contextPassedToOnTimeout = ctx; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic, onTimeout);

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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Should_call_ontimeout_with_timeout_supplied_different_for_each_execution_by_evaluating_func__pessimistic(int programaticallyControlledDelay)
        {

            Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25*programaticallyControlledDelay);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };

            var policy = Policy.Timeout<ResultPrimitive>(timeoutFunc, TimeoutStrategy.Pessimistic, onTimeout);

            policy.Invoking(p => p.Execute(() =>
                {
                    SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None);
                    return ResultPrimitive.WhateverButTooLate;
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
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };
            var policy = Policy.Timeout<ResultPrimitive>(timeoutProvider, TimeoutStrategy.Pessimistic, onTimeout);

            // Supply a programatically-controlled timeout, via the execution context.
            Context context = new Context("SomeExecutionKey") { ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay) };

            policy.Invoking(p => p.Execute(() =>
                {
                    SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None);
                    return ResultPrimitive.WhateverButTooLate;
                }, context))
                .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutProvider(context));
        }

        [Fact]
        public void Should_call_ontimeout_with_task_wrapping_abandoned_action__pessimistic()
        {
            Task taskPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { taskPassedToOnTimeout = task; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout<ResultPrimitive>(timeout, TimeoutStrategy.Pessimistic, onTimeout);

            policy.Invoking(p => p.Execute(() =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(3), CancellationToken.None);
                return ResultPrimitive.WhateverButTooLate;
            }))
            .ShouldThrow<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().NotBeNull();
        }

        [Fact]
        public void Should_call_ontimeout_with_task_wrapping_abandoned_action_allowing_capture_of_otherwise_unobserved_exception__pessimistic()
        {
            SystemClock.Reset(); // This is the only test which cannot work with the artificial SystemClock of TimeoutSpecsBase.  We want the invoked delegate to continue as far as: throw exceptionToThrow, to genuinely check that the walked-away-from task throws that, and that we pass it to onTimeout.  
            // That means we can't use the SystemClock.Sleep(...) within the executed delegate to artificially trigger the timeout cancellation (as for example the test above does).
            // In real execution, it is the .Wait(timeoutCancellationTokenSource.Token) in the timeout implementation which throws for the timeout.  We don't want to go as far as abstracting Task.Wait() out into SystemClock, so we let this test run at real-world speed, not abstracted-clock speed.

            Exception exceptionToThrow = new DivideByZeroException();

            Exception exceptionObservedFromTaskPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) =>
            {
                task.ContinueWith(t => exceptionObservedFromTaskPassedToOnTimeout = t.Exception.InnerException);
            };

            TimeSpan shimTimespan = TimeSpan.FromSeconds(1); // Consider increasing shimTimeSpan if test fails transiently in different environments.
            TimeSpan thriceShimTimeSpan = shimTimespan + shimTimespan + shimTimespan;
            var policy = Policy.Timeout<ResultPrimitive>(shimTimespan, TimeoutStrategy.Pessimistic, onTimeout);

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

            var policy = Policy.Timeout<ResultPrimitive>(timeoutPassedToConfiguration, TimeoutStrategy.Optimistic, onTimeout);
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(1), ct);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken))
            .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutPassedToConfiguration);
        }

        [Fact]
        public void Should_call_ontimeout_with_passed_context__optimistic()
        {
            string executionKey = Guid.NewGuid().ToString();
            Context contextPassedToExecute = new Context(executionKey);

            Context contextPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { contextPassedToOnTimeout = ctx; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic, onTimeout);
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct =>
                {
                    SystemClock.Sleep(TimeSpan.FromSeconds(3), ct);
                    return ResultPrimitive.WhateverButTooLate;
                }, contextPassedToExecute, userCancellationToken))
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
            Func<TimeSpan> timeoutFunc = () => TimeSpan.FromMilliseconds(25*programaticallyControlledDelay);

            TimeSpan? timeoutPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };

            var policy = Policy.Timeout<ResultPrimitive>(timeoutFunc, TimeoutStrategy.Optimistic, onTimeout);
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct =>
                {
                    SystemClock.Sleep(TimeSpan.FromSeconds(3), ct);
                    return ResultPrimitive.WhateverButTooLate;
                }, userCancellationToken))
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
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { timeoutPassedToOnTimeout = span; };
            var policy = Policy.Timeout<ResultPrimitive>(timeoutProvider, TimeoutStrategy.Optimistic, onTimeout);
            var userCancellationToken = CancellationToken.None;

            // Supply a programatically-controlled timeout, via the execution context.
            Context context = new Context("SomeExecutionKey")
            {
                ["timeout"] = TimeSpan.FromMilliseconds(25 * programaticallyControlledDelay)
            };

            policy.Invoking(p => p.Execute(ct =>
                {
                    SystemClock.Sleep(TimeSpan.FromSeconds(3), ct);
                    return ResultPrimitive.WhateverButTooLate;
                }, context, userCancellationToken))
                .ShouldThrow<TimeoutRejectedException>();

            timeoutPassedToOnTimeout.Should().Be(timeoutProvider(context));
        }

        [Fact]
        public void Should_call_ontimeout_but_not_with_task_wrapping_abandoned_action__optimistic()
        {
            Task taskPassedToOnTimeout = null;
            Action<Context, TimeSpan, Task> onTimeout = (ctx, span, task) => { taskPassedToOnTimeout = task; };

            TimeSpan timeout = TimeSpan.FromMilliseconds(250);
            var policy = Policy.Timeout<ResultPrimitive>(timeout, TimeoutStrategy.Optimistic, onTimeout);
            var userCancellationToken = CancellationToken.None;

            policy.Invoking(p => p.Execute(ct =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(3), ct);
                return ResultPrimitive.WhateverButTooLate;
            }, userCancellationToken))
            .ShouldThrow<TimeoutRejectedException>();

            taskPassedToOnTimeout.Should().BeNull();
        }

        #endregion

    }
}