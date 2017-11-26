using System;
using System.Collections.Generic;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs.CircuitBreaker
{
    [Collection("SystemClockDependantCollection")]
    public class CircuitBreakerTResultMixedResultExceptionSpecs : IDisposable
    {
        #region Circuit-breaker threshold-to-break tests

        [Fact]
        public void Should_open_circuit_with_exception_after_specified_number_of_specified_exception_have_been_returned_when_result_policy_handling_exceptions_only()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy<ResultPrimitive>
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Good))
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_exception_after_specified_number_of_exceptions_and_results_have_been_raised__breaking_on_result__when_configuring_result_first()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .Or<DivideByZeroException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Good))
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_handled_result_after_specified_number_of_exceptions_and_results_have_been_raised__breaking_on_result__when_configuring_result_first()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .Or<DivideByZeroException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Good))
                .ShouldThrow<BrokenCircuitException<ResultPrimitive>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e. Result == ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_exception_after_specified_number_of_exceptions_and_results_have_been_raised__breaking_on_result__when_configuring_exception_first()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Good))
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_handled_result_after_specified_number_of_exceptions_and_results_have_been_raised__breaking_on_result__when_configuring_exception_first()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Good))
                .ShouldThrow<BrokenCircuitException<ResultPrimitive>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e.Result == ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_if_results_and_exceptions_returned_match_combination_of_the_result_and_exception_predicates()
        {
            CircuitBreakerPolicy<ResultClass> breaker = Policy
                .Handle<ArgumentException>(e => e.ParamName == "key")
                .OrResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException("message", "key")))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.Fault))
                  .ResultCode.Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(new ResultClass(ResultPrimitive.Good)))
                .ShouldThrow<BrokenCircuitException<ResultClass>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e.Result.ResultCode == ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_not_open_circuit_if_result_returned_is_not_one_of_the_configured_results_or_exceptions()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
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
        public void Should_not_open_circuit_if_exception_thrown_is_not_one_of_the_configured_results_or_exceptions()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException()))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException()))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException()))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact]
        public void Should_not_open_circuit_if_result_returned_does_not_match_any_of_the_result_predicates()
        {
            CircuitBreakerPolicy<ResultClass> breaker = Policy
                .Handle<ArgumentException>(e => e.ParamName == "key")
                .OrResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            // non-matched result predicate
            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
                  .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
                  .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
                  .ResultCode.Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            // non-matched exception predicate
            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException("message", "value")))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException("message", "value")))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException("message", "value")))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        [Fact] public void Should_open_circuit_with_the_last_exception_after_specified_number_of_exceptions_and_results_have_been_raised__configuring_multiple_results_and_exceptions()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .Or<ArgumentException>()
                .OrResult(ResultPrimitive.FaultAgain)
                .CircuitBreaker(4, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException()))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 4 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Good))
                .ShouldThrow<BrokenCircuitException>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .WithInnerException<ArgumentException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }

        [Fact]
        public void Should_open_circuit_with_the_last_handled_result_after_specified_number_of_exceptions_and_results_have_been_raised__when_configuring_multiple_results_and_exceptions()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .Or<ArgumentException>()
                .OrResult(ResultPrimitive.FaultAgain)
                .CircuitBreaker(4, TimeSpan.FromMinutes(1));

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException()))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 4 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Good))
                .ShouldThrow<BrokenCircuitException<ResultPrimitive>>()
                .WithMessage("The circuit is now open and is not allowing calls.")
                .Where(e => e.Result == ResultPrimitive.FaultAgain);

            breaker.CircuitState.Should().Be(CircuitState.Open);
        }
        
        [Fact]
        public void Should_not_open_circuit_if_result_raised_or_exception_thrown_is_not_one_of_the_handled_results_or_exceptions()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .Or<DivideByZeroException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException()))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
                  .Should().Be(ResultPrimitive.FaultAgain);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new ArgumentException()))
                .ShouldThrow<ArgumentException>();
            breaker.CircuitState.Should().Be(CircuitState.Closed);
        }

        #endregion

        #region Circuit-breaker open->half-open->open/closed tests

        [Fact]
        public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_a_fault()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .Or<DivideByZeroException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
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
        public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                            .HandleResult(ResultPrimitive.Fault)
                            .Or<DivideByZeroException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                  .Should().Be(ResultPrimitive.Fault);
            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            // 2 exception raised, circuit is now open
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

            // first call after duration returns a fault, so circuit should break again
            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();
            breaker.CircuitState.Should().Be(CircuitState.Open);
            breaker.Invoking(b => b.RaiseResultSequence(ResultPrimitive.Fault))
                  .ShouldThrow<BrokenCircuitException>();

        }

        #endregion

        #region State-change delegate tests

        #region Tests of supplied parameters to onBreak delegate

        [Fact]
        public void Should_call_onbreak_with_the_last_handled_result()
        {
            ResultPrimitive? handledResult = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (outcome, _, __) => { handledResult = outcome.Result; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak, onBreak, onReset);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            handledResult?.Should().Be(ResultPrimitive.Fault);
        }

        [Fact]
        public void Should_call_onbreak_with_the_last_raised_exception()
        {
            Exception lastException = null;

            Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (outcome, _, __) => { lastException = outcome.Exception; };
            Action<Context> onReset = _ => { };

            TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, durationOfBreak, onBreak, onReset);

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            lastException.Should().BeOfType<DivideByZeroException>();
        }

        #endregion

        #endregion

        #region LastHandledResult and LastException property

        [Fact]
        public void Should_initialise_LastHandledResult_and_LastResult_to_default_on_creation()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.LastHandledResult.Should().Be(default(ResultPrimitive));
            breaker.LastException.Should().BeNull();
        }

        [Fact]
        public void Should_set_LastHandledResult_on_handling_result_even_when_not_breaking()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.LastHandledResult.Should().Be(ResultPrimitive.Fault);
            breaker.LastException.Should().BeNull();
        }

        [Fact]
        public void Should_set_LastException_on_exception_even_when_not_breaking()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Closed);

            breaker.LastHandledResult.Should().Be(default(ResultPrimitive));
            breaker.LastException.Should().BeOfType<DivideByZeroException>();
        }

        [Fact]
        public void Should_set_LastHandledResult_to_last_handled_result_when_breaking()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.LastHandledResult.Should().Be(ResultPrimitive.Fault);
            breaker.LastException.Should().BeNull();
        }

        [Fact]
        public void Should_set_LastException_to_last_exception_when_breaking()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();

            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.LastHandledResult.Should().Be(default(ResultPrimitive));
            breaker.LastException.Should().BeOfType<DivideByZeroException>();
        }

        [Fact]
        public void Should_set_LastHandledResult_and_LastException_to_default_on_circuit_reset()
        {
            CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                .Handle<DivideByZeroException>()
                .OrResult(ResultPrimitive.Fault)
                .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            breaker.Invoking(b => b.RaiseResultAndOrExceptionSequence(new DivideByZeroException()))
                .ShouldThrow<DivideByZeroException>();

            breaker.RaiseResultSequence(ResultPrimitive.Fault)
                .Should().Be(ResultPrimitive.Fault);

            breaker.CircuitState.Should().Be(CircuitState.Open);

            breaker.LastHandledResult.Should().Be(ResultPrimitive.Fault);
            breaker.LastException.Should().BeNull();

            breaker.Reset();

            breaker.LastHandledResult.Should().Be(default(ResultPrimitive));
            breaker.LastException.Should().BeNull();
        }

        #endregion


        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}