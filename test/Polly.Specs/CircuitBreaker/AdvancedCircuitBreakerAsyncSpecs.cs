using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.CircuitBreaker;

[Collection(Constants.SystemClockDependentTestCollection)]
public class AdvancedCircuitBreakerAsyncSpecs : IDisposable
{
    #region Configuration tests

    [Fact]
    public async Task Should_be_able_to_handle_a_duration_of_timespan_maxvalue()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.MaxValue);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public void Should_throw_if_failure_threshold_is_zero()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

        Should.Throw<ArgumentOutOfRangeException>(action)
            .ParamName.ShouldBe("failureThreshold");
    }

    [Fact]
    public void Should_throw_if_failure_threshold_is_less_than_zero()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(-0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

        Should.Throw<ArgumentOutOfRangeException>(action)
            .ParamName.ShouldBe("failureThreshold");
    }

    [Fact]
    public void Should_be_able_to_handle_a_failure_threshold_of_one()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(1.0, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

        Should.NotThrow(action);
    }

    [Fact]
    public void Should_throw_if_failure_threshold_is_greater_than_one()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(1.01, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

        Should.Throw<ArgumentOutOfRangeException>(action)
            .ParamName.ShouldBe("failureThreshold");
    }

    [Fact]
    public void Should_throw_if_timeslice_duration_is_less_than_resolution_of_circuit()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                0.5,
                TimeSpan.FromMilliseconds(20).Add(TimeSpan.FromTicks(-1)),
                4,
                TimeSpan.FromSeconds(30));

        Should.Throw<ArgumentOutOfRangeException>(action)
            .ParamName.ShouldBe("samplingDuration");
    }

    [Fact]
    public void Should_not_throw_if_timeslice_duration_is_resolution_of_circuit()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromMilliseconds(20), 4, TimeSpan.FromSeconds(30));

        Should.NotThrow(action);
    }

    [Fact]
    public void Should_throw_if_minimum_throughput_is_one()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 1, TimeSpan.FromSeconds(30));

        Should.Throw<ArgumentOutOfRangeException>(action)
            .ParamName.ShouldBe("minimumThroughput");
    }

    [Fact]
    public void Should_throw_if_minimum_throughput_is_less_than_one()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 0, TimeSpan.FromSeconds(30));

        Should.Throw<ArgumentOutOfRangeException>(action)
            .ParamName.ShouldBe("minimumThroughput");
    }

    [Fact]
    public void Should_throw_if_duration_of_break_is_less_than_zero()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, -TimeSpan.FromSeconds(1));

        Should.Throw<ArgumentOutOfRangeException>(action)
            .ParamName.ShouldBe("durationOfBreak");
    }

    [Fact]
    public void Should_be_able_to_handle_a_duration_of_break_of_zero()
    {
        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.Zero);

        Should.NotThrow(action);
    }

    [Fact]
    public void Should_initialise_to_closed_state()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    #endregion

    #region Circuit-breaker threshold-to-break tests

    #region Tests that are independent from health metrics implementation

    // Tests on the AdvancedCircuitBreakerAsync operation typically use a breaker:
    // - with a failure threshold of >=50%,
    // - and a throughput threshold of 4
    // - across a ten-second period.
    // These provide easy values for testing for failure and throughput thresholds each being met and non-met, in combination.

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_and_minimum_threshold_is_equalled_but_last_call_is_success()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Three of three actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Failure threshold exceeded, but throughput threshold not yet.

        // Throughput threshold will be exceeded by the below successful call, but we never break on a successful call; hence don't break on this.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
    }

    [Fact]
    public async Task Should_not_open_circuit_if_exceptions_raised_are_not_one_of_the_specified_exceptions()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentOutOfRangeException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw unhandled failures.
        for (int i = 0; i < 4; i++)
        {
            await Should.ThrowAsync<ArgumentNullException>(() => breaker.RaiseExceptionAsync<ArgumentNullException>());
            breaker.CircuitState.ShouldBe(CircuitState.Closed);
        }
    }

    #endregion

    #region With sample duration higher than 199 ms so that multiple windows are used

    // Tests on the AdvancedCircuitBreakerAsync operation typically use a breaker:
    // - with a failure threshold of >=50%,
    // - and a throughput threshold of 4
    // - across a ten-second period.
    // These provide easy values for testing for failure and throughput thresholds each being met and non-met, in combination.

    [Fact]
    public async Task Should_open_circuit_blocking_executions_and_noting_the_last_raised_exception_if_failure_threshold_exceeded_and_throughput_threshold_equalled_within_timeslice_in_same_window()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        bool delegateExecutedWhenBroken = false;
        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }));
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        delegateExecutedWhenBroken.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_and_throughput_threshold_equalled_within_timeslice_in_different_windows()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Placing the rest of the invocations ('samplingDuration' / 2) + 1 seconds later
        // ensures that even if there are only two windows, then the invocations are placed in the second.
        // They are still placed within same timeslice.
        SystemClock.UtcNow = () => time.AddSeconds((samplingDuration.Seconds / 2d) + 1);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_though_not_all_are_failures_and_throughput_threshold_equalled_within_timeslice_in_same_window()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Three of four actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_though_not_all_are_failures_and_throughput_threshold_equalled_within_timeslice_in_different_windows()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Three of four actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // Placing the rest of the invocations ('samplingDuration' / 2) + 1 seconds later
        // ensures that even if there are only two windows, then the invocations are placed in the second.
        // They are still placed within same timeslice
        SystemClock.UtcNow = () => time.AddSeconds((samplingDuration.Seconds / 2d) + 1);

        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_within_timeslice_in_same_window()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Two of four actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_within_timeslice_in_different_windows()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Two of four actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // Placing the rest of the invocations ('samplingDuration' / 2) + 1 seconds later
        // ensures that even if there are only two windows, then the invocations are placed in the second.
        // They are still placed within same timeslice
        SystemClock.UtcNow = () => time.AddSeconds((samplingDuration.Seconds / 2d) + 1);

        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures; but only the first three within the timeslice.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (clearly) expires; fourth exception thrown in next-recorded timeslice.
        SystemClock.UtcNow = () => time.Add(samplingDuration).Add(samplingDuration);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_even_if_timeslice_expires_only_exactly()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures; but only the first three within the timeslice.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (just) expires; fourth exception thrown in following timeslice.
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_even_if_error_occurring_just_at_the_end_of_the_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures; but only the first three within the original timeslice.

        // Two actions at the start of the original timeslice.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Creates a new window right at the end of the original timeslice.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (just) expires; fourth exception thrown in following timeslice.  If timeslice/window rollover is precisely defined, this should cause first two actions to be forgotten from statistics (rolled out of the window of relevance), and thus the circuit not to break.
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_even_if_only_just_within_timeslice()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Adjust SystemClock so that timeslice doesn't quite expire; fourth exception thrown in same timeslice.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_not_met_and_throughput_threshold_not_met()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // One of three actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_not_met_but_throughput_threshold_met_before_timeslice_expires()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // One of four actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_open_circuit_if_failures_at_end_of_last_timeslice_below_failure_threshold_and_failures_in_beginning_of_new_timeslice_where_total_equals_failure_threshold()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Executing a single invocation to ensure timeslice is created
        // This invocation is not be counted against the threshold
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // The time is set to just at the end of the sampling duration ensuring
        // the invocations are within the timeslice, but only barely.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        // Three of four actions in this test occur within the first timeslice.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Setting the time to just barely into the new timeslice
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        // This failure opens the circuit, because it is the second failure of four calls
        // equalling the failure threshold. The minimum threshold within the defined
        // sampling duration is met, when using rolling windows.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failures_at_end_of_last_timeslice_and_failures_in_beginning_of_new_timeslice_when_below_minimum_throughput_threshold()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Executing a single invocation to ensure timeslice is created
        // This invocation is not be counted against the threshold
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // The time is set to just at the end of the sampling duration ensuring
        // the invocations are within the timeslice, but only barely.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        // Two of three actions in this test occur within the first timeslice.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Setting the time to just barely into the new timeslice
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        // A third failure occurs just at the beginning of the new timeslice making
        // the number of failures above the failure threshold. However, the throughput is
        // below the minimum threshold as to open the circuit.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_open_circuit_if_failures_in_second_window_of_last_timeslice_and_failures_in_first_window_in_next_timeslice_exceeds_failure_threshold_and_minimum_threshold()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromSeconds(10);
        var numberOfWindowsDefinedInCircuitBreaker = 10;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Executing a single invocation to ensure timeslice is created
        // This invocation is not be counted against the threshold
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Setting the time to the second window in the rolling metrics
        SystemClock.UtcNow = () => time.AddSeconds(samplingDuration.Seconds / (double)numberOfWindowsDefinedInCircuitBreaker);

        // Three actions occur in the second window of the first timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Setting the time to just barely into the new timeslice
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    #endregion

    #region With sample duration at 199 ms so that only a single window is used

    // These tests on AdvancedCircuitBreakerAsync operation typically use a breaker:
    // - with a failure threshold of >=50%,
    // - and a throughput threshold of 4
    // - across a 199ms period.
    // These provide easy values for testing for failure and throughput thresholds each being met and non-met, in combination.

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_and_throughput_threshold_equalled_within_timeslice_low_sampling_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_exceeded_though_not_all_are_failures_and_throughput_threshold_equalled_within_timeslice_low_sampling_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Three of four actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_within_timeslice_low_sampling_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Two of four actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // No adjustment to SystemClock.UtcNow, so all exceptions were raised within same timeslice
        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_low_sampling_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromMilliseconds(199);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures; but only the first within the timeslice.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (clearly) expires; fourth exception thrown in next-recorded timeslice.
        SystemClock.UtcNow = () => time.Add(samplingDuration).Add(samplingDuration);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_exceeded_but_throughput_threshold_not_met_before_timeslice_expires_even_if_timeslice_expires_only_exactly_low_sampling_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromMilliseconds(199);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Two of four actions in this test throw handled failures; but only the first within the timeslice.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Adjust SystemClock so that timeslice (just) expires; fourth exception thrown in following timeslice.
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_open_circuit_with_the_last_raised_exception_if_failure_threshold_equalled_and_throughput_threshold_equalled_even_if_only_just_within_timeslice_low_sampling_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromMilliseconds(199);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Adjust SystemClock so that timeslice doesn't quite expire; fourth exception thrown in same timeslice.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();
        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_not_met_and_throughput_threshold_not_met_low_sampling_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // One of three actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failure_threshold_not_met_but_throughput_threshold_met_before_timeslice_expires_low_sampling_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(199),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // One of four actions in this test throw handled failures.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_not_open_circuit_if_failures_at_end_of_last_timeslice_below_failure_threshold_and_failures_in_beginning_of_new_timeslice_where_total_equals_failure_threshold_low_sampling_duration()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var samplingDuration = TimeSpan.FromMilliseconds(199);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30));

        // Executing a single invocation to ensure timeslice is created
        // This invocation is not be counted against the threshold
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // The time is set to just at the end of the sampling duration ensuring
        // the invocations are within the timeslice, but only barely.
        SystemClock.UtcNow = () => time.AddTicks(samplingDuration.Ticks - 1);

        // Three of four actions in this test occur within the first timeslice.
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Setting the time to just barely into the new timeslice
        SystemClock.UtcNow = () => time.Add(samplingDuration);

        // This failure does not open the circuit, because a new duration should have
        // started and with such low sampling duration, windows should not be used.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    #endregion

    #endregion

    #region Circuit-breaker open->half-open->open/closed tests

    [Fact]
    public async Task Should_halfopen_circuit_after_the_specified_duration_has_passed_with_failures_in_same_window()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_halfopen_circuit_after_the_specified_duration_has_passed_with_failures_in_different_windows()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);
        var samplingDuration = TimeSpan.FromSeconds(10);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: samplingDuration,
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // Placing the rest of the invocations ('samplingDuration' / 2) + 1 seconds later
        // ensures that even if there are only two windows, then the invocations are placed in the second.
        // They are still placed within same timeslice
        var anotherWindowDuration = (samplingDuration.Seconds / 2d) + 1;
        SystemClock.UtcNow = () => time.AddSeconds(anotherWindowDuration);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // Since the call that opened the circuit occurred in a later window, then the
        // break duration must be simulated as from that call.
        SystemClock.UtcNow = () => time.Add(durationOfBreak).AddSeconds(anotherWindowDuration);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration raises an exception, so circuit should open again
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_reset_circuit_after_the_specified_duration_has_passed_if_the_next_call_does_not_raise_an_exception()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__test_execution_permit_directly()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        // exception raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should permit first execution.
        Should.NotThrow(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should reject a second execution. (tho still in half-open condition).
        Should.Throw<BrokenCircuitException>(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_allow_single_execution_per_break_duration_in_halfopen_state__test_execution_permit_directly()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        // exception raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should permit first execution.
        Should.NotThrow(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should reject a second execution. (tho still in half-open condition).
        Should.Throw<BrokenCircuitException>(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // Allow another time window to pass (breaker should still be HalfOpen).
        SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // OnActionPreExecute() should now permit another trial execution.
        Should.NotThrow(() => breaker.BreakerController.OnActionPreExecute());
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_only_allow_single_execution_on_first_entering_halfopen_state__integration_test()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        // exceptions raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

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
        Task firstExecution = Task.Factory.StartNew(async () =>
        {
            await Should.NotThrowAsync(() => breaker.ExecuteAsync(async () =>
            {
                firstDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                // Signal the second execution can start, overlapping with this (the first) execution.
                firstExecutionActive = true;
                permitSecondExecutionAttempt.Set();

                // Hold first execution open until second indicates it is no longer needed, or time out.
                permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                await TaskHelper.EmptyTask;
                firstExecutionActive = false;

            }));
        }, TaskCreationOptions.LongRunning);

        // Attempt a second execution, signalled by the first execution to ensure they overlap: we should be able to verify it doesn't execute, and is rejected by a breaker in a HalfOpen state.
        permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks);

        Task secondExecution = Task.Factory.StartNew(async () =>
        {
            // Validation of correct sequencing and overlapping of tasks in test (guard against erroneous test refactorings/operation).
            firstExecutionActive.ShouldBeTrue();
            breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

            try
            {
                await breaker.ExecuteAsync(async () =>
                {
                    secondDelegateRejectedInHalfOpenState = false;
                    secondDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.
                    await TaskHelper.EmptyTask;
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
        Task.WaitAll([firstExecution, secondExecution], testTimeoutToExposeDeadlocks).ShouldBeTrue();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        if (firstExecution.IsFaulted)
        {
            throw firstExecution!.Exception!;
        }

        if (secondExecution.IsFaulted)
        {
            throw secondExecution!.Exception!;
        }

        firstExecution.Status.ShouldBe(TaskStatus.RanToCompletion);
        secondExecution.Status.ShouldBe(TaskStatus.RanToCompletion);

        // Assert:
        // - First execution should have been permitted and executed under a HalfOpen state
        // - Second overlapping execution in halfopen state should not have been permitted.
        // - Second execution attempt should have been rejected with HalfOpen state as cause.
        firstDelegateExecutedInHalfOpenState.ShouldNotBeNull();
        firstDelegateExecutedInHalfOpenState.Value.ShouldBeTrue();
        secondDelegateExecutedInHalfOpenState.ShouldNotBeNull();
        secondDelegateExecutedInHalfOpenState.Value.ShouldBeFalse();
        secondDelegateRejectedInHalfOpenState.ShouldNotBeNull();
        secondDelegateRejectedInHalfOpenState.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_allow_single_execution_per_break_duration_in_halfopen_state__integration_test()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        // exception raised, circuit is now open.
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // break duration passes, circuit now half open
        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

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
        Task firstExecution = Task.Factory.StartNew(async () =>
        {
            await Should.NotThrowAsync(() => breaker.ExecuteAsync(async () =>
            {
                firstDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                // Signal the second execution can start, overlapping with this (the first) execution.
                firstExecutionActive = true;
                permitSecondExecutionAttempt.Set();

                // Hold first execution open until second indicates it is no longer needed, or time out.
                permitFirstExecutionEnd.WaitOne(testTimeoutToExposeDeadlocks);
                await TaskHelper.EmptyTask;
                firstExecutionActive = false;
            }));
        }, TaskCreationOptions.LongRunning);

        // Attempt a second execution, signalled by the first execution to ensure they overlap; start it one breakDuration later.  We should be able to verify it does execute, though the breaker is still in a HalfOpen state.
        permitSecondExecutionAttempt.WaitOne(testTimeoutToExposeDeadlocks);

        Task secondExecution = Task.Factory.StartNew(async () =>
        {
            // Validation of correct sequencing and overlapping of tasks in test (guard against erroneous test refactorings/operation).
            firstExecutionActive.ShouldBeTrue();
            breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

            try
            {
                SystemClock.UtcNow = () => time.Add(durationOfBreak).Add(durationOfBreak);

                await breaker.ExecuteAsync(async () =>
                {
                    secondDelegateRejectedInHalfOpenState = false;
                    secondDelegateExecutedInHalfOpenState = breaker.CircuitState == CircuitState.HalfOpen; // For readability of test results, we assert on this at test end rather than nested in Task and breaker here.

                    await TaskHelper.EmptyTask;
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
        Task.WaitAll([firstExecution, secondExecution], testTimeoutToExposeDeadlocks).ShouldBeTrue();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        if (firstExecution.IsFaulted)
        {
            throw firstExecution!.Exception!;
        }

        if (secondExecution.IsFaulted)
        {
            throw secondExecution!.Exception!;
        }

        firstExecution.Status.ShouldBe(TaskStatus.RanToCompletion);
        secondExecution.Status.ShouldBe(TaskStatus.RanToCompletion);

        // Assert:
        // - First execution should have been permitted and executed under a HalfOpen state
        // - Second overlapping execution in halfopen state should have been permitted, one breakDuration later.
        firstDelegateExecutedInHalfOpenState.ShouldNotBeNull();
        firstDelegateExecutedInHalfOpenState.Value.ShouldBeTrue();
        secondDelegateExecutedInHalfOpenState.ShouldNotBeNull();
        secondDelegateExecutedInHalfOpenState.Value.ShouldBeTrue();
        secondDelegateRejectedInHalfOpenState.ShouldNotBeNull();
        secondDelegateRejectedInHalfOpenState.Value.ShouldBeFalse();
    }

    #endregion

    #region Isolate and reset tests

    [Fact]
    public async Task Should_open_circuit_and_block_calls_if_manual_override_open()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, TimeSpan.FromSeconds(30));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // manually break circuit
        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        // circuit manually broken: execution should be blocked; even non-exception-throwing executions should not reset circuit
        bool delegateExecutedWhenBroken = false;
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }));
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);
        breaker.LastException.ShouldBeOfType<IsolatedCircuitException>();
        delegateExecutedWhenBroken.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_hold_circuit_open_despite_elapsed_time_if_manual_override_open()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        bool delegateExecutedWhenBroken = false;
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => { delegateExecutedWhenBroken = true; return TaskHelper.EmptyTask; }));
        delegateExecutedWhenBroken.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_close_circuit_again_on_reset_after_manual_override()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));

        breaker.Reset();
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
    }

    [Fact]
    public async Task Should_be_able_to_reset_automatically_opened_circuit_without_specified_duration_passing()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // reset circuit, with no time having passed
        breaker.Reset();
        SystemClock.UtcNow().ShouldBe(time);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
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
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak, onBreak, onReset);

        onResetCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_automatically()
    {
        bool onBreakCalled = false;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled = true; };
        Action onReset = () => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        onBreakCalled.ShouldBeTrue();
    }

    [Fact]
    public void Should_call_onbreak_when_breaking_circuit_manually()
    {
        bool onBreakCalled = false;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled = true;
        Action onReset = () => { };

        var durationOfBreak = TimeSpan.FromSeconds(30);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak, onBreak, onReset);

        onBreakCalled.ShouldBeFalse();

        breaker.Isolate();

        onBreakCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_calls_placed_through_open_circuit()
    {
        int onBreakCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        // call through circuit when already broken - should not retrigger onBreak
        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);
    }

    [Fact]
    public async Task Should_call_onbreak_when_breaking_circuit_first_time_but_not_for_subsequent_call_failure_which_arrives_on_open_state_though_started_on_closed_state()
    {
        int onBreakCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { };

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
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
        Task longRunningExecution = Task.Factory.StartNew(async () =>
        {
            breaker.CircuitState.ShouldBe(CircuitState.Closed);

            // However, since execution started when circuit was closed, BrokenCircuitException will not have been thrown on entry; the original exception will still be thrown.
            await Should.ThrowAsync<DivideByZeroException>(() => breaker.ExecuteAsync(async () =>
            {
                await TaskHelper.EmptyTask;

                permitMainThreadToOpenCircuit.Set();

                // Hold this execution until rest of the test indicates it can proceed (or timeout, to expose deadlocks).
                permitLongRunningExecutionToReturnItsFailure.WaitOne(testTimeoutToExposeDeadlocks);

                // Throw a further failure when rest of test has already broken the circuit.
                breaker.CircuitState.ShouldBe(CircuitState.Open);
                throw new DivideByZeroException();

            }));
        }, TaskCreationOptions.LongRunning);

        permitMainThreadToOpenCircuit.WaitOne(testTimeoutToExposeDeadlocks).ShouldBeTrue();

        // Break circuit in the normal manner: onBreak() should be called once.
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        // Permit the second (long-running) execution to hit the open circuit with its failure.
        permitLongRunningExecutionToReturnItsFailure.Set();

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        // Graceful cleanup: allow executions time to end naturally; timeout if any deadlocks; expose any execution faults.  This validates the test ran as expected (and background delegates are complete) before we assert on outcomes.
#if NET
        longRunningExecution.Wait(testTimeoutToExposeDeadlocks, CancellationToken.None).ShouldBeTrue();
#else
        longRunningExecution.Wait(testTimeoutToExposeDeadlocks).ShouldBeTrue();
#endif
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        if (longRunningExecution.IsFaulted)
        {
            throw longRunningExecution!.Exception!;
        }

        longRunningExecution.Status.ShouldBe(TaskStatus.RanToCompletion);

        // onBreak() should still only have been called once.
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);
    }

    [Fact]
    public async Task Should_call_onreset_when_automatically_closing_circuit_but_not_when_halfopen()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => { onBreakCalled++; };
        Action onReset = () => { onResetCalled++; };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        onBreakCalled.ShouldBe(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // but not yet reset
        onResetCalled.ShouldBe(0);

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBe(1);
    }

    [Fact]
    public async Task Should_not_call_onreset_on_successive_successful_calls()
    {
        Action<Exception, TimeSpan> onBreak = (_, _) => { };
        bool onResetCalled = false;
        Action onReset = () => { onResetCalled = true; };

        var durationOfBreak = TimeSpan.FromSeconds(30);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak, onBreak, onReset);

        onResetCalled.ShouldBeFalse();

        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBeFalse();

        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_subsequent_execution()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        int onHalfOpenCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;
        Action onHalfOpen = () => onHalfOpenCalled++;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset,
                onHalfOpen: onHalfOpen);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        onBreakCalled.ShouldBe(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        onHalfOpenCalled.ShouldBe(0); // not yet transitioned to half-open, because we have not queried state

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(() => TaskHelper.EmptyTask);
        onHalfOpenCalled.ShouldBe(1); // called as action was placed for execution
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onResetCalled.ShouldBe(1); // called after action succeeded
    }

    [Fact]
    public async Task Should_call_onhalfopen_when_automatically_transitioning_to_halfopen_due_to_state_read()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        int onHalfOpenCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => onResetCalled++;
        Action onHalfOpen = () => onHalfOpenCalled++;

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset,
                onHalfOpen: onHalfOpen);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        onBreakCalled.ShouldBe(0);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        onBreakCalled.ShouldBe(1);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);
        onHalfOpenCalled.ShouldBe(1);
        onResetCalled.ShouldBe(0);
    }

    [Fact]
    public async Task Should_call_onreset_when_manually_resetting_circuit()
    {
        int onBreakCalled = 0;
        int onResetCalled = 0;
        Action<Exception, TimeSpan> onBreak = (_, _) => onBreakCalled++;
        Action onReset = () => { onResetCalled++; };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak, onBreak, onReset);

        onBreakCalled.ShouldBe(0);
        breaker.Isolate();
        onBreakCalled.ShouldBe(1);

        breaker.CircuitState.ShouldBe(CircuitState.Isolated);
        await Should.ThrowAsync<IsolatedCircuitException>(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));

        onResetCalled.ShouldBe(0);
        breaker.Reset();
        onResetCalled.ShouldBe(1);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        await Should.NotThrowAsync(() => breaker.ExecuteAsync(() => TaskHelper.EmptyTask));
    }

    #region Tests of supplied parameters to onBreak delegate

    [Fact]
    public async Task Should_call_onbreak_with_the_last_raised_exception()
    {
        Exception? passedException = null;

        Action<Exception, TimeSpan, Context> onBreak = (exception, _, _) => { passedException = exception; };
        Action<Context> onReset = _ => { };

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        passedException?.ShouldBeOfType<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_call_onbreak_with_a_state_of_closed()
    {
        CircuitState? transitionedState = null;

        Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, _, _) => transitionedState = state;
        Action<Context> onReset = _ => { };
        Action onHalfOpen = () => { };

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset,
                onHalfOpen: onHalfOpen);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        transitionedState?.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task Should_call_onbreak_with_a_state_of_half_open()
    {
        List<CircuitState> transitionedStates = [];

        Action<Exception, CircuitState, TimeSpan, Context> onBreak = (_, state, _, _) => { transitionedStates.Add(state); };
        Action<Context> onReset = _ => { };
        Action onHalfOpen = () => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset,
                onHalfOpen: onHalfOpen);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration raises an exception, so circuit should open again
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        transitionedStates[0].ShouldBe(CircuitState.Closed);
        transitionedStates[1].ShouldBe(CircuitState.HalfOpen);
    }

    [Fact]
    public async Task Should_call_onbreak_with_the_correct_timespan()
    {
        TimeSpan? passedBreakTimespan = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => passedBreakTimespan = timespan;
        Action<Context> onReset = _ => { };

        TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // No adjustment to SystemClock.UtcNow, so all exceptions raised within same timeslice
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        passedBreakTimespan.ShouldBe(durationOfBreak);
    }

    [Fact]
    public void Should_open_circuit_with_timespan_maxvalue_if_manual_override_open()
    {
        TimeSpan? passedBreakTimespan = null;
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => passedBreakTimespan = timespan;
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: onBreak,
                onReset: onReset);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // manually break circuit
        breaker.Isolate();
        breaker.CircuitState.ShouldBe(CircuitState.Isolated);

        passedBreakTimespan.ShouldBe(TimeSpan.MaxValue);
    }

    [Fact]
    public void Should_throw_when_failureThreshold_is_less_or_equals_than_zero()
    {
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => _ = timespan;
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: onBreak,
                onReset: onReset);

        Should.Throw<ArgumentOutOfRangeException>(action).ParamName.ShouldBe("failureThreshold");
    }

    [Fact]
    public void Should_throw_when_failureThreshold_is_more_than_one()
    {
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { _ = timespan; };
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 1.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: onBreak,
                onReset: onReset);

        Should.Throw<ArgumentOutOfRangeException>(action).ParamName.ShouldBe("failureThreshold");
    }

    [Fact]
    public void Should_throw_when_samplingDuration_is_less_than_resolutionOfCircuit()
    {
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { _ = timespan; };
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromMilliseconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: onBreak,
                onReset: onReset);

        Should.Throw<ArgumentOutOfRangeException>(action).ParamName.ShouldBe("samplingDuration");
    }

    [Fact]
    public void Should_throw_when_minimumThroughput_is_less_or_equals_than_one()
    {
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { _ = timespan; };
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 0,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: onBreak,
                onReset: onReset);

        Should.Throw<ArgumentOutOfRangeException>(action).ParamName.ShouldBe("minimumThroughput");
    }

    [Fact]
    public void Should_throw_when_durationOfBreak_is_negative_timespan()
    {
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { _ = timespan; };
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromMinutes(-1),
                onBreak: onBreak,
                onReset: onReset);

        Should.Throw<ArgumentOutOfRangeException>(action).ParamName.ShouldBe("durationOfBreak");
    }

    [Fact]
    public void Should_throw_when_onReset_is_null()
    {
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { _ = timespan; };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: onBreak,
                onReset: null);

        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe("onReset");
    }

    [Fact]
    public void Should_throw_when_onHalfOpen_is_null()
    {
        Action<Exception, TimeSpan, Context> onBreak = (_, timespan, _) => { _ = timespan; };
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        Action action = () => Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: onBreak,
                onReset: onReset,
                onHalfOpen: null);

        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe("onHalfOpen");
    }

    #endregion

    #region Tests that supplied context is passed to stage-change delegates

    [Fact]
    public async Task Should_call_onbreak_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) => { contextData = context; };
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(
            () =>
                breaker.RaiseExceptionAsync<DivideByZeroException>(
                    CreateDictionary("key1", "value1", "key2", "value2")));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_onreset_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<Exception, TimeSpan, Context> onBreak = (_, _, _) => { };
        Action<Context> onReset = context => { contextData = context; };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration should invoke onReset, with context
        await breaker.ExecuteAsync(_ => TaskHelper.EmptyTask, CreateDictionary("key1", "value1", "key2", "value2"));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        IDictionary<string, object> contextData = CreateDictionary("key1", "value1", "key2", "value2");

        Action<Exception, TimeSpan, Context> onBreak = (_, _, context) => { contextData = context; };
        Action<Context> onReset = _ => { };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        contextData.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        Action<Exception, TimeSpan, Context> onBreak =
            (_, _, context) => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };
        Action<Context> onReset =
            context => { contextValue = context.ContainsKey("key") ? context["key"].ToString() : null; };

        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromSeconds(30);

        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 4,
                durationOfBreak: durationOfBreak,
                onBreak: onBreak,
                onReset: onReset);

        // Four of four actions in this test throw handled failures.
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>(CreateDictionary("key", "original_value")));
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        contextValue.ShouldBe("original_value");

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // but not yet reset

        // first call after duration is successful, so circuit should reset
        await breaker.ExecuteAsync(_ => TaskHelper.EmptyTask, CreateDictionary("key", "new_value"));

        breaker.CircuitState.ShouldBe(CircuitState.Closed);
        contextValue.ShouldBe("new_value");
    }

    #endregion

    #endregion

    #region LastException property

    [Fact]
    public void Should_initialise_LastException_to_null_on_creation()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(30));

        breaker.LastException.ShouldBeNull();
    }

    [Fact]
    public async Task Should_set_LastException_on_handling_exception_even_when_not_breaking()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(30));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.LastException.ShouldBeOfType<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_set_LastException_to_last_raised_exception_when_breaking()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(30));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        breaker.LastException.ShouldBeOfType<DivideByZeroException>();
    }

    [Fact]
    public async Task Should_set_LastException_to_null_on_circuit_reset()
    {
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 2,
                durationOfBreak: TimeSpan.FromSeconds(30));

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        breaker.LastException.ShouldBeOfType<DivideByZeroException>();

        breaker.Reset();

        breaker.LastException.ShouldBeNull();
    }

    #endregion

    #region Cancellation support

    [Fact]
    public async Task Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            await Should.NotThrowAsync(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);
    }

    [Fact]
    public async Task Should_report_cancellation_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_during_faulting_action_execution_when_user_delegate_observes_cancellationToken()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_faulting_from_faulting_action_execution_when_user_delegate_does_not_observe_cancellation()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

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
            await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_cancellation_when_both_open_circuit_and_cancellation()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 2, durationOfBreak);

        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        await Should.ThrowAsync<DivideByZeroException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());

        var ex = await Should.ThrowAsync<BrokenCircuitException>(() => breaker.RaiseExceptionAsync<DivideByZeroException>());
        ex.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        ex.InnerException.ShouldBeOfType<DivideByZeroException>();

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

            var ex2 = await Should.ThrowAsync<OperationCanceledException>(() => breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex2.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);
    }

    [Fact]
    public async Task Should_honour_different_cancellationToken_captured_implicitly_by_action()
    {
        // Before CancellationToken support was built in to Polly, users of the library may have implicitly captured a CancellationToken and used it to cancel actions.
        // For backwards compatibility, Polly should not confuse these with its own CancellationToken; it should distinguish TaskCanceledExceptions thrown with different CancellationTokens.
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

        int attemptsInvoked = 0;

        using (var policyCancellationTokenSource = new CancellationTokenSource())
        using (var implicitlyCapturedActionCancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken policyCancellationToken = policyCancellationTokenSource.Token;
            CancellationToken implicitlyCapturedActionCancellationToken = implicitlyCapturedActionCancellationTokenSource.Token;

            implicitlyCapturedActionCancellationTokenSource.Cancel();

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => breaker.ExecuteAsync(async _ =>
            {
                attemptsInvoked++;
                await TaskHelper.EmptyTask;
                implicitlyCapturedActionCancellationToken.ThrowIfCancellationRequested();
            }, policyCancellationToken));

            ex.CancellationToken.ShouldBe(implicitlyCapturedActionCancellationToken);
        }

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_execute_func_returning_value_when_cancellationToken_not_cancelled()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

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

            await Should.NotThrowAsync(async () => result = await breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true));
        }

        result.ShouldNotBeNull();
        result.Value.ShouldBeTrue();

        attemptsInvoked.ShouldBe(1);
    }

    [Fact]
    public async Task Should_honour_and_report_cancellation_during_func_execution()
    {
        var durationOfBreak = TimeSpan.FromMinutes(1);
        var breaker = Policy
            .Handle<DivideByZeroException>()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 4, durationOfBreak);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(async () => result = await breaker.RaiseExceptionAndOrCancellationAsync<DivideByZeroException, bool>(scenario, cancellationTokenSource, onExecute, true));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        result.ShouldBeNull();

        attemptsInvoked.ShouldBe(1);
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
