using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.CircuitBreaker
{
    [Collection(Constants.SystemClockDependentTestCollection)]
    public class CircuitBreakerSpecs : IDisposable
    {
        #region Configuration tests

        [Theory, InlineData(true), InlineData(false)]
        public void Should_be_able_to_handle_a_duration_of_timespan_maxvalue(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(1, TimeSpan.MaxValue, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_throw_if_exceptions_allowed_before_breaking_is_less_than_one(bool timespanIsDynamic)
        {
           Action action = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .CircuitBreaker_WithConditionalTimespan(0, TimeSpan.FromSeconds(10), timespanIsDynamic);

            action.Should().Throw<ArgumentOutOfRangeException>()
                  .And.ParamName.Should()
                  .Be("exceptionsAllowedBeforeBreaking");
        }

        #region duration non-positive
        //Note that these are some of the few tests that vary based on static vs dynamic delay duration.
        [Fact]
        public void Should_throw_if_duration_of_break_is_less_than_zero_and_duration_is_static()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(1, -TimeSpan.FromSeconds(1));

            action.Should().Throw<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("durationOfBreak");
        }

        [Fact]
        public void Should_be_able_to_handle_dynamically_defined_durations_less_than_zero()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker(1, (_) => -TimeSpan.FromSeconds(1));

            action.Should().NotThrow();
        }


        [Theory, InlineData(true), InlineData(false)]
        public void Should_be_able_to_handle_a_duration_of_break_of_zero(bool timespanIsDynamic)
        {
            Action action = () => Policy
                                     .Handle<DivideByZeroException>()
                                     .CircuitBreaker_WithConditionalTimespan(1, TimeSpan.Zero, timespanIsDynamic);
            action.Should().NotThrow();
        }
        #endregion

        [Theory, InlineData(true), InlineData(false)]
        public void Should_initialise_to_closed_state(bool timespanIsDynamic)
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Circuit-breaker threshold-to-break tests

        [Theory, InlineData(true), InlineData(false)]
        public void Should_not_open_circuit_if_specified_number_of_specified_exception_are_not_raised_consecutively(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.Execute(() =>{})).Should().NotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_after_specified_number_of_specified_exception_have_been_raised(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            bool delegateExecutedWhenBroken = false;
            breaker.Invoking(x => x.Execute(() => delegateExecutedWhenBroken = true))
                  .Should().Throw<BrokenCircuitException>()
                  .WithMessage("The circuit is now open and is not allowing calls.")
                  .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            delegateExecutedWhenBroken.Should().BeFalse();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_after_specified_number_of_one_of_the_specified_exceptions_have_been_raised(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .Or<ArgumentOutOfRangeException>()
                            .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<ArgumentOutOfRangeException>())
                  .Should().Throw<ArgumentOutOfRangeException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            bool delegateExecutedWhenBroken = false;
            breaker.Invoking(x => x.Execute(() => delegateExecutedWhenBroken = true))
                  .Should().Throw<BrokenCircuitException>()
                  .WithMessage("The circuit is now open and is not allowing calls.")
                  .WithInnerException<ArgumentOutOfRangeException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            delegateExecutedWhenBroken.Should().BeFalse();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_not_open_circuit_if_exception_raised_is_not_the_specified_exception(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .Should().Throw<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .Should().Throw<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .Should().Throw<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_not_open_circuit_if_exception_raised_is_not_one_of_the_specified_exceptions(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .Or<ArgumentOutOfRangeException>()
                            .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .Should().Throw<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .Should().Throw<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .Should().Throw<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Circuit-breaker open->half-open->open/closed tests

        [Theory, InlineData(true), InlineData(false)]
        public void Should_halfopen_circuit_after_the_specified_duration_has_passed(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // first call after duration raises an exception, so circuit should break again
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_raise_an_exception(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => {});
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // circuit has been reset so should once again allow 2 exceptions to be raised before breaking
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_only_allow_single_execution_on_first_entering_halfopen_state__test_execution_permit_directly(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(1, durationOfBreak, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();

            // exception raised, circuit is now open.  
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // break duration passes, circuit now half open
            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);


            // OnActionPreExecute() should permit first execution.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).Should().NotThrow();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // OnActionPreExecute() should reject a second execution.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_allow_single_execution_per_break_duration_in_halfopen_state__test_execution_permit_directly(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(1, durationOfBreak, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();

            // exception raised, circuit is now open.  
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // break duration passes, circuit now half open
            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);


            // OnActionPreExecute() should permit first execution.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).Should().NotThrow();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // OnActionPreExecute() should reject a second execution in the same time window.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // Allow another time window to pass (breaker should still be HalfOpen).
            SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // OnActionPreExecute() should now permit another trial execution.
            breaker._breakerController.Invoking(c => c.OnActionPreExecute()).Should().NotThrow();
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_only_allow_single_execution_on_first_entering_halfopen_state__integration_test(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(1, durationOfBreak, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();

            // exception raised, circuit is now open.  
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // break duration passes, circuit now half open
            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // Start one execution during the HalfOpen state, and request a second execution before the first has completed (ie still during the HalfOpen state).
            // The second execution should be rejected due to the halfopen state.

            TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
            using (ManualResetEvent permitSecondExecutionAttempt = new ManualResetEvent(false))
            using (ManualResetEvent permitFirstExecutionEnd = new ManualResetEvent(false))
            { 
                bool? firstDelegateExecutedInHalfOpenState = null;
                bool? secondDelegateExecutedInHalfOpenState = null;
                bool? secondDelegateRejectedInHalfOpenState = null;

                bool firstExecutionActive = false;
                // First execution in HalfOpen state: we should be able to verify state is HalfOpen as it executes.
                Task firstExecution = Task.Factory.StartNew(() =>
                {
                    breaker.Invoking(x => x.Execute(() =>
                    {
                        firstDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                        // Signal the second execution can start, overlapping with this (the first) execution.
                        firstExecutionActive = true;
                        permitSecondExecutionAttempt.Set();

                        // Hold first execution open until second indicates it is no longer needed, or time out.
                        permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                        firstExecutionActive = false;

                    })).Should().NotThrow();
                }, TaskCreationOptions.LongRunning);

                // Attempt a second execution, signalled by the first execution to ensure they overlap: we should be able to verify it doesn't execute, and is rejected by a breaker in a HalfOpen state.
                permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks);

                Task secondExecution = Task.Factory.StartNew(() =>
                {
                    // Validation of correct sequencing and overlapping of tasks in test (guard against erroneous test refactorings/operation).
                    firstExecutionActive.Should().BeTrue();
                    breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

                    try
                    {
                        breaker.Execute(() =>
                        {
                            secondDelegateRejectedInHalfOpenState = false;
                            secondDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.
                        });
                    }
                    catch (BrokenCircuitException)
                    {
                        secondDelegateExecutedInHalfOpenState = false;
                        secondDelegateRejectedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested here.
                    }

                    // Release first execution soon as second overlapping execution is done gathering data.
                    permitFirstExecutionEnd.Set();
                }, TaskCreationOptions.LongRunning);

                // Graceful cleanup: allow executions time to end naturally; signal them to end if not; timeout any deadlocks; expose any execution faults. This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
                permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                permitFirstExecutionEnd.Set();
                Task.WaitAll(new[] { firstExecution, secondExecution }, testTimeoutToExposeDeadlocks).Should().BeTrue();
                if (firstExecution.IsFaulted) throw firstExecution.Exception;
                if (secondExecution.IsFaulted) throw secondExecution.Exception;
                firstExecution.Status.Should().Be(TaskStatus.RanToCompletion);
                secondExecution.Status.Should().Be(TaskStatus.RanToCompletion);

                // Assert: 
                // - First execution should have been permitted and executed under a HalfOpen state
                // - Second overlapping execution in halfopen state should not have been permitted.
                // - Second execution attempt should have been rejected with HalfOpen state as cause.
                firstDelegateExecutedInHalfOpenState.Should().BeTrue();
                secondDelegateExecutedInHalfOpenState.Should().BeFalse();
                secondDelegateRejectedInHalfOpenState.Should().BeTrue();

            }
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_allow_single_execution_per_break_duration_in_halfopen_state__integration_test(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(1, durationOfBreak, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();

            // exception raised, circuit is now open.  
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // break duration passes, circuit now half open
            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // Start one execution during the HalfOpen state.
            // Request a second execution while the first is still in flight (not completed), while still during the HalfOpen state, but after one breakDuration later.
            // The second execution should be accepted in the halfopen state due to being requested after one breakDuration later.

            TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
            using (ManualResetEvent permitSecondExecutionAttempt = new ManualResetEvent(false))
            using (ManualResetEvent permitFirstExecutionEnd = new ManualResetEvent(false))
            {
                bool? firstDelegateExecutedInHalfOpenState = null;
                bool? secondDelegateExecutedInHalfOpenState = null;
                bool? secondDelegateRejectedInHalfOpenState = null;

                bool firstExecutionActive = false;
                // First execution in HalfOpen state: we should be able to verify state is HalfOpen as it executes.
                Task firstExecution = Task.Factory.StartNew(() =>
                {
                    breaker.Invoking(x => x.Execute(() =>
                    {
                        firstDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                        // Signal the second execution can start, overlapping with this (the first) execution.
                        firstExecutionActive = true;
                        permitSecondExecutionAttempt.Set();

                        // Hold first execution open until second indicates it is no longer needed, or time out.
                        permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                        firstExecutionActive = false;

                    })).Should().NotThrow();
                }, TaskCreationOptions.LongRunning);

                // Attempt a second execution, signalled by the first execution to ensure they overlap; start it one breakDuration later.  We should be able to verify it does execute, though the breaker is still in a HalfOpen state.
                permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks);

                Task secondExecution = Task.Factory.StartNew(() =>
                {
                    // Validation of correct sequencing and overlapping of tasks in test (guard against erroneous test refactorings/operation).
                    firstExecutionActive.Should().BeTrue();
                    breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

                    try
                    {
                        SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);

                        breaker.Execute(() =>
                        {
                            secondDelegateRejectedInHalfOpenState = false;
                            secondDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.
                        });
                    }
                    catch (BrokenCircuitException)
                    {
                        secondDelegateExecutedInHalfOpenState = false;
                        secondDelegateRejectedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested here.
                    }

                    // Release first execution soon as second overlapping execution is done gathering data.
                    permitFirstExecutionEnd.Set();
                }, TaskCreationOptions.LongRunning);

                // Graceful cleanup: allow executions time to end naturally; signal them to end if not; timeout any deadlocks; expose any execution faults. This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
                permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                permitFirstExecutionEnd.Set();
                Task.WaitAll(new[] {firstExecution, secondExecution}, testTimeoutToExposeDeadlocks).Should().BeTrue();
                if (firstExecution.IsFaulted) throw firstExecution.Exception;
                if (secondExecution.IsFaulted) throw secondExecution.Exception;
                firstExecution.Status.Should().Be(TaskStatus.RanToCompletion);
                secondExecution.Status.Should().Be(TaskStatus.RanToCompletion);

                // Assert: 
                // - First execution should have been permitted and executed under a HalfOpen state
                // - Second overlapping execution in halfopen state should have been permitted, one breakDuration later.
                firstDelegateExecutedInHalfOpenState.Should().BeTrue();
                secondDelegateExecutedInHalfOpenState.Should().BeTrue();
                secondDelegateRejectedInHalfOpenState.Should().BeFalse();
            }
        }

        #endregion

        #region Isolate and reset tests

        [Theory, InlineData(true), InlineData(false)]
        public void Should_open_circuit_and_block_calls_if_manual_override_open(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // manually break circuit
            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            // circuit manually broken: execution should be blocked; even non-exception-throwing executions should not reset circuit
            bool delegateExecutedWhenBroken = false;
            breaker.Invoking(x => x.Execute(() => delegateExecutedWhenBroken = true))
                .Should().Throw<IsolatedCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.LastException.Should().BeOfType<IsolatedCircuitException>();
            delegateExecutedWhenBroken.Should().BeFalse();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_hold_circuit_open_despite_elapsed_time_if_manual_override_open(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            bool delegateExecutedWhenBroken = false;
            breaker.Invoking(x => x.Execute(() => { delegateExecutedWhenBroken = true; return ResultPrimitive.Good; }))
                .Should().Throw<IsolatedCircuitException>();
            delegateExecutedWhenBroken.Should().BeFalse();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_close_circuit_again_on_reset_after_manual_override(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Invoking(x => x.Execute(() => { }))
                .Should().Throw<IsolatedCircuitException>();

            breaker.Reset();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Invoking(x => x.Execute(() => { })).Should().NotThrow();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing(bool timespanIsDynamic)
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // reset circuit, with no time having passed
            breaker.Reset();
            SystemClock.UtcNow().Should().Be(time);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Invoking(x => x.Execute(() => { })).Should().NotThrow();
        }

        #endregion

        #region State-change delegate tests

        [Theory, InlineData(true), InlineData(false)]
        public void Should_not_call_onreset_on_initialise(bool timespanIsDynamic)
        {
            Action<Exception, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, timespanIsDynamic);

            onResetCalled.Should().BeFalse();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onbreak_when_breaking_circuit_automatically(bool timespanIsDynamic)
        {
            bool onBreakCalled = false;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().BeFalse();

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().BeTrue();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onbreak_when_breaking_circuit_manually(bool timespanIsDynamic)
        {
            bool onBreakCalled = false;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, timespanIsDynamic);
            onBreakCalled.Should().BeFalse();

            breaker.Isolate();

            onBreakCalled.Should().BeTrue();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_placed_through_open_circuit(bool timespanIsDynamic)
        {
            int onBreakCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            // call through circuit when already broken - should not retrigger onBreak 
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_call_failure_which_arrives_on_open_state_though_started_on_closed_state(bool timespanIsDynamic)
        {
            int onBreakCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(1, TimeSpan.FromMinutes(1), onBreak, onReset, timespanIsDynamic);

            // Start an execution when the breaker is in the closed state, but hold it from returning (its failure) until the breaker has opened.  This call, a failure hitting an already open breaker, should indicate its fail, but should not cause onBreak() to be called a second time.
            TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
            using (ManualResetEvent permitLongRunningExecutionToReturnItsFailure = new ManualResetEvent(false))
            using (ManualResetEvent permitMainThreadToOpenCircuit = new ManualResetEvent(false))
            { 
                Task longRunningExecution = Task.Factory.StartNew(() =>
                {
                    breaker.CircuitState.Should().Be(CircuitState.Closed);

                    breaker.Invoking(x => x.Execute(() =>
                    {
                        permitMainThreadToOpenCircuit.Set();

                    // Hold this execution until rest of the test indicates it can proceed (or timeout, to expose deadlocks).
                    permitLongRunningExecutionToReturnItsFailure.WaitOne(testTimeoutToExposeDeadlocks);

                    // Throw a further failure when rest of test has already broken the circuit.
                    breaker.CircuitState.Should().Be(CircuitState.Open);
                        throw new DivideByZeroException();

                    })).Should().Throw<DivideByZeroException>(); // However, since execution started when circuit was closed, BrokenCircuitException will not have been thrown on entry; the original exception will still be thrown.
                }, TaskCreationOptions.LongRunning);

                permitMainThreadToOpenCircuit.WaitOne(testTimeoutToExposeDeadlocks).Should().BeTrue();

                // Break circuit in the normal manner: onBreak() should be called once.
                breaker.CircuitState.Should().Be(CircuitState.Closed);
                onBreakCalled.Should().Be(0);
                breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                      .Should().Throw<DivideByZeroException>();
                breaker.CircuitState.Should().Be(CircuitState.Open);
                onBreakCalled.Should().Be(1);

                // Permit the second (long-running) execution to hit the open circuit with its failure.
                permitLongRunningExecutionToReturnItsFailure.Set();

                // Graceful cleanup: allow executions time to end naturally; timeout if any deadlocks; expose any execution faults.  This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
                longRunningExecution.Wait(testTimeoutToExposeDeadlocks).Should().BeTrue();
                if (longRunningExecution.IsFaulted) throw longRunningExecution.Exception;
                longRunningExecution.Status.Should().Be(TaskStatus.RanToCompletion);

                // onBreak() should still only have been called once.
                breaker.CircuitState.Should().Be(CircuitState.Open);
                onBreakCalled.Should().Be(1);
            }
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onreset_when_automatically_closing_circuit_but_not_when_halfopen(bool timespanIsDynamic) 
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, onBreak, onReset, timespanIsDynamic);

            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset
            onResetCalled.Should().Be(0);

            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => { });
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_not_call_onreset_on_successive_successful_calls(bool timespanIsDynamic)
        {
            Action<Exception, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, timespanIsDynamic);

            onResetCalled.Should().BeFalse();

            breaker.Execute(() => { });
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();

            breaker.Execute(() => { });
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_subsequent_execution(bool timespanIsDynamic)
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            int onHalfOpenCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, onBreak, onReset, onHalfOpen, timespanIsDynamic);

            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            // duration has passed, circuit now half open
            onHalfOpenCalled.Should().Be(0); // not yet transitioned to half-open, because we have not queried state

            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => { });
            onHalfOpenCalled.Should().Be(1);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_state_read(bool timespanIsDynamic)
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            int onHalfOpenCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, onBreak, onReset, onHalfOpen, timespanIsDynamic);

            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            onHalfOpenCalled.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onreset_when_manually_resetting_circuit(bool timespanIsDynamic)
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, onBreak, onReset, timespanIsDynamic);

            onBreakCalled.Should().Be(0);
            breaker.Isolate();
            onBreakCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Invoking(x => x.Execute(() => { }))
                .Should().Throw<IsolatedCircuitException>();

            onResetCalled.Should().Be(0);
            breaker.Reset();
            onResetCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Invoking(x => x.Execute(() => { })).Should().NotThrow();
        }

        #region Tests of supplied parameters to onBreak delegate

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onbreak_with_the_last_raised_exception(bool timespanIsDynamic)
        {
            Exception passedException = null;

            Action<Exception, TimeSpan, Context> onBreak = (exception, _, __) => { passedException = exception; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, onBreak, onReset, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            passedException?.Should().BeOfType<DivideByZeroException>();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onbreak_with_a_state_of_closed(bool timespanIsDynamic)
        {
            CircuitState? transitionedState = null;

            Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, __, ___) => { transitionedState = state; };
            Action<Context> onReset = _ => { };
            Action onHalfOpen = () => { };

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, onHalfOpen, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            transitionedState?.Should().Be(CircuitState.Closed);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onbreak_with_a_state_of_half_open(bool timespanIsDynamic)
        {
            List<CircuitState> transitionedStates = new List<CircuitState>();

            Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, __, ___) => { transitionedStates.Add(state); };
            Action<Context> onReset = _ => { };
            Action onHalfOpen = () => { };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, onHalfOpen, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // first call after duration raises an exception, so circuit should break again
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .Should().Throw<BrokenCircuitException>();

            transitionedStates[0].Should().Be(CircuitState.Closed);
            transitionedStates[1].Should().Be(CircuitState.HalfOpen);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_rethrow_and_call_onbreak_with_the_last_raised_exception_unwrapped_if_matched_as_inner(bool timespanIsDynamic)
        {
            Exception passedException = null;

            Action<Exception, TimeSpan, Context> onBreak = (exception, _, __) => { passedException = exception; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .HandleInner<DivideByZeroException>()
                .Or<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, onBreak, onReset, timespanIsDynamic);

            Exception toRaiseAsInner = new DivideByZeroException();
            Exception withInner = new AggregateException(toRaiseAsInner);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.Invoking(x => x.RaiseException(withInner))
                .Should().Throw<DivideByZeroException>().Which.Should().BeSameAs(toRaiseAsInner);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            passedException?.Should().BeSameAs(toRaiseAsInner);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onbreak_with_the_correct_timespan(bool timespanIsDynamic)
        {
            TimeSpan? passedBreakTimespan = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, timespan, __) => { passedBreakTimespan = timespan; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, onBreak, onReset, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            passedBreakTimespan.Should().Be(durationOfBreak);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_open_circuit_with_timespan_maxvalue_if_manual_override_open(bool timespanIsDynamic)
        {
            TimeSpan? passedBreakTimespan = null;
            Action<Exception, TimeSpan, Context> onBreak = (_, timespan, __) => { passedBreakTimespan = timespan; };
            Action<Context> onReset = _ => { };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, onBreak, onReset, timespanIsDynamic);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // manually break circuit
            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            passedBreakTimespan.Should().Be(TimeSpan.MaxValue);
        }

        #endregion

        #region Tests that supplied context is passed to stage-change delegates

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onbreak_with_the_passed_context(bool timespanIsDynamic)
        {
            IDictionary<string, object> contextData = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>(
                new {key1 = "value1", key2 = "value2"}.AsDictionary()
                )).Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_call_onreset_with_the_passed_context(bool timespanIsDynamic)
        {
            IDictionary<string, object> contextData = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, ___) => { };
            Action<Context> onReset = context => { contextData = context; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, onBreak, onReset, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration should invoke onReset, with context
            breaker.Execute(ctx => { }, new {key1 = "value1", key2 = "value2"}.AsDictionary());

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Context_should_be_empty_if_execute_not_called_with_any_context_data(bool timespanIsDynamic)
        {
            IDictionary<string, object> contextData = new {key1 = "value1", key2 = "value2"}.AsDictionary();

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should().BeEmpty();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_create_new_context_for_each_call_to_execute(bool timespanIsDynamic)
        {
            string contextValue = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };
            Action<Context> onReset = context => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), onBreak, onReset, timespanIsDynamic);

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>(new {key = "original_value"}.AsDictionary()))
                .Should().Throw<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            contextValue.Should().Be("original_value");

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset

            // first call after duration is successful, so circuit should reset
            breaker.Execute(ctx => { }, new {key = "new_value"}.AsDictionary());
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            contextValue.Should().Be("new_value");
        }

        #endregion

        #endregion

        #region LastException property

        [Theory, InlineData(true), InlineData(false)]
        public void Should_initialise_LastException_to_null_on_creation(bool timespanIsDynamic)
        {
            var breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.LastException.Should().BeNull();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_set_LastException_on_handling_exception_even_when_not_breaking(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.LastException.Should().BeOfType<DivideByZeroException>();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_set_LastException_on_handling_inner_exception_even_when_not_breaking(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                .HandleInner<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);


            Exception toRaiseAsInner = new DivideByZeroException();
            Exception withInner = new AggregateException(toRaiseAsInner);

            breaker.Invoking(x => x.RaiseException(withInner))
                .Should().Throw<DivideByZeroException>().Which.Should().BeSameAs(toRaiseAsInner);

            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.LastException.Should().BeSameAs(toRaiseAsInner);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_set_LastException_to_last_raised_exception_when_breaking(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.LastException.Should().BeOfType<DivideByZeroException>();
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_set_LastException_to_null_on_circuit_reset(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.LastException.Should().BeOfType<DivideByZeroException>();

            breaker.Reset();

            breaker.LastException.Should().BeNull();
        }

        #endregion

        #region ExecuteAndCapture with HandleInner

        [Theory, InlineData(true), InlineData(false)]
        public void Should_set_PolicyResult_on_handling_inner_exception(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                .HandleInner<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);


            Exception toRaiseAsInner = new DivideByZeroException();
            Exception withInner = new AggregateException(toRaiseAsInner);

            PolicyResult policyResult = breaker.ExecuteAndCapture(() => throw withInner);

            policyResult.ExceptionType.Should().Be(ExceptionType.HandledByThisPolicy);
            policyResult.FinalException.Should().BeSameAs(toRaiseAsInner);
        }

        #endregion

        #region Cancellation support

        [Theory, InlineData(true), InlineData(false)]
        public void Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled(bool timespanIsDynamic)
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().NotThrow();

            attemptsInvoked.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute(bool timespanIsDynamic)
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
            };

            cancellationTokenSource.Cancel();

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken(bool timespanIsDynamic)
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

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

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_report_cancellation_during_faulting_action_execution_when_user_delegate_observes_cancellationToken(bool timespanIsDynamic)
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_report_faulting_from_faulting_action_execution_when_user_delegate_does_not_observe_cancellation(bool timespanIsDynamic)
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<DivideByZeroException>();

            attemptsInvoked.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_report_cancellation_when_both_open_circuit_and_cancellation(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreaker_WithConditionalTimespan(1, TimeSpan.FromMinutes(1), timespanIsDynamic);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<DivideByZeroException>();

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .Should().Throw<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            // Circuit is now broken.

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            cancellationTokenSource.Cancel();

            PolicyExtensions.ExceptionAndOrCancellationScenario scenario = new PolicyExtensions.ExceptionAndOrCancellationScenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null, // Cancelled manually instead - see above.
                ActionObservesCancellation = false
            };

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_honour_different_cancellationToken_captured_implicitly_by_action(bool timespanIsDynamic)
        {
            // Before CancellationToken support was built in to Polly, users of the library may have implicitly captured a CancellationToken and used it to cancel actions.  For backwards compatibility, Polly should not confuse these with its own CancellationToken; it should distinguish OperationCanceledExceptions thrown with different CancellationTokens.

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

            CancellationTokenSource policyCancellationTokenSource = new CancellationTokenSource();
            CancellationToken policyCancellationToken = policyCancellationTokenSource.Token;

            CancellationTokenSource implicitlyCapturedActionCancellationTokenSource = new CancellationTokenSource();
            CancellationToken implicitlyCapturedActionCancellationToken = implicitlyCapturedActionCancellationTokenSource.Token;

            implicitlyCapturedActionCancellationTokenSource.Cancel();

            int attemptsInvoked = 0;

            breaker.Invoking(x => x.Execute(ct =>
            {
                attemptsInvoked++;
                implicitlyCapturedActionCancellationToken.ThrowIfCancellationRequested();
            }, policyCancellationToken))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(implicitlyCapturedActionCancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_execute_func_returning_value_when_cancellationToken_not_cancelled(bool timespanIsDynamic)
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker_WithConditionalTimespan(2, durationOfBreak, timespanIsDynamic);

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

            breaker.Invoking(x => result = x.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
                .Should().NotThrow();

            result.Should().BeTrue();

            attemptsInvoked.Should().Be(1);
        }

        [Theory, InlineData(true), InlineData(false)]
        public void Should_honour_and_report_cancellation_during_func_execution(bool timespanIsDynamic)
        {
            CircuitBreakerPolicy breaker = Policy
                             .Handle<DivideByZeroException>()
                             .CircuitBreaker_WithConditionalTimespan(2, TimeSpan.FromMinutes(1), timespanIsDynamic);

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

            breaker.Invoking(x => result = x.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
                .Should().Throw<OperationCanceledException>().And.CancellationToken.Should().Be(cancellationToken);

            result.Should().Be(null);

            attemptsInvoked.Should().Be(1);
        }

        #endregion

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }

    internal static class PolicyBuilderBExtension
    {
        internal static CircuitBreakerPolicy CircuitBreaker_WithConditionalTimespan(
            this PolicyBuilder builder,
            int exceptionsAllowedBeforeBreaking,
            TimeSpan durationOfBreak,
            bool timespanIsDynamic)
        {
            if (timespanIsDynamic)
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, durationOfBreak);
            }
            else
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, (_) => durationOfBreak);
            }
        }

        internal static CircuitBreakerPolicy CircuitBreaker_WithConditionalTimespan(
            this PolicyBuilder builder,
            int exceptionsAllowedBeforeBreaking,
            TimeSpan durationOfBreak,
            Action<Exception, TimeSpan> onBreak,
            Action onReset,
            bool timespanIsDynamic)
        {
            if (timespanIsDynamic)
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, durationOfBreak, onBreak, onReset);
            }
            else
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, (_) => durationOfBreak, onBreak, onReset);
            }
        }

        internal static CircuitBreakerPolicy CircuitBreaker_WithConditionalTimespan(
            this PolicyBuilder builder,
            int exceptionsAllowedBeforeBreaking,
            TimeSpan durationOfBreak,
            Action<Exception, TimeSpan, Context> onBreak,
            Action<Context> onReset,
            bool timespanIsDynamic)
        {
            if (timespanIsDynamic)
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, durationOfBreak, onBreak, onReset);
            }
            else
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, (_) => durationOfBreak, onBreak, onReset);
            }
        }

        internal static CircuitBreakerPolicy CircuitBreaker_WithConditionalTimespan(
            this PolicyBuilder builder,
            int exceptionsAllowedBeforeBreaking,
            TimeSpan durationOfBreak,
            Action<Exception, TimeSpan> onBreak,
            Action onReset,
            Action onHalfOpen,
            bool timespanIsDynamic)
        {
            if (timespanIsDynamic)
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, durationOfBreak, onBreak, onReset, onHalfOpen);
            }
            else
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, (_) => durationOfBreak, onBreak, onReset, onHalfOpen);
            }
        }

        internal static CircuitBreakerPolicy CircuitBreaker_WithConditionalTimespan(
            this PolicyBuilder builder,
            int exceptionsAllowedBeforeBreaking,
            TimeSpan durationOfBreak,
            Action<Exception, CircuitState, TimeSpan, Context> onBreak,
            Action<Context> onReset,
            Action onHalfOpen,
            bool timespanIsDynamic)
        {
            if (timespanIsDynamic)
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, durationOfBreak, onBreak, onReset, onHalfOpen);
            }
            else
            {
                return builder.CircuitBreaker(exceptionsAllowedBeforeBreaking, (_) => durationOfBreak, onBreak, onReset, onHalfOpen);
            }
        }
    }
}