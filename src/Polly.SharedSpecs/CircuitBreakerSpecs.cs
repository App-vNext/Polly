﻿using System;
using FluentAssertions;
using Polly.CircuitBreaker;
using Polly.Specs.Helpers;
using Polly.Utilities;
using Xunit;

namespace Polly.Specs
{
    public class CircuitBreakerSpecs : IDisposable
    {
        [Fact]
        public void Should_be_able_to_handle_a_duration_of_timespan_maxvalue()
        {
            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker(1, TimeSpan.MaxValue);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_be_able_to_handle_a_duration_of_timespan_maxvalue_async()
        {
            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(1, TimeSpan.MaxValue);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_throw_if_exceptions_allowed_before_breaking_is_less_than_one()
        {
           Action action = () => Policy
                                    .Handle<DivideByZeroException>()
                                    .CircuitBreaker(0, new TimeSpan());

            action.ShouldThrow<ArgumentOutOfRangeException>()
                  .And.ParamName.Should()
                  .Be("exceptionsAllowedBeforeBreaking");
        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_after_specified_number_of_specified_exception_have_been_raised()
        {
            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>()
                  .WithMessage("The circuit is now open and is not allowing calls.")
                  .WithInnerException<DivideByZeroException>();
        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_after_specified_number_of_specified_exception_have_been_raised_async()
        {
            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>()
                  .WithMessage("The circuit is now open and is not allowing calls.")
                  .WithInnerException<DivideByZeroException>();
        }

        [Fact]
        public void Should_open_circuit_with_the_last_raised_exception_after_specified_number_of_one_of_the_specified_exceptions_have_been_raised()
        {
            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .Or<ArgumentOutOfRangeException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<ArgumentOutOfRangeException>())
                  .ShouldThrow<ArgumentOutOfRangeException>();

            // 2 exception raised, cicuit is now open
            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>()
                  .WithMessage("The circuit is now open and is not allowing calls.")
                  .WithInnerException<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Should_not_open_circuit_if_exception_raised_is_not_the_specified_exception()
        {
            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            policy.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();

            policy.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();

            policy.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Should_not_open_circuit_if_exception_raised_is_not_one_of_the_the_specified_exceptions()
        {
            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .Or<ArgumentOutOfRangeException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

            policy.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();

            policy.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();

            policy.Invoking(x => x.RaiseException<ArgumentNullException>())
                  .ShouldThrow<ArgumentNullException>();
        }


        [Fact]
        public void Should_reset_the_policy_when_reset_is_called()
        {
            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker(2, TimeSpan.FromMinutes(2));

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();

            policy.Reset();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
        }

        [Fact]
        public void Should_close_circuit_after_the_specified_duration_has_passed()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker(2, durationOfBreak);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            // 2 exception raised, cicuit is now open
            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_close_circuit_after_the_specified_duration_has_passed_async()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreakerAsync(2, durationOfBreak);

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            // 2 exception raised, cicuit is now open
            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // duration has passed, circuit now half open
            policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker(2, durationOfBreak);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            // 2 exception raised, cicuit is now open
            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // fist call after duration raises an exception, so circuit should break again
            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
        }


        [Fact]
        public void Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_raise_an_exception()
        {
            var time = 1.January(2000);
            SystemClock.UtcNow = () => time;

            var durationOfBreak = TimeSpan.FromMinutes(1);

            var policy = Policy
                            .Handle<DivideByZeroException>()
                            .CircuitBreaker(2, durationOfBreak);

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            // 2 exception raised, cicuit is now open
            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();

            SystemClock.UtcNow = () => time.Add(durationOfBreak);

            // fist call after duration is successful, so circuit should reset
            policy.Execute(() => {});

            // circuit has been reset so should once again allow 2 exceptions to be raised before breaking
            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<DivideByZeroException>();

            policy.Invoking(x => x.RaiseException<DivideByZeroException>())
                  .ShouldThrow<BrokenCircuitException>();
        }

        public void Dispose()
        {
            SystemClock.Reset();
        }
    }
}