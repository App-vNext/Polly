using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.Retry
{
    public class RetryForeverAsyncSpecs
    {
        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

            policy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .RetryForeverAsync();

            policy.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

            policy.Awaiting(async x => await x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .RetryForeverAsync();

            policy.Awaiting(async x => await x.RaiseExceptionAsync<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .RetryForeverAsync();

            policy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Or<ArgumentException>(e => false)
                .RetryForeverAsync();

            policy.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .RetryForeverAsync();

            policy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Or<ArgumentException>(e => true)
                .RetryForeverAsync();

            policy.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public async Task Should_call_onretry_on_each_retry_with_the_current_exception()
        {
            var expectedExceptions = new object[] {"Exception #1", "Exception #2", "Exception #3"};
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync(exception => retryExceptions.Add(exception));

            await policy.RaiseExceptionAsync<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

            retryExceptions
                .Select(x => x.HelpLink)
                .Should()
                .ContainInOrder(expectedExceptions);
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync((_, context) => contextData = context);

            policy.RaiseExceptionAsync<DivideByZeroException>(
                new { key1 = "value1", key2 = "value2" }.AsDictionary()
            );

            contextData.Should()
                       .ContainKeys("key1", "key2").And
                       .ContainValues("value1", "value2");
        }

        [Fact]
        public void Context_should_be_empty_if_execute_not_called_with_any_data()
        {
            Context capturedContext = null;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync((_, context) => capturedContext = context);

            policy.Awaiting(async x => await x.RaiseExceptionAsync<DivideByZeroException>()).ShouldNotThrow();

            capturedContext.Should()
                           .BeEmpty();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_execute()
        {
            string contextValue = null;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync((_, context) => contextValue = context["key"].ToString());

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
        public void Should_not_call_onretry_when_no_retries_are_performed()
        {
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync(exception => retryExceptions.Add(exception));

            policy.Awaiting(async x => await x.RaiseExceptionAsync<ArgumentException>())
                  .ShouldThrow<ArgumentException>();

            retryExceptions.Should()
                           .BeEmpty();
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
                .RetryForeverAsync(async ex =>
                {
                    await Task.Delay(shimTimeSpan).ConfigureAwait(false);
                    executeDelegateInvocationsWhenOnRetryExits = executeDelegateInvocations;
                });

            policy.Awaiting(async p => await p.ExecuteAsync(async () =>
            {
                executeDelegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                if (executeDelegateInvocations == 1) { throw new DivideByZeroException(); }
            })).ShouldNotThrow();

            while (executeDelegateInvocationsWhenOnRetryExits == 0) { } // Wait for the onRetry delegate to complete.

            executeDelegateInvocationsWhenOnRetryExits.Should().Be(1); // If the async onRetry delegate is genuinely awaited, only one execution of the .Execute delegate should have occurred by the time onRetry completes.  If the async onRetry delegate were instead assigned to an Action<...>, then onRetry will return, and the second action execution will commence, before await Task.Delay() completes, leaving executeDelegateInvocationsWhenOnRetryExits == 2.  
            executeDelegateInvocations.Should().Be(2);
        }

        [Fact]
        public void Should_execute_action_when_non_faulting_and_cancellationtoken_not_cancelled()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationtoken_cancelled_before_execute()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

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

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

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

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

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

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

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

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

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

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(2);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

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

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(2);
        }

        [Fact]
        public void Should_report_cancellation_after_faulting_action_execution_and_cancel_further_retries_if_onRetry_invokes_cancellation()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync(
                _ =>
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

            policy.Awaiting(async x => await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_execute_func_returning_value_when_cancellationtoken_not_cancelled()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            bool? result = null;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            policy.Awaiting(async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true).ConfigureAwait(false))
                .ShouldNotThrow();

            result.Should().BeTrue();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_honour_and_report_cancellation_during_func_execution()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .RetryForeverAsync();

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

            policy.Awaiting(async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true).ConfigureAwait(false))
                .ShouldThrow<TaskCanceledException>().And.CancellationToken.Should().Be(cancellationToken);

            result.Should().Be(null);

            attemptsInvoked.Should().Be(1);
        }

    }
}