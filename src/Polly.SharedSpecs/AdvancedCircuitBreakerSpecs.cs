using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs
{
    public class AdvancedCircuitBreakerSpecs : IDisposable
    {
        #region Configuration tests

        [Fact]
        public void Should_be_able_to_handle_a_duration_of_timespan_maxvalue()
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.MaxValue);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_if_failure_threshold_is_zero()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("failureThreshold");
        }

        [Fact]
        public void Should_throw_if_failure_threshold_is_less_than_zero()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(-0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("failureThreshold");
        }

        [Fact]
        public void Should_be_able_to_handle_a_failure_threshold_of_one()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(1.0, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

            action.ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_if_failure_threshold_is_greater_than_one()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(1.01, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("failureThreshold");
        }

        [Fact]
        public void Should_throw_if_timeslice_duration_is_less_than_resolution_of_circuit()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    0.5, 
                    TimeSpan.FromMilliseconds(20).Add(TimeSpan.FromTicks(-1)), 
                    4, 
                    TimeSpan.FromSeconds(30));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("timesliceDuration");
        }

        [Fact]
        public void Should_not_throw_if_timeslice_duration_is_resolution_of_circuit()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromMilliseconds(20), 4, TimeSpan.FromSeconds(30));

            action.ShouldNotThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Should_throw_if_minimum_throughput_is_zero()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 0, TimeSpan.FromSeconds(30));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("minimumThroughput");
        }

        [Fact]
        public void Should_throw_if_minimum_throughput_is_less_than_zero()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), -1, TimeSpan.FromSeconds(30));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("minimumThroughput");
        }

        [Fact]
        public void Should_throw_if_duration_of_break_is_less_than_zero()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, -TimeSpan.FromSeconds(1));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("durationOfBreak");
        }

        [Fact]
        public void Should_be_able_to_handle_a_duration_of_break_of_zero()
        {
            Action action = () => Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.Zero);

            action.ShouldNotThrow();
        }

        [Fact]
        public void Should_initialise_to_closed_state()
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Circuit-breaker threshold-to-break tests

        #region Tests that are independent from health metrics implementation

        // Tests on the AdvancedCircuitBreaker operation typically use a breaker: 
        // - with a failure threshold of >=50%, 
        // - and a throughput threshold of 4
        // - across a ten-second period.
        // These provide easy values for testing for failure and throughput thresholds each being met and non-met, in combination.

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_and_minimum_threshold_is_equalled_but_last_call_is_success()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Three of three actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            // Failure threshold exceeded, but throughput threshold not yet.

            // Throughput threshold will be exceeded by the below successful call, but we never break on a successful call; hence don't break on this.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
        }

        [Fact]
        public void Should_not_open_circuit_if_exceptions_raised_are_not_one_of_the_the_specified_exceptions()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .Or<ArgumentOutOfRangeException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw unhandled failures.
            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
                .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region With sample duration higher than 199 ms so that multiple windows are used

        // Tests on the AdvancedCircuitBreaker operation typically use a breaker: 
        // - with a failure threshold of >=50%, 
        // - and a throughput threshold of 4
        // - across a ten-second period.
        // These provide easy values for testing for failure and throughput thresholds each being met and non-met, in combination.

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_and_throughput_threshold_equalled_within_timeslice_in_same_window()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_and_throughput_threshold_equalled_within_timeslice_in_different_windows()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Placing the rest of the invocations ('timesliceDuration' / 2) + 1 seconds later
            // ensures that even if there are only two windows, then the invocations are placed in the second.
            // They are still placed within same timeslice.
            SystemClock.UtcNow = () => time.AddSeconds(timesliceDuration.Seconds / 2d + 1);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_though_not_all_are_failures_and_throughput_threshold_equalled_within_timeslice_in_same_window()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Three of four actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_though_not_all_are_failures_and_throughput_threshold_equalled_within_timeslice_in_different_windows()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Three of four actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // Placing the rest of the invocations ('timesliceDuration' / 2) + 1 seconds later
            // ensures that even if there are only two windows, then the invocations are placed in the second.
            // They are still placed within same timeslice
            SystemClock.UtcNow = () => time.AddSeconds(timesliceDuration.Seconds / 2d + 1);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_within_timeslice_in_same_window()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Two of four actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_within_timeslice_in_different_windows()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Two of four actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // Placing the rest of the invocations ('timesliceDuration' / 2) + 1 seconds later
            // ensures that even if there are only two windows, then the invocations are placed in the second.
            // They are still placed within same timeslice
            SystemClock.UtcNow = () => time.AddSeconds(timesliceDuration.Seconds / 2d + 1);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw handled failures; but only the first three within the timeslice.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Adjust SystemClock so that timeslice (clearly) expires; fourth exception thrown in next-recorded timeslice.
            SystemClock.UtcNow = () => time.Add(timesliceDuration).Add(timesliceDuration);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

        }

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_even_if_timeslice_expires_only_exactly()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw handled failures; but only the first three within the timeslice.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Adjust SystemClock so that timeslice (just) expires; fourth exception thrown in following timeslice.
            SystemClock.UtcNow = () => time.Add(timesliceDuration);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

        }

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_even_if_error_occurring_just_at_the_end_of_the_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw handled failures; but only the first three within the original timeslice.

            // Two actions at the start of the original timeslice.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Creates a new window right at the end of the original timeslice.
            SystemClock.UtcNow = () => time.AddTicks(timesliceDuration.Ticks - 1);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Adjust SystemClock so that timeslice (just) expires; fourth exception thrown in following timeslice.  If timeslice/window rollover is precisely defined, this should cause first two actions to be forgotten from statistics (rolled out of the window of relevance), and thus the circuit not to break.
            SystemClock.UtcNow = () => time.Add(timesliceDuration);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_even_if_only_just_within_timeslice()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Adjust SystemClock so that timeslice doesn't quite expire; fourth exception thrown in same timeslice.
            SystemClock.UtcNow = () => time.AddTicks(timesliceDuration.Ticks - 1);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_not_met_and_throughput_threshold_not_met()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // One of three actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
        }

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_not_met_but_throughput_threshold_met_before_timeslice_expires()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // One of four actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
        }

        [Fact]
        public void Should_open_circuit_if_failures_at_end_of_last_timeslice_below_failure_threshold_and_failures_in_beginning_of_new_timeslice_where_total_equals_failure_threshold()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Executing a single invocation to ensure timeslice is created
            // This invocation is not be counted against the threshold
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // The time is set to just at the end of the timeslice duration ensuring
            // the invocations are within the timeslice, but only barely.
            SystemClock.UtcNow = () => time.AddTicks(timesliceDuration.Ticks - 1);

            // Three of four actions in this test occur within the first timeslice.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Setting the time to just barely into the new timeslice
            SystemClock.UtcNow = () => time.Add(timesliceDuration);

            // This failure opens the circuit, because it is the second failure of four calls
            // equalling the failure threshold. The minimum threshold within the defined
            // timeslice duration is met, when using rolling windows.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_not_open_circuit_if_failures_at_end_of_last_timeslice_and_failures_in_beginning_of_new_timeslice_when_below_minimum_throughput_threshold()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Executing a single invocation to ensure timeslice is created
            // This invocation is not be counted against the threshold
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // The time is set to just at the end of the timeslice duration ensuring
            // the invocations are within the timeslice, but only barely.
            SystemClock.UtcNow = () => time.AddTicks(timesliceDuration.Ticks - 1);

            // Two of three actions in this test occur within the first timeslice.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Setting the time to just barely into the new timeslice
            SystemClock.UtcNow = () => time.Add(timesliceDuration);

            // A third failure occurs just at the beginning of the new timeslice making 
            // the number of failures above the failure threshold. However, the throughput is 
            // below the minimum threshold as to open the circuit.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_open_circuit_if_failures_in_second_window_of_last_timeslice_and_failures_in_first_window_in_next_timeslice_exceeds_failure_threshold_and_minimum_threshold()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromSeconds(10);
            var numberOfWindowsDefinedInCircuitBreaker = 10;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Executing a single invocation to ensure timeslice is created
            // This invocation is not be counted against the threshold
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Setting the time to the second window in the rolling metrics
            SystemClock.UtcNow = () => time.AddSeconds(timesliceDuration.Seconds / (double)numberOfWindowsDefinedInCircuitBreaker);

            // Three actions occur in the second window of the first timeslice
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Setting the time to just barely into the new timeslice
            SystemClock.UtcNow = () => time.Add(timesliceDuration);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        #endregion

        #region With sample duration at 199 ms so that only a single window is used

        // These tests on AdvancedCircuitBreaker operation typically use a breaker: 
        // - with a failure threshold of >=50%, 
        // - and a throughput threshold of 4
        // - across a 199ms period.
        // These provide easy values for testing for failure and throughput thresholds each being met and non-met, in combination.

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_and_throughput_threshold_equalled_within_timeslice_low_samping_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromMilliseconds(199),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_though_not_all_are_failures_and_throughput_threshold_equalled_within_timeslice_low_samping_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromMilliseconds(199),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Three of four actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_within_timeslice_low_samping_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromMilliseconds(199),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Two of four actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_low_samping_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromMilliseconds(199);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw handled failures; but only the first within the timeslice.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Adjust SystemClock so that timeslice (clearly) expires; fourth exception thrown in next-recorded timeslice.
            SystemClock.UtcNow = () => time.Add(timesliceDuration).Add(timesliceDuration);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

        }

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_even_if_timeslice_expires_only_exactly_low_samping_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromMilliseconds(199);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Two of four actions in this test throw handled failures; but only the first within the timeslice.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Adjust SystemClock so that timeslice (just) expires; fourth exception thrown in following timeslice.
            SystemClock.UtcNow = () => time.Add(timesliceDuration);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_even_if_only_just_within_timeslice_low_samping_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromMilliseconds(199);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Adjust SystemClock so that timeslice doesn't quite expire; fourth exception thrown in same timeslice.
            SystemClock.UtcNow = () => time.AddTicks(timesliceDuration.Ticks - 1);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

        }

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_not_met_and_throughput_threshold_not_met_low_samping_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromMilliseconds(199),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // One of three actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
        }

        [Fact]
        public void Should_not_open_circuit_if_failure_threshold_not_met_but_throughput_threshold_met_before_timeslice_expires_low_samping_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromMilliseconds(199),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // One of four actions in this test throw handled failures.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
        }

        [Fact]
        public void Should_not_open_circuit_if_failures_at_end_of_last_timeslice_below_failure_threshold_and_failures_in_beginning_of_new_timeslice_where_total_equals_failure_threshold_low_samping_duration()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var timesliceDuration = TimeSpan.FromMilliseconds(199);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                );

            // Executing a single invocation to ensure timeslice is created
            // This invocation is not be counted against the threshold
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // The time is set to just at the end of the timeslice duration ensuring
            // the invocations are within the timeslice, but only barely.
            SystemClock.UtcNow = () => time.AddTicks(timesliceDuration.Ticks - 1);

            // Three of four actions in this test occur within the first timeslice.
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldNotThrow();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Setting the time to just barely into the new timeslice
            SystemClock.UtcNow = () => time.Add(timesliceDuration);

            // This failure does not open the circuit, because a new duration should have 
            // started and with such low sampling duration, windows should not be used.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #endregion

        #region Circuit-breaker open->half-open->open/closed tests

        [Fact]
        public void Should_halfopen_circuit_after_the_specified_duration_has_passed_with_failures_in_same_window()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_halfopen_circuit_after_the_specified_duration_has_passed_with_failures_in_different_windows()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);
            var timesliceDuration = TimeSpan.FromSeconds(10);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: timesliceDuration,
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // Placing the rest of the invocations ('timesliceDuration' / 2) + 1 seconds later
            // ensures that even if there are only two windows, then the invocations are placed in the second.
            // They are still placed within same timeslice
            var anotherwindowDuration = timesliceDuration.Seconds / 2d + 1;
            SystemClock.UtcNow = () => time.AddSeconds(anotherwindowDuration);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // Since the call that opened the circuit occurred in a later window, then the
            // break duration must be simulated as from that call.
            SystemClock.UtcNow = () => time.Add(durationOfBreak).AddSeconds(anotherwindowDuration);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration raises an exception, so circuit should open again
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>();
            
        }

        [Fact]
        public void Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_raise_an_exception()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => {});
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Isolate and reset tests

        [Fact]
        public void Should_open_circuit_and_block_calls_if_manual_override_open()
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // manually break circuit
            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            // circuit manually broken: execution should be blocked; even non-exception-throwing executions should not reset circuit
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldThrow<IsolatedCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
        }

        [Fact]
        public void Should_hold_circuit_open_despite_elapsed_time_if_manual_override_open()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldThrow<IsolatedCircuitException>();
        }

        [Fact]
        public void Should_close_circuit_again_on_reset_after_manual_override()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldThrow<IsolatedCircuitException>();

            breaker.Reset();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Invoking(x => x.Execute(() => { })).ShouldNotThrow();
        }

        [Fact]
        public void Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // reset circuit, with no time having passed
            breaker.Reset();
            SystemClock.UtcNow().Should().Be(time);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Invoking(x => x.Execute(() => { })).ShouldNotThrow();
        }

        #endregion

        #region State-change delegate tests

        [Fact]
        public void Should_not_call_onreset_on_initialise()
        {
            Action<Exception, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            var durationOfBreak = TimeSpan.FromSeconds(30);
            Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak, onBreak, onReset);

            onResetCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_automatically()
        {
            bool onBreakCalled = false;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: onBreak,
                    onReset: onReset
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            onBreakCalled.Should().BeTrue();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_manually()
        {
            bool onBreakCalled = false;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            var durationOfBreak = TimeSpan.FromSeconds(30);
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().BeFalse();

            breaker.Isolate();

            onBreakCalled.Should().BeTrue();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_through_open_circuit()
        {
            int onBreakCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: onBreak,
                    onReset: onReset
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            onBreakCalled.Should().Be(1);

            // call through circuit when already broken - should not retrigger onBreak 
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);
        }

        [Fact]
        public void Should_call_onreset_when_automatically_closing_circuit_but_not_when_halfopen()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak,
                    onBreak: onBreak,
                    onReset: onReset
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
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

        [Fact]
        public void Should_not_call_onreset_on_successive_successful_calls()
        {
            Action<Exception, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            var durationOfBreak = TimeSpan.FromSeconds(30);
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak, onBreak, onReset);

            onResetCalled.Should().BeFalse();

            breaker.Execute(() => { });
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();

            breaker.Execute(() => { });
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_subsequent_execution()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            int onHalfOpenCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak,
                    onBreak: onBreak,
                    onReset: onReset,
                    onHalfOpen: onHalfOpen
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            // duration has passed, circuit now half open
            onHalfOpenCalled.Should().Be(0); // not yet transitioned to half-open, because we have not queried state

            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => { });
            onHalfOpenCalled.Should().Be(1); // called as action was placed for execution
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().Be(1); // called after action succeeded
        }

        [Fact]
        public void Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_state_read()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            int onHalfOpenCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak,
                    onBreak: onBreak,
                    onReset: onReset,
                    onHalfOpen: onHalfOpen
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            onHalfOpenCalled.Should().Be(1);
        }

        [Fact]
        public void Should_call_onreset_when_manually_resetting_circuit()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().Be(0);
            breaker.Isolate();
            onBreakCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Invoking(x => x.Execute(() => { }))
                .ShouldThrow<IsolatedCircuitException>();

            onResetCalled.Should().Be(0);
            breaker.Reset();
            onResetCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Invoking(x => x.Execute(() => { })).ShouldNotThrow();
        }

        #region Tests that supplied context is passed to stage-change delegates

        [Fact]
        public void Should_call_onbreak_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: onBreak,
                    onReset: onReset
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>(
                new { key1 = "value1", key2 = "value2" }.AsDictionary()
                )).ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onreset_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, ___) => { };
            Action<Context> onReset = context => { contextData = context; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak,
                    onBreak: onBreak,
                    onReset: onReset
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);


            // first call after duration should invoke onReset, with context
            breaker.Execute(() => { }, new { key1 = "value1", key2 = "value2" }.AsDictionary());
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            IDictionary<string, object> contextData = new { key1 = "value1", key2 = "value2" }.AsDictionary();

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: onBreak,
                    onReset: onReset
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should().BeEmpty();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_execute()
        {
            string contextValue = null;

            Action<Exception, TimeSpan, Context> onBreak =
                (_, __, context) => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };
            Action<Context> onReset =
                context => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromSeconds(30);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .AdvancedCircuitBreaker(
                    failureThreshold: 0.5,
                    timesliceDuration: TimeSpan.FromSeconds(10),
                    minimumThroughput: 4,
                    durationOfBreak: durationOfBreak,
                    onBreak: onBreak,
                    onReset: onReset
                );

            // Four of four actions in this test throw handled failures.
            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(x => x.RaiseException<DivideByZeroException>(new { key = "original_value" }.AsDictionary()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            contextValue.Should().Be("original_value");

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset

            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => { }, new { key = "new_value" }.AsDictionary());
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            contextValue.Should().Be("new_value");
        }

        #endregion

        #endregion

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}
