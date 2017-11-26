using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Polly.Specs.Helpers;
using Xunit;

namespace Polly.Specs.Retry
{
    public class RetrySpecs
    {
        [Fact]
        public void Should_throw_when_retry_count_is_less_than_zero_without_context()
        {
            Action<Exception, int> onRetry = (_, __) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .Retry(-1, onRetry);
        
            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                  .ParamName.Should().Be("retryCount");
        }

        [Fact]
        public void Should_throw_when_onretry_action_without_context_is_null()
        {
            Action<Exception, int> nullOnRetry = null;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .Retry(1, nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_throw_when_retry_count_is_less_than_zero_with_context()
        {
            Action<Exception, int, Context> onRetry = (_, __, ___) => { };

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .Retry(-1, onRetry);

            policy.ShouldThrow<ArgumentOutOfRangeException>().And
                  .ParamName.Should().Be("retryCount");
        }

        [Fact]
        public void Should_throw_when_onretry_action_with_context_is_null()
        {
            Action<Exception, int, Context> nullOnRetry = null;

            Action policy = () => Policy
                                      .Handle<DivideByZeroException>()
                                      .Retry(1, nullOnRetry);

            policy.ShouldThrow<ArgumentNullException>().And
                  .ParamName.Should().Be("onRetry");
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_thrown_same_number_of_times_as_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_same_number_of_times_as_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<ArgumentException>(3))
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_thrown_less_number_of_times_than_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();
        }
        
        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exceptions_thrown_less_number_of_times_than_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldNotThrow();
        }
        
