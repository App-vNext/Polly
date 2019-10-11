﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

using Scenario = Polly.Specs.Helpers.PolicyTResultExtensionsAsync.ResultAndOrCancellationScenario;

namespace Polly.Specs.Retry
{
    public class RetryTResultSpecsAsync
    {
        [Fact]
        public void Should_throw_when_retry_count_is_less_than_zero_without_context()
        {
            Action<DelegateResult<ResultPrimitive>, int> onRetry = (_, __) => { };

            Action policy = () => Policy
                                      .HandleResult(ResultPrimitive.Fault)
                                      .RetryAsync(-1, onRetry);

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                  .ParamName.Should().Be("retryCount");
        }

        [Fact]
        public void Should_throw_when_onretry_action_without_context_is_null()
        {
            Action<DelegateResult<ResultPrimitive>, int> nullOnRetry = null;

            Action policy = () => Policy
                                      .HandleResult(ResultPrimitive.Fault)
                                      .RetryAsync(1, nullOnRetry);

            policy.Should().Throw<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_throw_when_retry_count_is_less_than_zero_with_context()
        {
            Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, __, ___) => { };

            Action policy = () => Policy
                                      .HandleResult(ResultPrimitive.Fault)
                                      .RetryAsync(-1, onRetry);

            policy.Should().Throw<ArgumentOutOfRangeException>().And
                  .ParamName.Should().Be("retryCount");
        }

        [Fact]
        public void Should_throw_when_onretry_action_with_context_is_null()
        {
            Action<DelegateResult<ResultPrimitive>, int, Context> nullOnRetry = null;

            Action policy = () => Policy
                                      .HandleResult(ResultPrimitive.Fault)
                                      .RetryAsync(1, nullOnRetry);

            policy.Should().Throw<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public async Task Should_not_return_handled_result_when_handled_result_raised_same_number_of_times_as_retry_count()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async Task Should_not_return_handled_result_when_one_of_the_handled_results_raised_same_number_of_times_as_retry_count()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.FaultAgain, ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async Task Should_not_return_handled_result_when_handled_result_raised_less_number_of_times_than_retry_count()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async Task Should_not_return_handled_result_when_all_of_the_handled_results_raised_less_number_of_times_than_retry_count()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.FaultAgain, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async Task Should_return_handled_result_when_handled_result_raised_more_times_then_retry_count()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Fault); // It should give up retrying after 3 retries and return the last failure, so should return Fault, not Good.
        }

