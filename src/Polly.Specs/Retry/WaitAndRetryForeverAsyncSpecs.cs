using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Polly.Retry;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.Retry
{
    [Collection(Constants.SystemClockDependentTestCollection)]
    public class WaitAndRetryForeverAsyncSpecs : IDisposable
    {
        public WaitAndRetryForeverAsyncSpecs()
        {
            // do nothing on call to sleep
            SystemClock.SleepAsync = (_, _) => TaskHelper.EmptyTask;
        }

        [Fact]
        public void Should_throw_when_sleep_duration_provider_is_null_without_context()
        {
            Action<Exception, TimeSpan> onRetry = (_, _) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForeverAsync(null, onRetry);

            policy.Should().Throw<ArgumentNullException>().And
                  .ParamName.Should().Be("sleepDurationProvider");
        }

        [Fact]
        public void Should_throw_when_sleep_duration_provider_is_null_with_context()
        {
            Action<Exception, TimeSpan, Context> onRetry = (_, _, _) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForeverAsync(null, onRetry);

            policy.Should().Throw<ArgumentNullException>().And
                  .ParamName.Should().Be("sleepDurationProvider");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_without_context()
        {
            Action<Exception, TimeSpan> nullOnRetry = null;
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForeverAsync(provider, nullOnRetry);

            policy.Should().Throw<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_throw_when_onretry_action_is_null_with_context()
        {
            Action<Exception, TimeSpan, Context> nullOnRetry = null;
            Func<int, Context, TimeSpan> provider = (_, _) => TimeSpan.Zero;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .WaitAndRetryForeverAsync(provider, nullOnRetry);

            policy.Should().Throw<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(_ => TimeSpan.Zero);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(3))
                  .Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetryForeverAsync(_ => TimeSpan.Zero);

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>(3))
                  .Should().NotThrow();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>(_ => false)
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>(_ => false)
                .Or<ArgumentException>(_ => false)
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
        {
            Func<int, TimeSpan> provider = _ => 1.Seconds();

            var policy = Policy
                .Handle<DivideByZeroException>(_ => true)
                .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .Should().NotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
        {
            Func<int, TimeSpan> provider = _ => 1.Seconds();

            var policy = Policy
                .Handle<DivideByZeroException>(_ => true)
                .Or<ArgumentException>(_ => true)
               .WaitAndRetryForeverAsync(provider);

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .Should().NotThrow();
        }

        [Fact]
        public void Should_not_sleep_if_no_retries()
        {
            Func<int, TimeSpan> provider = _ => 1.Seconds();

            var totalTimeSlept = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            SystemClock.Sleep = (span, _) => totalTimeSlept += span.Seconds;

            policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
                  .Should().Throw<NullReferenceException>();

            totalTimeSlept.Should()
                          .Be(0);
        }

        [Fact]
        public async Task Should_call_onretry_on_each_retry_with_the_current_exception()
        {
            var expectedExceptions = new object[] { "Exception #1", "Exception #2", "Exception #3" };
            var retryExceptions = new List<Exception>();
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider, (exception, _) => retryExceptions.Add(exception));

            await policy.RaiseExceptionAsync<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

            retryExceptions
                .Select(x => x.HelpLink)
                .Should()
                .ContainInOrder(expectedExceptions);
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
        {
            var expectedRetryCounts = new[] { 1, 2, 3 };
            var retryCounts = new List<int>();
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider, (_, retryCount, _) => retryCounts.Add(retryCount));

            policy.RaiseExceptionAsync<DivideByZeroException>(3);

            retryCounts.Should()
                .ContainInOrder(expectedRetryCounts);
        }

        [Fact]
        public void Should_not_call_onretry_when_no_retries_are_performed()
        {
            Func<int, TimeSpan> provider = _ => 1.Seconds();
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider, (exception, _) => retryExceptions.Add(exception));

            policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
                  .Should().Throw<ArgumentException>();

            retryExceptions.Should().BeEmpty();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_policy()
        {
            Func<int, Context, TimeSpan> provider = (_, _) => 1.Seconds();

            string contextValue = null;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(
                provider,
                (_, _, context) => contextValue = context["key"].ToString());

            policy.RaiseExceptionAsync<DivideByZeroException>(
                new { key = "original_value" }.AsDictionary()
            );

            contextValue.Should().Be("original_value");

            policy.RaiseExceptionAsync<DivideByZeroException>(
                new { key = "new_value" }.AsDictionary()
            );

            contextValue.Should().Be("new_value");
        }

        [Fact]
        public async Task Should_calculate_retry_timespans_from_current_retry_attempt_and_timespan_provider()
        {
            var expectedRetryWaits = new[]
                {
                    2.Seconds(),
                    4.Seconds(),
                    8.Seconds(),
                    16.Seconds(),
                    32.Seconds()
                };

            var actualRetryWaits = new List<TimeSpan>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (_, timeSpan) => actualRetryWaits.Add(timeSpan)
                );

            await policy.RaiseExceptionAsync<DivideByZeroException>(5);

            actualRetryWaits.Should()
                       .ContainInOrder(expectedRetryWaits);
        }

        [Fact]
        public async Task Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
        {
            Dictionary<Exception, TimeSpan> expectedRetryWaits = new Dictionary<Exception, TimeSpan>(){

                {new DivideByZeroException(), 2.Seconds()},
                {new ArgumentNullException(), 4.Seconds()},
            };

            var actualRetryWaits = new List<TimeSpan>();

            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(
                    sleepDurationProvider: (_, exc, _) => expectedRetryWaits[exc],
                    onRetryAsync: (_, timeSpan, _) =>
                    {
                        actualRetryWaits.Add(timeSpan);
                        return TaskHelper.EmptyTask;
                    });

            using (var enumerator = expectedRetryWaits.GetEnumerator())
            {
                await policy.ExecuteAsync(() => {
                    if (enumerator.MoveNext()) throw enumerator.Current.Key;
                    return TaskHelper.EmptyTask;
                });
            }

            actualRetryWaits.Should().ContainInOrder(expectedRetryWaits.Values);
        }


        [Fact]
        public async Task Should_be_able_to_pass_retry_duration_from_execution_to_sleepDurationProvider_via_context()
        {
            var expectedRetryDuration = 1.Seconds();
            TimeSpan? actualRetryDuration = null;

            TimeSpan defaultRetryAfter = 30.Seconds();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(
                    sleepDurationProvider: (_, context) => context.ContainsKey("RetryAfter") ? (TimeSpan)context["RetryAfter"] : defaultRetryAfter, // Set sleep duration from Context, when available.
                    onRetry: (_, timeSpan, _) => actualRetryDuration = timeSpan // Capture the actual sleep duration that was used, for test verification purposes.
                );

            bool failedOnce = false;
            await policy.ExecuteAsync(async (context, _) =>
            {
                await TaskHelper.EmptyTask; // Run some remote call; maybe it returns a RetryAfter header, which we can pass back to the sleepDurationProvider, via the context.
                context["RetryAfter"] = expectedRetryDuration;

                if (!failedOnce)
                {
                    failedOnce = true;
                    throw new DivideByZeroException();
                }
            },
                new { RetryAfter = defaultRetryAfter }.AsDictionary(), // Can also set an initial value for RetryAfter, in the Context passed into the call.
                CancellationToken.None
                );

            actualRetryDuration.Should().Be(expectedRetryDuration);
        }

        [Fact]
        public void Should_wait_asynchronously_for_async_onretry_delegate()
        {
            // This test relates to https://github.com/App-vNext/Polly/issues/107.
            // An async (...) => { ... } anonymous delegate with no return type may compile to either an async void or an async Task method; which assign to an Action<...> or Func<..., Task> respectively.  However, if it compiles to async void (assigning tp Action<...>), then the delegate, when run, will return at the first await, and execution continues without waiting for the Action to complete, as described by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/08/10265476.aspx
            // If Polly were to declare only an Action<...> delegate for onRetry - but users declared async () => { } onRetry delegates - the compiler would happily assign them to the Action<...>, but the next 'try' would/could occur before onRetry execution had completed.
            // This test ensures the relevant retry policy does have a Func<..., Task> form for onRetry, and that it is awaited before the next try commences.

            TimeSpan shimTimeSpan = TimeSpan.FromSeconds(0.2); // Consider increasing shimTimeSpan if test fails transiently in different environments.

            int executeDelegateInvocations = 0;
            int executeDelegateInvocationsWhenOnRetryExits = 0;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(
                _ => TimeSpan.Zero,
                async (_, _) =>
                {
                    await Task.Delay(shimTimeSpan);
                    executeDelegateInvocationsWhenOnRetryExits = executeDelegateInvocations;
                });

            policy.Awaiting(p => p.ExecuteAsync(async () =>
            {
                executeDelegateInvocations++;
                await TaskHelper.EmptyTask;
                if (executeDelegateInvocations == 1) { throw new DivideByZeroException(); }
            })).Should().NotThrow();

            while (executeDelegateInvocationsWhenOnRetryExits == 0) { } // Wait for the onRetry delegate to complete.

            executeDelegateInvocationsWhenOnRetryExits.Should().Be(1); // If the async onRetry delegate is genuinely awaited, only one execution of the .Execute delegate should have occurred by the time onRetry completes.  If the async onRetry delegate were instead assigned to an Action<...>, then onRetry will return, and the second action execution will commence, before await Task.Delay() completes, leaving executeDelegateInvocationsWhenOnRetryExits == 2.
            executeDelegateInvocations.Should().Be(2);
        }

        [Fact]
        public void Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().NotThrow();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
            };

            cancellationTokenSource.Cancel();

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 2,
                ActionObservesCancellation = true
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(2);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 2,
                ActionObservesCancellation = false
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(2);
        }

        [Fact]
        public void Should_report_cancellation_after_faulting_action_execution_and_cancel_further_retries_if_onRetry_invokes_cancellation()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider,
                (_, _) =>
                {
                    cancellationTokenSource.Cancel();
                });

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = null, // Cancellation during onRetry instead - see above.
                ActionObservesCancellation = false
            };

            policy.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_execute_func_returning_value_when_cancellationToken_not_cancelled()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetryForeverAsync(provider);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            bool? result = null;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            Func<AsyncRetryPolicy, Task> action = async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            policy.Awaiting(action).Should().NotThrow();

            result.Should().BeTrue();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_honour_and_report_cancellation_during_func_execution()
        {
            Func<int, TimeSpan> provider = _ => TimeSpan.Zero;

            var policy = Policy
                .Handle<DivideByZeroException>()
               .WaitAndRetryForeverAsync(provider);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            bool? result = null;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            Func<AsyncRetryPolicy, Task> action = async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true);
            policy.Awaiting(action)
                .Should().Throw<OperationCanceledException>().And.CancellationToken.Should().Be(cancellationToken);

            result.Should().Be(null);

            attemptsInvoked.Should().Be(1);
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