        [Fact]
        public void Should_throw_when_specified_exception_thrown_more_times_then_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>(3 + 1))
                  .ShouldThrow<DivideByZeroException>();
        }
        
        [Fact]
        public void Should_throw_when_one_of_the_specified_exceptions_are_thrown_more_times_then_retry_count()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .Retry(3);

            policy.Invoking(x => x.RaiseException<ArgumentException>(3 + 1))
                  .ShouldThrow<ArgumentException>();
        }
        
        [Fact]
        public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry();

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentException>()
                .Retry();

            policy.Invoking(x => x.RaiseException<NullReferenceException>())
                  .ShouldThrow<NullReferenceException>();
        }

        [Fact]
        public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Retry();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => false)
                .Or<ArgumentException>(e => false)
                .Retry();

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Retry();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
        {
            var policy = Policy
                .Handle<DivideByZeroException>(e => true)
                .Or<ArgumentException>(e => true)
                .Retry();

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                  .ShouldNotThrow();
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
        {
            var expectedRetryCounts = new[] { 1, 2, 3 };
            var retryCounts = new List<int>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3, (_, retryCount) => retryCounts.Add(retryCount));

            policy.RaiseException<DivideByZeroException>(3);

            retryCounts.Should()
                       .ContainInOrder(expectedRetryCounts);
        }

        [Fact]
        public void Should_call_onretry_on_each_retry_with_the_current_exception()
        {
            var expectedExceptions = new object[] { "Exception #1", "Exception #2", "Exception #3" };
            var retryExceptions = new List<Exception>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3, (exception, _) => retryExceptions.Add(exception));

            policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

            retryExceptions
                .Select(x => x.HelpLink)
                .Should()
                .ContainInOrder(expectedExceptions);
        }

        [Fact]
        public void Should_call_onretry_with_a_handled_innerexception()
        {
            Exception passedToOnRetry = null;

            var policy = Policy
                .HandleInner<DivideByZeroException>()
                .Retry(3, (exception, _) => passedToOnRetry = exception);

            Exception toRaiseAsInner = new DivideByZeroException();
            Exception withInner = new AggregateException(toRaiseAsInner);

            policy.RaiseException(withInner);

            passedToOnRetry.Should().BeSameAs(toRaiseAsInner);
        }

        [Fact]
        public void Should_not_call_onretry_when_no_retries_are_performed()
        {
            var retryCounts = new List<int>();

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, retryCount) => retryCounts.Add(retryCount));

            policy.Invoking(x => x.RaiseException<ArgumentException>())
                .ShouldThrow<ArgumentException>();

            retryCounts.Should()
                .BeEmpty();
        }

        [Fact]
        public void Should_call_onretry_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, context) => contextData = context);

            policy.RaiseException<DivideByZeroException>(
                new { key1 = "value1", key2 = "value2" }.AsDictionary()
                );

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onretry_with_the_passed_context_when_execute_and_capture()
        {
            IDictionary<string, object> contextData = null;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, context) => contextData = context);

            policy.Invoking(p => p.ExecuteAndCapture(() => { throw new DivideByZeroException();}, 
                new { key1 = "value1", key2 = "value2" }.AsDictionary()))
                .ShouldNotThrow();

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            Context capturedContext = null;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, context) => capturedContext = context);

            policy.RaiseException<DivideByZeroException>();

            capturedContext.Should()
                           .BeEmpty();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_execute()
        {
            string contextValue = null;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, context) => contextValue = context["key"].ToString());

            policy.RaiseException<DivideByZeroException>(
                new { key = "original_value" }.AsDictionary()
            );

            contextValue.Should().Be("original_value");

            policy.RaiseException<DivideByZeroException>(
                new { key = "new_value" }.AsDictionary()
            );

            contextValue.Should().Be("new_value");
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_execute_and_capture()
        {
            string contextValue = null;

            Policy policy = Policy
                .Handle<DivideByZeroException>()
                .Retry((_, __, context) => contextValue = context["key"].ToString());

            policy.Invoking(p => p.ExecuteAndCapture(() => { throw new DivideByZeroException(); }, 
                new { key = "original_value" }.AsDictionary()))
                .ShouldNotThrow();

            contextValue.Should().Be("original_value");

            policy.Invoking(p => p.ExecuteAndCapture(() => { throw new DivideByZeroException(); },
                new { key = "new_value" }.AsDictionary()))
                .ShouldNotThrow();

            contextValue.Should().Be("new_value");
        }

        [Fact]
        public void Should_create_new_state_for_each_call_to_policy()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldNotThrow();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldNotThrow();
        }

        [Fact]
        public void Should_not_call_onretry_when_retry_count_is_zero_without_context()
        {
            bool retryInvoked = false;

            Action<Exception, int> onRetry = (_, __) => { retryInvoked = true; };

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(0, onRetry);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            retryInvoked.Should().BeFalse();
        }

        [Fact]
        public void Should_not_call_onretry_when_retry_count_is_zero_with_context()
        {
            bool retryInvoked = false;

            Action<Exception, int, Context> onRetry = (_, __, ___) => { retryInvoked = true; };

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(0, onRetry);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            retryInvoked.Should().BeFalse();
        }


        #region Sync cancellation tests

        [Fact]
        public void Should_execute_action_when_non_faulting_and_cancellationtoken_not_cancelled()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_execute_all_tries_when_faulting_and_cancellationtoken_not_cancelled()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = null,
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<DivideByZeroException>();

            attemptsInvoked.Should().Be(1 + 3);
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationtoken_cancelled_before_execute()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
            };

            cancellationTokenSource.Cancel();

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_initial_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_observes_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 2,
                ActionObservesCancellation = true
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(2);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_retried_action_execution_and_cancel_further_retries_when_user_delegate_does_not_observe_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 2,
                ActionObservesCancellation = false
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(2);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_last_retry_execution_when_user_delegate_does_observe_cancellationtoken()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 1 + 3,
                ActionObservesCancellation = true
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1 + 3);
        }

        [Fact]
        public void Should_report_faulting_from_faulting_last_retry_execution_when_user_delegate_does_not_observe_cancellation_raised_during_last_retry()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = 1 + 3,
                ActionObservesCancellation = false
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<DivideByZeroException>();

            attemptsInvoked.Should().Be(1 + 3);
        }

        [Fact]
        public void Should_report_cancellation_after_faulting_action_execution_and_cancel_further_retries_if_onRetry_invokes_cancellation()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3, (_, __) =>
                {
                    cancellationTokenSource.Cancel();
                });

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1 + 3,
                AttemptDuringWhichToCancel = null, // Cancellation during onRetry instead - see above.
                ActionObservesCancellation = false
            };

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_execute_func_returning_value_when_cancellationtoken_not_cancelled()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            bool? result = null;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            policy.Invoking(x => result = x.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
                .ShouldNotThrow();

            result.Should().BeTrue();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_honour_and_report_cancellation_during_func_execution()
        {
            var policy = Policy
                .Handle<DivideByZeroException>()
                .Retry(3);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            bool? result = null;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            policy.Invoking(x => result = x.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
                .ShouldThrow<OperationCanceledException>().And.CancellationToken.Should().Be(cancellationToken);

            result.Should().Be(null);

            attemptsInvoked.Should().Be(1);
        }

        #endregion


    }
}