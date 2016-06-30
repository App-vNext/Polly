using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs
{
    public class CircuitBreakerTResultSpecs : IDisposable
    {
        #region Configuration tests

        [Fact]
        public void Should_be_able_to_handle_a_duration_of_timespan_maxvalue()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(1, TimeSpan.MaxValue);

            var result = breaker.RaiseResultSequence(ResultPrimitive.Fault);
            result.Should().Be(ResultPrimitive.Fault);
        }

        [Fact]
        public void Should_throw_if_faults_allowed_before_breaking_is_less_than_one()
        {
            Action action = () => Policy
                                     .HandleResult(ResultPrimitive.Fault)
                                     .CircuitBreaker(0, TimeSpan.FromSeconds(10));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                  .And.ParamName.Should()
                  .Be("handledEventsAllowedBeforeBreaking");
        }

        [Fact]
        public void Should_throw_if_duration_of_break_is_less_than_zero()
        {
            Action action = () => Policy
                                     .HandleResult(ResultPrimitive.Fault)
                                     .CircuitBreaker(1, -TimeSpan.FromSeconds(1));

            action.ShouldThrow<ArgumentOutOfRangeException>()
                .And.ParamName.Should()
                .Be("durationOfBreak");
        }

        [Fact]
        public void Should_be_able_to_handle_a_duration_of_break_of_zero()
        {
            Action action = () => Policy
                                     .HandleResult(ResultPrimitive.Fault)
                                     .CircuitBreaker(1, TimeSpan.Zero);
            action.ShouldNotThrow();
        }

        [Fact]
        public void Should_initialise_to_closed_state()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Circuit-breaker threshold-to-break tests

        [Fact]
        public void Should_not_open_circuit_if_specified_number_of_specified_handled_result_are_not_raised_consecutively()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Good)
                  .Should().Be(ResultPrimitive.Good);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_handled_result_after_specified_number_of_specified_handled_result_have_been_returned()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Fault))
                .ShouldThrow<BrokenCircuitException<ResultPrimitive>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e.Result == ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_handled_result_after_specified_number_of_one_of_the_specified_handled_results_have_been_raised()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .OrResult(ResultPrimitive.FaultAgain)
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Fault))
                .ShouldThrow<BrokenCircuitException<ResultPrimitive>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e.Result == ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_handled_result_after_specified_number_of_specified_handled_result_with_predicate_have_been_returned()
        {
            CircuitBreakerPolicy<ResultClass> breaker = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.Fault))
                .ResultCode.Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.Fault))
                .ResultCode.Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(b => b.RaiseResultSequence(new ResultClass(ResultPrimitive.Good)))
                .ShouldThrow<BrokenCircuitException<ResultClass>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e.Result.ResultCode == ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_not_open_circuit_if_result_returned_is_not_the_handled_result()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_not_open_circuit_if_result_returned_is_not_one_of_the_handled_results()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .OrResult(ResultPrimitive.FaultYetAgain)
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_not_open_circuit_if_result_returned_does_not_match_result_predicate()
        {
            CircuitBreakerPolicy<ResultClass> breaker = Policy
                            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
                  .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
                  .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
                  .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_not_open_circuit_if_result_returned_does_not_match_any_of_the_result_predicates()
        {
            CircuitBreakerPolicy<ResultClass> breaker = Policy
                .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .OrResult(r => r.ResultCode == ResultPrimitive.FaultYetAgain)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
                  .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
                  .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
                  .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Circuit-breaker open->half-open->open/closed tests

        [Fact]
        public void Should_halfopen_circuit_after_the_specified_duration_has_passed()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, durationOfBreak);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Fault))
                .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
        }

        [Fact]
        public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_a_fault()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, durationOfBreak);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration returns a fault, so circuit should break again
            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();

        }

        [Fact]
        public void Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_return_a_fault()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, durationOfBreak);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => ResultPrimitive.Good);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // circuit has been reset so should once again allow 2 faults to be raised before breaking
            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        #endregion

        #region Isolate and reset tests

        [Fact]
        public void Should_open_circuit_and_block_calls_if_manual_override_open()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // manually break circuit
            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            // circuit manually broken: execution should be blocked; even non-fault-returning executions should not reset circuit
            bool delegateExecutedWhenBroken = false;
            breaker.Invoking(x => x.Execute(() => { delegateExecutedWhenBroken = true; return ResultPrimitive.Good;}))
                .ShouldThrow<IsolatedCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            delegateExecutedWhenBroken.Should().BeFalse();

        }

        [Fact]
        public void Should_hold_circuit_open_despite_elapsed_time_if_manual_override_open()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            bool delegateExecutedWhenBroken = false;
            breaker.Invoking(x => x.Execute(() => { delegateExecutedWhenBroken = true; return ResultPrimitive.Good; }))
                .ShouldThrow<IsolatedCircuitException>();
            delegateExecutedWhenBroken.Should().BeFalse();
        }

        [Fact]
        public void Should_close_circuit_again_on_reset_after_manual_override()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good))
                .ShouldThrow<IsolatedCircuitException>();

            breaker.Reset();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good)).ShouldNotThrow();
        }

        [Fact]
        public void Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, durationOfBreak);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // reset circuit, with no time having passed
            breaker.Reset();
            SystemClock.UtcNow().Should().Be(time);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good)).ShouldNotThrow();
        }

        #endregion

        #region State-change delegate tests

        [Fact]
        public void Should_not_call_onreset_on_initialise()
        {
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            onResetCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_automatically()
        {
            bool onBreakCalled = false;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().BeFalse();

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().BeTrue();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_manually()
        {
            bool onBreakCalled = false;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);
            onBreakCalled.Should().BeFalse();

            breaker.Isolate();

            onBreakCalled.Should().BeTrue();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_through_open_circuit()
        {
            int onBreakCalled = 0;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            // call through circuit when already broken - should not retrigger onBreak 
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good))
                  .ShouldThrow<BrokenCircuitException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);
        }

        [Fact]
        public void Should_call_onreset_when_automatically_closing_circuit_but_not_when_halfopen()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().Be(0);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(0);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset
            onResetCalled.Should().Be(0);

            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => ResultPrimitive.Good);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().Be(1);
        }

        [Fact]
        public void Should_not_call_onreset_on_successive_successful_calls()
        {
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            onResetCalled.Should().BeFalse();

            breaker.Execute(() => ResultPrimitive.Good);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();

            breaker.Execute(() => ResultPrimitive.Good);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_subsequent_execution()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            int onHalfOpenCalled = 0;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, durationOfBreak, onBreak, onReset, onHalfOpen);

            onBreakCalled.Should().Be(0);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(0);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            // duration has passed, circuit now half open
            onHalfOpenCalled.Should().Be(0); // not yet transitioned to half-open, because we have not queried state

            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => ResultPrimitive.Good);
            onHalfOpenCalled.Should().Be(1);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().Be(1);
        }

        [Fact]
        public void Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_state_read()
        {
            int onBreakCalled = 0;
            int onResetCalled = 0;
            int onHalfOpenCalled = 0;
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, durationOfBreak, onBreak, onReset, onHalfOpen);

            onBreakCalled.Should().Be(0);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(0);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good))
                  .ShouldThrow<BrokenCircuitException>();
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
            Action<DelegateResult<ResultPrimitive>, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .CircuitBreaker(2, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().Be(0);
            breaker.Isolate();
            onBreakCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good))
                .ShouldThrow<IsolatedCircuitException>();

            onResetCalled.Should().Be(0);
            breaker.Reset();
            onResetCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Invoking(x => x.Execute(() => ResultPrimitive.Good)).ShouldNotThrow();
        }

        #region Tests of supplied parameters to onBreak delegate

        [Fact]
        public void Should_call_onbreak_with_the_last_handled_result()
        {
            ResultPrimitive? handledResult = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (outcome, _, __) => { handledResult = outcome.Result; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak, onBreak, onReset);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            handledResult?.Should().Be(ResultPrimitive.Fault);
        }

        [Fact]
        public void Should_call_onbreak_with_the_correct_timespan()
        {
            TimeSpan? passedBreakTimespan = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, timespan, __) => { passedBreakTimespan = timespan; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak, onBreak, onReset);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            passedBreakTimespan.Should().Be(durationOfBreak);
        }

        [Fact]
        public void Should_open_circuit_with_timespan_maxvalue_if_manual_override_open()
        {
            TimeSpan? passedBreakTimespan = null;
            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, timespan, __) => { passedBreakTimespan = timespan; };
            Action<Context> onReset = _ => { };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak, onBreak, onReset);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // manually break circuit
            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            passedBreakTimespan.Should().Be(TimeSpan.MaxValue);
        }

        #endregion

        #region Tests that supplied context is passed to stage-change delegates

        [Fact]
        public void Should_call_onbreak_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.RaiseResultSequence(new {key1 = "value1", key2 = "value2"}.AsDictionary(),
                ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Should_call_onreset_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, __, ___) => { };
            Action<Context> onReset = context => { contextData = context; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak, onBreak, onReset);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);
            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration should invoke onReset, with context
            breaker.Execute(() => ResultPrimitive.Good, new { key1 = "value1", key2 = "value2" }.AsDictionary());

            contextData.Should()
                .ContainKeys("key1", "key2").And
                .ContainValues("value1", "value2");
        }

        [Fact]
        public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
        {
            IDictionary<string, object> contextData = new { key1 = "value1", key2 = "value2" }.AsDictionary();

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should().BeEmpty();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_execute()
        {
            string contextValue = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (_, __, context) => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };
            Action<Context> onReset = context => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .HandleResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            // 2 exception raised, circuit is now open
            breaker.RaiseResultSequence(new { key = "original_value" }.AsDictionary(), ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);
            contextValue.Should().Be("original_value");

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset

            // first call after duration is successful, so circuit should reset
            breaker.Execute(() => ResultPrimitive.Good, new { key = "new_value" }.AsDictionary());
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