        [Fact]
        public async Task Should_return_handled_result_when_one_of_the_handled_results_is_raised_more_times_then_retry_count()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.FaultAgain);
        }

        [Fact]
        public async Task Should_return_result_when_result_is_not_the_specified_handled_result()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync();

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.FaultAgain);
        }

        [Fact]
        public async Task Should_return_result_when_result_is_not_one_of_the_specified_handled_results()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .RetryAsync();

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.FaultYetAgain, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.FaultYetAgain);
        }

        [Fact]
        public async Task Should_return_result_when_specified_result_predicate_is_not_satisfied()
        {
            var policy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .RetryAsync();

            ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain), new ResultClass(ResultPrimitive.Good)).ConfigureAwait(false);
            result.ResultCode.Should().Be(ResultPrimitive.FaultAgain);
        }

        [Fact]
        public async Task Should_return_result_when_none_of_the_specified_result_predicates_are_satisfied()
        {
            var policy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .OrResult(r => r.ResultCode == ResultPrimitive.FaultAgain)
                .RetryAsync();

            ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultYetAgain), new ResultClass(ResultPrimitive.Good)).ConfigureAwait(false);
            result.ResultCode.Should().Be(ResultPrimitive.FaultYetAgain);
        }

        [Fact]
        public async Task Should_not_return_handled_result_when_specified_result_predicate_is_satisfied()
        {
            var policy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .RetryAsync();

            ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault), new ResultClass(ResultPrimitive.Good)).ConfigureAwait(false);
            result.ResultCode.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async Task Should_not_return_handled_result_when_one_of_the_specified_result_predicates_is_satisfied()
        {
            var policy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .OrResult(r => r.ResultCode == ResultPrimitive.FaultAgain)
                .RetryAsync();

            ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain), new ResultClass(ResultPrimitive.Good)).ConfigureAwait(false);
            result.ResultCode.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async Task Should_call_onretry_on_each_retry_with_the_current_retry_count()
        {
            var expectedRetryCounts = new[] { 1, 2, 3 };
            var retryCounts = new List<int>();

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3, (_, retryCount) => retryCounts.Add(retryCount));

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Good);

            retryCounts.Should()
                       .ContainInOrder(expectedRetryCounts);
        }

        [Fact]
        public async Task Should_call_onretry_on_each_retry_with_the_current_handled_result()
        {
            var expectedFaults = new[] { "Fault #1", "Fault #2", "Fault #3" };
            var retryFaults = new List<string>();

            var policy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .RetryAsync(3, (outcome, _) => retryFaults.Add(outcome.Result.SomeString));

            IList<ResultClass> resultsToRaise = expectedFaults.Select(s => new ResultClass(ResultPrimitive.Fault, s)).ToList();
            resultsToRaise.Add(new ResultClass(ResultPrimitive.Fault));

            (await policy.RaiseResultSequenceAsync(resultsToRaise).ConfigureAwait(false))
                .ResultCode.Should().Be(ResultPrimitive.Fault);

            retryFaults
                .Should()
                .ContainInOrder(expectedFaults);
        }

        [Fact]
        public async Task Should_not_call_onretry_when_no_retries_are_performed()
        {
            var retryCounts = new List<int>();

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, retryCount) => retryCounts.Add(retryCount));

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Good).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Good);

            retryCounts.Should()
                .BeEmpty();
        }

        [Fact]
        public async Task Should_call_onretry_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, context) => contextData = context);

            (await policy.RaiseResultSequenceAsync(
                new { key1 = "value1", key2 = "value2" }.AsDictionary(),
                ResultPrimitive.Fault, ResultPrimitive.Good
                ).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Good);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public async Task Should_call_onretry_with_the_passed_context_when_execute_and_capture()
        {
            IDictionary<string, object> contextData = null;

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, context) => contextData = context);

            PolicyResult<ResultPrimitive> result = await policy.RaiseResultSequenceOnExecuteAndCaptureAsync(
                new { key1 = "value1", key2 = "value2" }.AsDictionary(),
                ResultPrimitive.Fault, ResultPrimitive.Good
                ).ConfigureAwait(false);

            result.Should().BeEquivalentTo(new
            {
                Outcome = OutcomeType.Successful,
                FinalException = (Exception)null,
                ExceptionType = (ExceptionType?)null,
                FaultType = (FaultType?)null,
                FinalHandledResult = default(ResultPrimitive),
                Result = ResultPrimitive.Good
            });

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            Context capturedContext = null;

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, context) => capturedContext = context);

            await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);

            capturedContext.Should()
                           .BeEmpty();
        }

        [Fact]
        public async Task Should_create_new_context_for_each_call_to_execute()
        {
            string contextValue = null;

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, context) => contextValue = context["key"].ToString());

            await policy.RaiseResultSequenceAsync(
                new { key = "original_value" }.AsDictionary(),
                ResultPrimitive.Fault, ResultPrimitive.Good
            ).ConfigureAwait(false);

            contextValue.Should().Be("original_value");

            await policy.RaiseResultSequenceAsync(
                new { key = "new_value" }.AsDictionary(),
                ResultPrimitive.Fault, ResultPrimitive.Good
            ).ConfigureAwait(false);

            contextValue.Should().Be("new_value");
        }

        [Fact]
        public async Task Should_create_new_context_for_each_call_to_execute_and_capture()
        {
            string contextValue = null;

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, context) => contextValue = context["key"].ToString());

            await policy.RaiseResultSequenceOnExecuteAndCaptureAsync(
                new { key = "original_value" }.AsDictionary(),
                ResultPrimitive.Fault, ResultPrimitive.Good
            ).ConfigureAwait(false);

            contextValue.Should().Be("original_value");

            await policy.RaiseResultSequenceOnExecuteAndCaptureAsync(
                new { key = "new_value" }.AsDictionary(),
                ResultPrimitive.Fault, ResultPrimitive.Good
            ).ConfigureAwait(false);

            contextValue.Should().Be("new_value");
        }

        [Fact]
        public async Task Should_create_new_state_for_each_call_to_policy()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(1);

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false)).Should().Be(ResultPrimitive.Good);

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false)).Should().Be(ResultPrimitive.Good);

        }

        [Fact]
        public async Task Should_not_call_onretry_when_retry_count_is_zero_without_context()
        {
            bool retryInvoked = false;

            Action<DelegateResult<ResultPrimitive>, int> onRetry = (_, __) => { retryInvoked = true; };

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(0, onRetry);

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false)).Should().Be(ResultPrimitive.Fault);

            retryInvoked.Should().BeFalse();
        }

        [Fact]
        public async Task Should_not_call_onretry_when_retry_count_is_zero_with_context()
        {
            bool retryInvoked = false;

            Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, __, ___) => { retryInvoked = true; };

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(0, onRetry);

            (await policy.RaiseResultSequenceAsync(
                 new { key = "value" }.AsDictionary(),
                ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false)).Should().Be(ResultPrimitive.Fault);

            retryInvoked.Should().BeFalse();
        }

        #region Async and cancellation tests

        [Fact]
        public async Task Should_wait_asynchronously_for_async_onretry_delegate()
        {
            // This test relates to https://github.com/App-vNext/Polly/issues/107.  
            // An async (...) => { ... } anonymous delegate with no return type may compile to either an async void or an async Task method; which assign to an Action<...> or Func<..., Task> respectively.  However, if it compiles to async void (assigning tp Action<...>), then the delegate, when run, will return at the first await, and execution continues without waiting for the Action to complete, as described by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/08/10265476.aspx
            // If Polly were to declare only an Action<...> delegate for onRetry - but users declared async () => { } onRetry delegates - the compiler would happily assign them to the Action<...>, but the next 'try' would/could occur before onRetry execution had completed.
            // This test ensures the relevant retry policy does have a Func<..., Task> form for onRetry, and that it is awaited before the next try commences.

            TimeSpan shimTimeSpan = TimeSpan.FromSeconds(0.2); // Consider increasing shimTimeSpan if test fails transiently in different environments.

            int executeDelegateInvocations = 0;
            int executeDelegateInvocationsWhenOnRetryExits = 0;

            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(async (ex, retry) =>
                {
                    await Task.Delay(shimTimeSpan).ConfigureAwait(false);
                    executeDelegateInvocationsWhenOnRetryExits = executeDelegateInvocations;
                });

            (await policy.ExecuteAsync(async () =>
            {
                executeDelegateInvocations++;
                await TaskHelper.EmptyTask.ConfigureAwait(false);
                return ResultPrimitive.Fault;
            }).ConfigureAwait(false)).Should().Be(ResultPrimitive.Fault);

            while (executeDelegateInvocationsWhenOnRetryExits == 0) { } // Wait for the onRetry delegate to complete.

            executeDelegateInvocationsWhenOnRetryExits.Should().Be(1); // If the async onRetry delegate is genuinely awaited, only one execution of the .Execute delegate should have occurred by the time onRetry completes.  If the async onRetry delegate were instead assigned to an Action<...>, then onRetry will return, and the second action execution will commence, before await Task.Delay() completes, leaving executeDelegateInvocationsWhenOnRetryExits == 2.  
            executeDelegateInvocations.Should().Be(2);
        }

        [Fact]
        public async Task Should_execute_all_tries_when_faulting_and_cancellationToken_not_cancelled()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = null,
            };

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Fault,
                    ResultPrimitive.Good).ConfigureAwait(false))
                    .Should().Be(ResultPrimitive.Good);

            attemptsInvoked.Should().Be(1 + 3);
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
            };

            cancellationTokenSource.Cancel();

            policy.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
        {
            var policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Good,
                   ResultPrimitive.Good,
                   ResultPrimitive.Good,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
        {
            var policy = Policy
               .HandleResult(ResultPrimitive.Fault)
               .RetryAsync(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
        {
            var policy = Policy
              .HandleResult(ResultPrimitive.Fault)
              .RetryAsync(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            policy.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationToken()
        {
            var policy = Policy
              .HandleResult(ResultPrimitive.Fault)
              .RetryAsync(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 2,
                ActionObservesCancellation = true
            };

            policy.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(2);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationToken()
        {
            var policy = Policy
              .HandleResult(ResultPrimitive.Fault)
              .RetryAsync(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 2,
                ActionObservesCancellation = false
            };

            policy.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(2);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_last_retry_execution_when_user_delegate_does_observe_cancellationToken()
        {
            var policy = Policy
                       .HandleResult(ResultPrimitive.Fault)
                       .RetryAsync(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 1 + 3,
                ActionObservesCancellation = true
            };

            policy.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1 + 3);
        }

        [Fact]
        public async Task Should_report_faulting_from_faulting_last_retry_execution_when_user_delegate_does_not_observe_cancellation_raised_during_last_retry()
        {
            var policy = Policy
                       .HandleResult(ResultPrimitive.Fault)
                       .RetryAsync(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = 1 + 3,
                ActionObservesCancellation = false
            };

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good).ConfigureAwait(false))
               .Should().Be(ResultPrimitive.Fault);

            attemptsInvoked.Should().Be(1 + 3);
        }

        [Fact]
        public void Should_report_cancellation_after_faulting_action_execution_and_cancel_further_retries_if_onRetry_invokes_cancellation()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
           .HandleResult(ResultPrimitive.Fault)
           .RetryAsync(3, (_, __) =>
           {
               cancellationTokenSource.Cancel();
           });

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                AttemptDuringWhichToCancel = null, // Cancellation during onRetry instead - see above.
                ActionObservesCancellation = false
            };

            policy.Awaiting(async x => await x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Fault,
                   ResultPrimitive.Good))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }
        
        #endregion
    }
}
