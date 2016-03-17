using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

using Scenario = Polly.Specs.Helpers.PolicyExtensions.ExceptionAndOrCancellationScenario;

namespace Polly.Specs
{
    public class CircuitBreakerAsyncSpecs : IDisposable
    {
        [Fact]
        public void Should_be_able_to_handle_a_duration_of_timespan_maxvalue()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(1, TimeSpan.MaxValue);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_if_exceptions_allowed_before_breaking_is_less_than_one()
        {
           Action action = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .CircuitBreakerAsync(0, new TimeSpan());

            action.ShouldThrow<ArgumentOutOfRangeException>()
                  .And.ParamName.Should()
                  .Be("exceptionsAllowedBeforeBreaking");
        }

        [Fact]
        public void Should_initialise_to_closed_state()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, durationOfBreak);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_after_specified_number_of_specified_exception_have_been_raised()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>()
                  .WithMessage("The circuit is now open and is not allowing calls.")
                  .WithInnerException<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_after_specified_number_of_one_of_the_specified_exceptions_have_been_raised()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .Or<ArgumentOutOfRangeException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentOutOfRangeException>())
                  .ShouldThrow<ArgumentOutOfRangeException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>()
                  .WithMessage("The circuit is now open and is not allowing calls.")
                  .WithInnerException<ArgumentOutOfRangeException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_not_open_circuit_if_exception_raised_is_not_the_specified_exception()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_not_open_circuit_if_exception_raised_is_not_one_of_the_the_specified_exceptions()
        {
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .Or<ArgumentOutOfRangeException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_halfopen_circuit_after_the_specified_duration_has_passed()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // first call after duration raises an exception, so circuit should break again
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();

        }

        [Fact]
        public async Task Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_raise_an_exception()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // first call after duration is successful, so circuit should reset
            await breaker.ExecuteAsync(() => Task.FromResult(0));
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // circuit has been reset so should once again allow 2 exceptions to be raised before breaking
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_and_block_calls_if_manual_override_open()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // manually break circuit
            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            // circuit manually broken: execution should be blocked; even non-exception-throwing executions should not reset circuit
            breaker.Awaiting(x => x.ExecuteAsync(() => Task.FromResult(true)))
                .ShouldThrow<IsolatedCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

        }

        [Fact]
        public void Should_hold_circuit_open_despite_elapsed_time_if_manual_override_open()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Awaiting(x => x.ExecuteAsync(() => Task.FromResult(true)))
                .ShouldThrow<IsolatedCircuitException>();
        }

        [Fact]
        public void Should_close_circuit_again_on_reset_after_manual_override()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Isolate();
            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Awaiting(x => x.ExecuteAsync(() => Task.FromResult(true)))
                .ShouldThrow<IsolatedCircuitException>();

            breaker.Reset();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Awaiting(x => x.ExecuteAsync(() => Task.FromResult(true))).ShouldNotThrow();
        }

        [Fact]
        public void Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // reset circuit, with no time having passed
            breaker.Reset();
            SystemClock.UtcNow().Should().Be(time);
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Awaiting(x => x.ExecuteAsync(() => Task.FromResult(true))).ShouldNotThrow();
        }

        [Fact]
        public void Should_not_call_onreset_on_initialise()
        {
            Action<Exception, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            onResetCalled.Should().BeFalse();
        }

        [Fact]
        public void Should_call_onbreak_when_breaking_circuit_automatically()
        {
            bool onBreakCalled = false;
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled = true; };
            Action onReset = () => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().BeFalse();

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
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

            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);
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

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onBreakCalled.Should().Be(0);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            // call through circuit when already broken - should not retrigger onBreak 
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);
        }

        [Fact]
        public void Should_not_call_onreset_on_successive_successful_calls()
        {
            Action<Exception, TimeSpan> onBreak = (_, __) => { };
            bool onResetCalled = false;
            Action onReset = () => { onResetCalled = true; };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            onResetCalled.Should().BeFalse();

            breaker.ExecuteAsync(() => Task.FromResult(true));
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();

            breaker.ExecuteAsync(() => Task.FromResult(true));
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().BeFalse();
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

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().Be(0);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(0);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset
            onResetCalled.Should().Be(0);

            // first call after duration is successful, so circuit should reset
            breaker.ExecuteAsync(() => Task.FromResult(true));
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            onResetCalled.Should().Be(1);
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

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

            onBreakCalled.Should().Be(0);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(0);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            onBreakCalled.Should().Be(1);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            // duration has passed, circuit now half open
            onHalfOpenCalled.Should().Be(0); // not yet transitioned to half-open, because we have not queried state

            // first call after duration is successful, so circuit should reset
            breaker.ExecuteAsync(() => Task.FromResult(true));
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
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };
            Action onHalfOpen = () => { onHalfOpenCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset, onHalfOpen);

            onBreakCalled.Should().Be(0);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(0);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            onBreakCalled.Should().Be(1);

            // 2 exception raised, circuit is now open
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
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
            Action<Exception, TimeSpan> onBreak = (_, __) => { onBreakCalled++; };
            Action onReset = () => { onResetCalled++; };

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            onBreakCalled.Should().Be(0);
            breaker.Isolate();
            onBreakCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Isolated);
            breaker.Awaiting(x => x.ExecuteAsync(() => Task.FromResult(true)))
                .ShouldThrow<IsolatedCircuitException>();

            onResetCalled.Should().Be(0);
            breaker.Reset();
            onResetCalled.Should().Be(1);

            breaker.CircuitState.Should().Be(CircuitState.Closed);
            breaker.Awaiting(x => x.ExecuteAsync(() => Task.FromResult(true))).ShouldNotThrow();
        }

        [Fact]
        public void Should_call_onbreak_with_the_passed_context()
        {
            IDictionary<string, object> contextData = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextData = context; };
            Action<Context> onReset = _ => { };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(
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

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak, onBreak, onReset);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration should invoke onReset, with context
            breaker.ExecuteAsync(() => Task.FromResult(true), new { key1 = "value1", key2 = "value2" }.AsDictionary());

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

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            contextData.Should().BeEmpty();
        }

        [Fact]
        public void Should_create_new_context_for_each_call_to_execute()
        {
            string contextValue = null;

            Action<Exception, TimeSpan, Context> onBreak = (_, __, context) => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };
            Action<Context> onReset = context => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };

            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1), onBreak, onReset);

            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            // 2 exception raised, circuit is now open
            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>(new { key = "original_value" }.AsDictionary()))
                  .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            contextValue.Should().Be("original_value");

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
            // but not yet reset

            // first call after duration is successful, so circuit should reset
            breaker.ExecuteAsync(() => Task.FromResult(true), new { key = "new_value" }.AsDictionary());
            breaker.CircuitState.Should().Be(CircuitState.Closed);
            contextValue.Should().Be("new_value");
        }
        [Fact]
        public void Should_execute_action_when_non_faulting_and_cancellationtoken_not_cancelled()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null,
            };

            breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldNotThrow();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_not_execute_action_when_cancellationtoken_cancelled_before_execute()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 0,
                AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
            };

            cancellationTokenSource.Cancel();

            breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationtoken()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

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

            breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_action_execution_when_user_delegate_observes_cancellationtoken()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = true
            };

            breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_during_faulting_action_execution_when_user_delegate_does_not_observe_cancellationtoken()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = 1,
                ActionObservesCancellation = false
            };

            breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_report_cancellation_when_both_open_circuit_and_cancellation()
        {
            CircuitBreakerPolicy breaker = Policy
                .Handle<DivideByZeroException>()
                .CircuitBreakerAsync(1, TimeSpan.FromMinutes(1));

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                .ShouldThrow<DivideByZeroException>();

            breaker.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();
            // Circuit is now broken.

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            int attemptsInvoked = 0;
            Action onExecute = () => attemptsInvoked++;

            cancellationTokenSource.Cancel();

            Scenario scenario = new Scenario
            {
                NumberOfTimesToRaiseException = 1,
                AttemptDuringWhichToCancel = null, // Cancelled manually instead - see above.
                ActionObservesCancellation = false
            };

            breaker.Awaiting(x => x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);

            attemptsInvoked.Should().Be(0);
        }

        [Fact]
        public void Should_honour_different_cancellationtoken_captured_implicitly_by_action()
        {
            // Before CancellationToken support was built in to Polly, users of the library may have implicitly captured a CancellationToken and used it to cancel actions.  For backwards compatibility, Polly should not confuse these with its own CancellationToken; it should distinguish TaskCanceledExceptions thrown with different CancellationTokens.

            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            CancellationTokenSource policyCancellationTokenSource = new CancellationTokenSource();
            CancellationToken policyCancellationToken = policyCancellationTokenSource.Token;

            CancellationTokenSource implicitlyCapturedActionCancellationTokenSource = new CancellationTokenSource();
            CancellationToken implicitlyCapturedActionCancellationToken = implicitlyCapturedActionCancellationTokenSource.Token;

            implicitlyCapturedActionCancellationTokenSource.Cancel();

            int attemptsInvoked = 0;

            breaker.Awaiting(x => x.ExecuteAsync(async ct =>
            {
                attemptsInvoked++;
                await Task.FromResult(true);
                implicitlyCapturedActionCancellationToken.ThrowIfCancellationRequested();
            }, policyCancellationToken))
                .ShouldThrow<TaskCanceledException>()
                .And.CancellationToken.Should().Be(implicitlyCapturedActionCancellationToken);

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_execute_func_returning_value_when_cancellationtoken_not_cancelled()
        {
            var durationOfBreak = TimeSpan.FromMinutes(1);
            CircuitBreakerPolicy breaker = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

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

            breaker.Awaiting(async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true).ConfigureAwait(false))
                .ShouldNotThrow();

            result.Should().BeTrue();

            attemptsInvoked.Should().Be(1);
        }

        [Fact]
        public void Should_honour_and_report_cancellation_during_func_execution()
        {
            CircuitBreakerPolicy breaker = Policy
                             .Handle<DivideByZeroException>()
                             .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

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

            breaker.Awaiting(async x => result = await x.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true).ConfigureAwait(false))
                .ShouldThrow<TaskCanceledException>().And.CancellationToken.Should().Be(cancellationToken);

            result.Should().Be(null);

            attemptsInvoked.Should().Be(1);
        }
        

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}