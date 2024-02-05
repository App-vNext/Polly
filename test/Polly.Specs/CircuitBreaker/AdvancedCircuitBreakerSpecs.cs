using Scenario = Polly.Specs.Helpers.PolicyExtensions.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.CircuitBreaker;

[Collection(Constants.SystemClockDependentTestCollection)]
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
            .Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Should_throw_if_failure_threshold_is_zero()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

        action.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should()
            .Be("failureThreshold");
    }

    [Fact]
    public void Should_throw_if_failure_threshold_is_less_than_zero()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(-0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

        action.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should()
            .Be("failureThreshold");
    }

    [Fact]
    public void Should_be_able_to_handle_a_failure_threshold_of_one()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(1.0, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

        action.Should().NotThrow();
    }

    [Fact]
    public void Should_throw_if_failure_threshold_is_greater_than_one()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(1.01, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

        action.Should().Throw<ArgumentOutOfRangeException>()
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

        action.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should()
            .Be("samplingDuration");
    }

    [Fact]
    public void Should_not_throw_if_timeslice_duration_is_resolution_of_circuit()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromMilliseconds(20), 4, TimeSpan.FromSeconds(30));

        action.Should().NotThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Should_throw_if_minimum_throughput_is_one()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 1, TimeSpan.FromSeconds(30));

        action.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should()
            .Be("minimumThroughput");
    }

    [Fact]
    public void Should_throw_if_minimum_throughput_is_less_than_one()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 0, TimeSpan.FromSeconds(30));

        action.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should()
            .Be("minimumThroughput");
    }

    [Fact]
    public void Should_throw_if_duration_of_break_is_less_than_zero()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, -TimeSpan.FromSeconds(1));

        action.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should()
            .Be("durationOfBreak");
    }

    [Fact]
    public void Should_be_able_to_handle_a_duration_of_break_of_zero()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.Zero);

        action.Should().NotThrow();
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
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Three of three actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Failure threshold exceeded, but throughput threshold not yet.

        // Throughput threshold will be exceeded by the below successful call, but we never break on a successful call; hence don't break on this.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
    }

    [Fact]
    public void Should_not_open_circuit_if_exceptions_raised_are_not_one_of_the_specified_exceptions()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentOutOfRangeException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw unhandled failures.
        breaker.Invoking(x => x.RaiseException<ArgumentNullException>())
            .Should().Throw<ArgumentNullException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

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

    #region With sample duration higher than 199 ms so that multiple windows are used

    // Tests on the AdvancedCircuitBreaker operation typically use a breaker:
    // - with a failure threshold of >=50%,
    // - and a throughput threshold of 4
    // - across a ten-second period.
    // These provide easy values for testing for failure and throughput thresholds each being met and non-met, in combination.

    [Fact]
    public void Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_if_failure_threshold_exceeded_and_throughput_threshold_equalled_within_timeslice_in_same_window()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

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

    [Fact]
    public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_and_throughput_threshold_equalled_within_timeslice_in_different_windows()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Placing the rest of the invocations ('samplingDuration' / 2) + 1 seconds later
        // ensures that even if there are only two windows, then the invocations are placed in the second.
        // They are still placed within same timeslice.
        SystemClock.UtcNow = () => time.AddSeconds((samplingDuration.Seconds / 2d) + 1);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
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
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Three of four actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
            .WithMessage("The circuit is now open and is not allowing calls.")
            .WithInnerException<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

    }

    [Fact]
    public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_though_not_all_are_failures_and_throughput_threshold_equalled_within_timeslice_in_different_windows()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Three of four actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // Placing the rest of the invocations ('samplingDuration' / 2) + 1 seconds later
        // ensures that even if there are only two windows, then the invocations are placed in the second.
        // They are still placed within same timeslice
        SystemClock.UtcNow = () => time.AddSeconds((samplingDuration.Seconds / 2d) + 1);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
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
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Two of four actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
            .WithMessage("The circuit is now open and is not allowing calls.")
            .WithInnerException<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

    }

    [Fact]
    public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_within_timeslice_in_different_windows()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Two of four actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // Placing the rest of the invocations ('samplingDuration' / 2) + 1 seconds later
        // ensures that even if there are only two windows, then the invocations are placed in the second.
        // They are still placed within same timeslice
        SystemClock.UtcNow = () => time.AddSeconds((samplingDuration.Seconds / 2d) + 1);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
            .WithMessage("The circuit is now open and is not allowing calls.")
            .WithInnerException<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

    }

    [Fact]
    public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures; but only the first three within the timeslice.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (clearly) expires; fourth exception thrown in next-recorded timeslice.
        SystemClock.UtcNow = () => time.Add(samplingDuration).Add(samplingDuration);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

    }

    [Fact]
    public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_even_if_timeslice_expires_only_exactly()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures; but only the first three within the timeslice.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (just) expires; fourth exception thrown in following timeslice.
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

    }

    [Fact]
    public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_even_if_error_occurring_just_at_the_end_of_the_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures; but only the first three within the original timeslice.

        // Two actions at the start of the original timeslice.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Creates a new window right at the end of the original timeslice.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (just) expires; fourth exception thrown in following timeslice.  If timeslice/window rollover is precisely defined, this should cause first two actions to be forgotten from statistics (rolled out of the window of relevance), and thus the circuit not to break.
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

    }

    [Fact]
    public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_even_if_only_just_within_timeslice()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Adjust SystemClock so that timeslice doesn't quite expire; fourth exception thrown in same timeslice.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
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
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // One of three actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
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
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // One of four actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
    }

    [Fact]
    public void Should_open_circuit_if_failures_at_end_of_last_timeslice_below_failure_threshold_and_failures_in_beginning_of_new_timeslice_where_total_equals_failure_threshold()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Executing a single invocation to ensure timeslice is created
        // This invocation is not be counted against the threshold
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // The time is set to just at the end of the sampling duration ensuring
        // the invocations are within the timeslice, but only barely.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        // Three of four actions in this test occur within the first timeslice.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Setting the time to just barely into the new timeslice
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        // This failure opens the circuit, because it is the second failure of four calls
        // equalling the failure threshold. The minimum threshold within the defined
        // sampling duration is met, when using rolling windows.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
    }

    [Fact]
    public void Should_not_open_circuit_if_failures_at_end_of_last_timeslice_and_failures_in_beginning_of_new_timeslice_when_below_minimum_throughput_threshold()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Executing a single invocation to ensure timeslice is created
        // This invocation is not be counted against the threshold
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // The time is set to just at the end of the sampling duration ensuring
        // the invocations are within the timeslice, but only barely.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        // Two of three actions in this test occur within the first timeslice.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Setting the time to just barely into the new timeslice
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        // A third failure occurs just at the beginning of the new timeslice making
        // the number of failures above the failure threshold. However, the throughput is
        // below the minimum threshold as to open the circuit.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void Should_open_circuit_if_failures_in_second_window_of_last_timeslice_and_failures_in_first_window_in_next_timeslice_exceeds_failure_threshold_and_minimum_threshold()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);
        var numberOfWindowsDefinedInCircuitBreaker = 10;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Executing a single invocation to ensure timeslice is created
        // This invocation is not be counted against the threshold
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Setting the time to the second window in the rolling metrics
        SystemClock.UtcNow = () => time.AddSeconds(samplingDuration.Seconds / (double)numberOfWindowsDefinedInCircuitBreaker);

        // Three actions occur in the second window of the first timeslice
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Setting the time to just barely into the new timeslice
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
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
    public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_and_throughput_threshold_equalled_within_timeslice_low_sampling_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
            .WithMessage("The circuit is now open and is not allowing calls.")
            .WithInnerException<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

    }

    [Fact]
    public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_though_not_all_are_failures_and_throughput_threshold_equalled_within_timeslice_low_sampling_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Three of four actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
            .WithMessage("The circuit is now open and is not allowing calls.")
            .WithInnerException<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

    }

    [Fact]
    public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_within_timeslice_low_sampling_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Two of four actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
            .WithMessage("The circuit is now open and is not allowing calls.")
            .WithInnerException<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

    }

    [Fact]
    public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_low_sampling_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromMilliseconds(199);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures; but only the first within the timeslice.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (clearly) expires; fourth exception thrown in next-recorded timeslice.
        SystemClock.UtcNow = () => time.Add(samplingDuration).Add(samplingDuration);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

    }

    [Fact]
    public void Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_even_if_timeslice_expires_only_exactly_low_sampling_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromMilliseconds(199);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Two of four actions in this test throw handled failures; but only the first within the timeslice.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (just) expires; fourth exception thrown in following timeslice.
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

    }

    [Fact]
    public void Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_even_if_only_just_within_timeslice_low_sampling_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromMilliseconds(199);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Adjust SystemClock so that timeslice doesn't quite expire; fourth exception thrown in same timeslice.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
            .WithMessage("The circuit is now open and is not allowing calls.")
            .WithInnerException<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

    }

    [Fact]
    public void Should_not_open_circuit_if_failure_threshold_not_met_and_throughput_threshold_not_met_low_sampling_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // One of three actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
    }

    [Fact]
    public void Should_not_open_circuit_if_failure_threshold_not_met_but_throughput_threshold_met_before_timeslice_expires_low_sampling_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // One of four actions in this test throw handled failures.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
    }

    [Fact]
    public void Should_not_open_circuit_if_failures_at_end_of_last_timeslice_below_failure_threshold_and_failures_in_beginning_of_new_timeslice_where_total_equals_failure_threshold_low_sampling_duration()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromMilliseconds(199);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Executing a single invocation to ensure timeslice is created
        // This invocation is not be counted against the threshold
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // The time is set to just at the end of the sampling duration ensuring
        // the invocations are within the timeslice, but only barely.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        // Three of four actions in this test occur within the first timeslice.
        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.Execute(() => { }))
            .Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Setting the time to just barely into the new timeslice
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        // This failure does not open the circuit, because a new duration should have
        // started and with such low sampling duration, windows should not be used.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
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
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Should_halfopen_circuit_after_the_specified_duration_has_passed_with_failures_in_different_windows()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);
        var samplingDuration = TimeSpan.FromSeconds(10);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // Placing the rest of the invocations ('samplingDuration' / 2) + 1 seconds later
        // ensures that even if there are only two windows, then the invocations are placed in the second.
        // They are still placed within same timeslice
        var anotherWindowDuration = (samplingDuration.Seconds / 2d) + 1;
        SystemClock.UtcNow = () => time.AddSeconds(anotherWindowDuration);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // Since the call that opened the circuit occurred in a later window, then the
        // break duration must be simulated as from that call.
        SystemClock.UtcNow = () => time.Add(durationOfBreak).AddSeconds(anotherWindowDuration);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
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
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // first call after duration raises an exception, so circuit should open again
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

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
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // first call after duration is successful, so circuit should reset
        breaker.Execute(() => { });
        breaker.CircuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void Should_only_allow_single_execution_on_first_entering_halfopen_state__test_execution_permit_directly()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: durationOfBreak);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
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

    [Fact]
    public void Should_allow_single_execution_per_break_duration_in_halfopen_state__test_execution_permit_directly()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: durationOfBreak);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
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

        // Allow another time window to pass (breaker should still be HalfOpen).
        SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // OnActionPreExecute() should now permit another trial execution.
        breaker._breakerController.Invoking(c => c.OnActionPreExecute()).Should().NotThrow();
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public void Should_only_allow_single_execution_on_first_entering_halfopen_state__integration_test()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: durationOfBreak);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        // exceptions raised, circuit is now open.
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // Start one execution during the HalfOpen state, and request a second execution before the first has completed (ie still during the HalfOpen state).
        // The second execution should be rejected due to the halfopen state.

        TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
        using ManualResetEvent permitSecondExecutionAttempt = new ManualResetEvent(false);
        using ManualResetEvent permitFirstExecutionEnd = new ManualResetEvent(false);
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

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        Task.WaitAll([firstExecution, secondExecution], testTimeoutToExposeDeadlocks).Should().BeTrue();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        if (firstExecution.IsFaulted)
            throw firstExecution!.Exception!;
        if (secondExecution.IsFaulted)
            throw secondExecution!.Exception!;
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

    [Fact]
    public void Should_allow_single_execution_per_break_duration_in_halfopen_state__integration_test()
    {
        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: durationOfBreak);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
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
        using ManualResetEvent permitSecondExecutionAttempt = new ManualResetEvent(false);
        using ManualResetEvent permitFirstExecutionEnd = new ManualResetEvent(false);
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

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        Task.WaitAll([firstExecution, secondExecution], testTimeoutToExposeDeadlocks).Should().BeTrue();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        if (firstExecution.IsFaulted)
            throw firstExecution!.Exception!;
        if (secondExecution.IsFaulted)
            throw secondExecution!.Exception!;
        firstExecution.Status.Should().Be(TaskStatus.RanToCompletion);
        secondExecution.Status.Should().Be(TaskStatus.RanToCompletion);

        // Assert:
        // - First execution should have been permitted and executed under a HalfOpen state
        // - Second overlapping execution in half-open state should have been permitted, one breakDuration later.
        firstDelegateExecutedInHalfOpenState.Should().BeTrue();
        secondDelegateExecutedInHalfOpenState.Should().BeTrue();
        secondDelegateRejectedInHalfOpenState.Should().BeFalse();
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
        bool delegateExecutedWhenBroken = false;
        breaker.Invoking(x => x.Execute(() => delegateExecutedWhenBroken = true))
            .Should().Throw<IsolatedCircuitException>();
        breaker.CircuitState.Should().Be(CircuitState.Isolated);
        breaker.LastException.Should().BeOfType<IsolatedCircuitException>();
        delegateExecutedWhenBroken.Should().BeFalse();
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

        bool delegateExecutedWhenBroken = false;
        breaker.Invoking(x => x.Execute(() => delegateExecutedWhenBroken = true))
            .Should().Throw<IsolatedCircuitException>();
        delegateExecutedWhenBroken.Should().BeFalse();
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
            .Should().Throw<IsolatedCircuitException>();

        breaker.Reset();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        breaker.Invoking(x => x.Execute(() => { })).Should().NotThrow();
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
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        // reset circuit, with no time having passed
        breaker.Reset();
        SystemClock.UtcNow().Should().Be(time);
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        breaker.Invoking(x => x.Execute(() => { })).Should().NotThrow();
    }

    #endregion

    #region State-change delegate tests

    [Fact]
    public void Should_not_call_onreset_on_initialise()
    {
        Action<Exception, TimeSpan> onBreak = (_, _) => { };
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
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled = true; };
        Action onReset = () => { };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        onBreakCalled.Should().BeTrue();
    }

    [Fact]
    public void Should_call_onbreak_when_breaking_circuit_manually()
    {
        bool onBreakCalled = false;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled = true; };
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
    public void Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_placed_through_open_circuit()
    {
        int onBreakCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

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

    [Fact]
    public void Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_call_failure_which_arrives_on_open_state_though_started_on_closed_state()
    {
        int onBreakCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { };

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Start an execution when the breaker is in the closed state, but hold it from returning (its failure) until the breaker has opened.  This call, a failure hitting an already open breaker, should indicate its fail, but should not cause onBreak() to be called a second time.
        TimeSpan testTimeoutToExposeDeadlocks = TimeSpan.FromSeconds(5);
        using ManualResetEvent permitLongRunningExecutionToReturnItsFailure = new ManualResetEvent(false);
        using ManualResetEvent permitMainThreadToOpenCircuit = new ManualResetEvent(false);
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
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().Be(1);

        // Permit the second (long-running) execution to hit the open circuit with its failure.
        permitLongRunningExecutionToReturnItsFailure.Set();

        // Graceful cleanup: allow executions time to end naturally; timeout if any deadlocks; expose any execution faults.  This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        longRunningExecution.Wait(testTimeoutToExposeDeadlocks).Should().BeTrue();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        if (longRunningExecution.IsFaulted)
        {
            throw longRunningExecution!.Exception!;
        }

        longRunningExecution.Status.Should().Be(TaskStatus.RanToCompletion);

        // onBreak() should still only have been called once.
        breaker.CircuitState.Should().Be(CircuitState.Open);
        onBreakCalled.Should().Be(1);
    }

    [Fact]
    public void Should_call_onreset_when_automatically_closing_circuit_but_not_when_halfopen()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { onResetCalled++; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
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
        Action<Exception, TimeSpan> onBreak = (_, _) => { };
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
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { onResetCalled++; };
        Action onHalfOpen = () => { onHalfOpenCalled++; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset,
                onHalfOpen: onHalfOpen);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
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
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { onResetCalled++; };
        Action onHalfOpen = () => { onHalfOpenCalled++; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset,
                onHalfOpen: onHalfOpen);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        onBreakCalled.Should().Be(0);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        onBreakCalled.Should().Be(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);
        onHalfOpenCalled.Should().Be(1);
        onResetCalled.Should().Be(0);
    }

    [Fact]
    public void Should_call_onreset_when_manually_resetting_circuit()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
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
            .Should().Throw<IsolatedCircuitException>();

        onResetCalled.Should().Be(0);
        breaker.Reset();
        onResetCalled.Should().Be(1);

        breaker.CircuitState.Should().Be(CircuitState.Closed);
        breaker.Invoking(x => x.Execute(() => { })).Should().NotThrow();
    }

    #region Tests of supplied parameters to onBreak delegate

    [Fact]
    public void Should_call_onbreak_with_the_last_raised_exception()
    {
        Exception? passedException = null;

        Action<Exception, TimeSpan, Context> onBreak = (exception, _, _) => { passedException = exception; };
        Action<Context> onReset = _ => { };

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        passedException?.Should().BeOfType<DivideByZeroException>();
    }

    [Fact]
    public void Should_call_onbreak_with_a_state_of_closed()
    {
        CircuitState? transitionedState = null;

        Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, _, _) => { transitionedState = state; };
        Action<Context> onReset = _ => { };
        Action onHalfOpen = () => { };

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset,
                onHalfOpen: onHalfOpen);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        transitionedState?.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void Should_call_onbreak_with_a_state_of_half_open()
    {
        List<CircuitState> transitionedStates = [];

        Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, _, _) => { transitionedStates.Add(state); };
        Action<Context> onReset = _ => { };
        Action onHalfOpen = () => { };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset,
                onHalfOpen: onHalfOpen);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // first call after duration raises an exception, so circuit should open again
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        transitionedStates[0].Should().Be(CircuitState.Closed);
        transitionedStates[1].Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public void Should_call_onbreak_with_the_correct_timespan()
    {
        TimeSpan? passedBreakTimespan = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { passedBreakTimespan = timespan; };
        Action<Context> onReset = _ => { };

        TimeSpan durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        passedBreakTimespan.Should().Be(durationOfBreak);
    }

    [Fact]
    public void Should_open_circuit_with_timespan_maxvalue_if_manual_override_open()
    {
        TimeSpan? passedBreakTimespan = null;
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { passedBreakTimespan = timespan; };
        Action<Context> onReset = _ => { };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);
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
        IDictionary<string, object>? contextData = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) => { contextData = context; };
        Action<Context> onReset = _ => { };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>(
            new { key1 = "value1", key2 = "value2" }.AsDictionary())).Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Should_call_onreset_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, _, _) => { };
        Action<Context> onReset = context => { contextData = context; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // first call after duration should invoke onReset, with context
        breaker.Execute(_ => { }, new { key1 = "value1", key2 = "value2" }.AsDictionary());
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        IDictionary<string, object> contextData = new { key1 = "value1", key2 = "value2" }.AsDictionary();

        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) => { contextData = context; };
        Action<Context> onReset = _ => { };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        breaker.CircuitState.Should().Be(CircuitState.Open);

        contextData.Should().BeEmpty();
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        Action<Exception, TimeSpan, Context> onBreak =
            (_, _, context) => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };
        Action<Context> onReset =
            context => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };

        var time = 1.January(2000);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>(new { key = "original_value" }.AsDictionary()))
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);
        contextValue.Should().Be("original_value");

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.Should().Be(CircuitState.HalfOpen);

        // but not yet reset

        // first call after duration is successful, so circuit should reset
        breaker.Execute(_ => { }, new { key = "new_value" }.AsDictionary());
        breaker.CircuitState.Should().Be(CircuitState.Closed);
        contextValue.Should().Be("new_value");
    }

    #endregion

    #endregion

    #region LastException property

    [Fact]
    public void Should_initialise_LastException_to_null_on_creation()
    {
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(30));

        breaker.LastException.Should().BeNull();
    }

    [Fact]
    public void Should_set_LastException_on_handling_exception_even_when_not_breaking()
    {
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(30));

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.LastException.Should().BeOfType<DivideByZeroException>();
    }

    [Fact]
    public void Should_set_LastException_to_last_raised_exception_when_breaking()
    {
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(30));

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        breaker.LastException.Should().BeOfType<DivideByZeroException>();
    }

    [Fact]
    public void Should_set_LastException_to_null_on_circuit_reset()
    {
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(30));

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Closed);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();
        breaker.CircuitState.Should().Be(CircuitState.Open);

        breaker.LastException.Should().BeOfType<DivideByZeroException>();

        breaker.Reset();

        breaker.LastException.Should().BeNull();
    }

    #endregion

    #region Cancellation support

    [Fact]
    public void Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().NotThrow();
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(0);
    }

    [Fact]
    public void Should_report_cancellation_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_report_cancellation_during_faulting_action_execution_when_user_delegate_observes_cancellationToken()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_report_faulting_from_faulting_action_execution_when_user_delegate_does_not_observe_cancellation()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<DivideByZeroException>();
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_report_cancellation_when_both_open_circuit_and_cancellation()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 2, durationOfBreak);

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<DivideByZeroException>();

        breaker.Invoking(x => x.RaiseException<DivideByZeroException>())
            .Should().Throw<BrokenCircuitException>()
            .WithMessage("The circuit is now open and is not allowing calls.")
            .WithInnerException<DivideByZeroException>();

        // Circuit is now broken.

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = null, // Cancelled manually instead - see above.
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            breaker.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(0);
    }

    [Fact]
    public void Should_honour_different_cancellationToken_captured_implicitly_by_action()
    {
        // Before CancellationToken support was built in to Polly, users of the library may have implicitly captured a CancellationToken and used it to cancel actions.  For backwards compatibility, Polly should not confuse these with its own CancellationToken; it should distinguish OperationCanceledExceptions thrown with different CancellationTokens.

        var durationOfBreak = TimeSpan.FromMinutes(1);
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;

        using (var policyCancellationTokenSource = new CancellationTokenSource())
        using (var implicitlyCapturedActionCancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken policyCancellationToken = policyCancellationTokenSource.Token;
            CancellationToken implicitlyCapturedActionCancellationToken = implicitlyCapturedActionCancellationTokenSource.Token;

            implicitlyCapturedActionCancellationTokenSource.Cancel();

            breaker.Invoking(x => x.Execute(_ =>
            {
                attemptsInvoked++;
                implicitlyCapturedActionCancellationToken.ThrowIfCancellationRequested();
            }, policyCancellationToken))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(implicitlyCapturedActionCancellationToken);
        }

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_execute_func_returning_value_when_cancellationToken_not_cancelled()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;
        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            breaker.Invoking(x => result = x.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
                .Should().NotThrow();
        }

        result.Should().BeTrue();

        attemptsInvoked.Should().Be(1);
    }

    [Fact]
    public void Should_honour_and_report_cancellation_during_func_execution()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        CircuitBreakerPolicy breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        bool? result = null;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            breaker.Invoking(x => result = x.RaiseExceptionAndOrCancellation<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true))
                .Should().Throw<OperationCanceledException>().And.CancellationToken.Should().Be(cancellationToken);
        }

        result.Should().Be(null);

        attemptsInvoked.Should().Be(1);
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
