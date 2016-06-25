using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Polly.Retry;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs
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

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                  .ParamName.Should().Be("retryCount");
        }

        [Fact]
        public void Should_throw_when_onretry_action_without_context_is_null()
        {
            Action<DelegateResult<ResultPrimitive>, int> nullOnRetry = null;

            Action policy = () => Policy
                                      .HandleResult(ResultPrimitive.Fault)
                                      .RetryAsync(1, nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_throw_when_retry_count_is_less_than_zero_with_context()
        {
            Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, __, ___) => { };

            Action policy = () => Policy
                                      .HandleResult(ResultPrimitive.Fault)
                                      .RetryAsync(-1, onRetry);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                  .ParamName.Should().Be("retryCount");
        }

        [Fact]
        public void Should_throw_when_onretry_action_with_context_is_null()
        {
            Action<DelegateResult<ResultPrimitive>, int, Context> nullOnRetry = null;

            Action policy = () => Policy
                                      .HandleResult(ResultPrimitive.Fault)
                                      .RetryAsync(1, nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public async void Should_not_return_handled_result_when_handled_result_raised_same_number_of_times_as_retry_count()
        {
            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async void Should_not_return_handled_result_when_one_of_the_handled_results_raised_same_number_of_times_as_retry_count()
        {
            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.FaultAgain, ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async void Should_not_return_handled_result_when_handled_result_raised_less_number_of_times_than_retry_count()
        {
            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async void Should_not_return_handled_result_when_all_of_the_handled_results_raised_less_number_of_times_than_retry_count()
        {
            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.FaultAgain, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async void Should_return_handled_result_when_handled_result_raised_more_times_then_retry_count()
        {
            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.Fault); // It should give up retrying after 3 retries and return the last failure, so should return Fault, not Good.
        }

        [Fact]
        public async void Should_return_handled_result_when_one_of_the_handled_results_is_raised_more_times_then_retry_count()
        {
            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .RetryAsync(3);

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.FaultAgain, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.FaultAgain);
        }

        [Fact]
        public async void Should_return_result_when_result_is_not_the_specified_handled_result()
        {
            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync();

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.FaultAgain);
        }

        [Fact]
        public async void Should_return_result_when_result_is_not_one_of_the_specified_handled_results()
        {
            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .OrResult(ResultPrimitive.FaultAgain)
                .RetryAsync();

            ResultPrimitive result = await policy.RaiseResultSequenceAsync(ResultPrimitive.FaultYetAgain, ResultPrimitive.Good).ConfigureAwait(false);
            result.Should().Be(ResultPrimitive.FaultYetAgain);
        }

        [Fact]
        public async void Should_return_result_when_specified_result_predicate_is_not_satisfied()
        {
            Policy<ResultClass> policy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .RetryAsync();

            ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain), new ResultClass(ResultPrimitive.Good)).ConfigureAwait(false);
            result.ResultCode.Should().Be(ResultPrimitive.FaultAgain);
        }

        [Fact]
        public async void Should_return_result_when_none_of_the_specified_result_predicates_are_satisfied()
        {
            Policy<ResultClass> policy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .OrResult<ResultClass>(r => r.ResultCode == ResultPrimitive.FaultAgain)
                .RetryAsync();

            ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultYetAgain), new ResultClass(ResultPrimitive.Good)).ConfigureAwait(false);
            result.ResultCode.Should().Be(ResultPrimitive.FaultYetAgain);
        }

        [Fact]
        public async void Should_not_return_handled_result_when_specified_result_predicate_is_satisfied()
        {
            Policy<ResultClass> policy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .RetryAsync();

            ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault), new ResultClass(ResultPrimitive.Good)).ConfigureAwait(false);
            result.ResultCode.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async void Should_not_return_handled_result_when_one_of_the_specified_result_predicates_is_satisfied()
        {
            Policy<ResultClass> policy = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .OrResult<ResultClass>(r => r.ResultCode == ResultPrimitive.FaultAgain)
                .RetryAsync();

            ResultClass result = await policy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.FaultAgain), new ResultClass(ResultPrimitive.Good)).ConfigureAwait(false);
            result.ResultCode.Should().Be(ResultPrimitive.Good);
        }

        [Fact]
        public async void Should_call_onretry_on_each_retry_with_the_current_retry_count()
        {
            var expectedRetryCounts = new[] { 1, 2, 3 };
            var retryCounts = new List<int>();

            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(3, (_, retryCount) => retryCounts.Add(retryCount));

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Good);

            retryCounts.Should()
                       .ContainInOrder(expectedRetryCounts);
        }

        [Fact]
        public async void Should_call_onretry_on_each_retry_with_the_current_handled_result()
        {
            var expectedFaults = new[] { "Fault #1", "Fault #2", "Fault #3" };
            var retryFaults = new List<string>();

            Policy<ResultClass> policy = Policy
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
        public async void Should_not_call_onretry_when_no_retries_are_performed()
        {
            var retryCounts = new List<int>();

            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, retryCount) => retryCounts.Add(retryCount));

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Good).ConfigureAwait(false))
                .Should().Be(ResultPrimitive.Good);

            retryCounts.Should()
                .BeEmpty();
        }

        [Fact]
        public async void Should_call_onretry_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            RetryPolicy<ResultPrimitive> policy = Policy
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
        public async void Should_call_onretry_with_the_passed_context_when_execute_and_capture()
        {
            IDictionary<string, object> contextData = null;

            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, context) => contextData = context);

            PolicyResult<ResultPrimitive> result = await policy.RaiseResultSequenceOnExecuteAndCaptureAsync(
                new { key1 = "value1", key2 = "value2" }.AsDictionary(),
                ResultPrimitive.Fault, ResultPrimitive.Good
                ).ConfigureAwait(false);

            result.ShouldBeEquivalentTo(new
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
        public async void Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            Context capturedContext = null;

            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync((_, __, context) => capturedContext = context);

            await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false);

            capturedContext.Should()
                           .BeEmpty();
        }

        [Fact]
        public async void Should_create_new_context_for_each_call_to_execute()
        {
            string contextValue = null;

            RetryPolicy<ResultPrimitive> policy = Policy
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
        public async void Should_create_new_context_for_each_call_to_execute_and_capture()
        {
            string contextValue = null;

            RetryPolicy<ResultPrimitive> policy = Policy
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
        public async void Should_create_new_state_for_each_call_to_policy()
        {
            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(1);

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false)).Should().Be(ResultPrimitive.Good);

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false)).Should().Be(ResultPrimitive.Good);

        }

        [Fact]
        public async void Should_not_call_onretry_when_retry_count_is_zero_without_context()
        {
            bool retryInvoked = false;

            Action<DelegateResult<ResultPrimitive>, int> onRetry = (_, __) => { retryInvoked = true; };

            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(0, onRetry);

            (await policy.RaiseResultSequenceAsync(ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false)).Should().Be(ResultPrimitive.Fault);

            retryInvoked.Should().BeFalse();
        }

        [Fact]
        public async void Should_not_call_onretry_when_retry_count_is_zero_with_context()
        {
            bool retryInvoked = false;

            Action<DelegateResult<ResultPrimitive>, int, Context> onRetry = (_, __, ___) => { retryInvoked = true; };

            RetryPolicy<ResultPrimitive> policy = Policy
                .HandleResult(ResultPrimitive.Fault)
                .RetryAsync(0, onRetry);

            (await policy.RaiseResultSequenceAsync(
                 new { key = "value" }.AsDictionary(),
                ResultPrimitive.Fault, ResultPrimitive.Good).ConfigureAwait(false)).Should().Be(ResultPrimitive.Fault);

            retryInvoked.Should().BeFalse();
        }

    }
}
