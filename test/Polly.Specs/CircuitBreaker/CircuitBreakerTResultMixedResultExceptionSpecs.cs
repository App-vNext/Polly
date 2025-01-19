namespace Polly.Specs.CircuitBreaker;

[Collection(Constants.SystemClockDependentTestCollection)]
public class CircuitBreakerTResultMixedResultExceptionSpecs : IDisposable
{
    #region Circuit-breaker threshold-to-break tests

    [Fact]
    public void Should_open_circuit_with_exception_after_specified_number_of_specified_exception_have_been_returned_when_result_policy_handling_exceptions_only()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy<ResultPrimitive>
                        .Handle<DivideByZeroException>()
                        .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        var exception = Should.Throw<BrokenCircuitException>(() => breaker.RaiseResultSequence(ResultPrimitive.Good));
        exception.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        exception.InnerException.ShouldBeOfType<DivideByZeroException>();

        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public void Should_open_circuit_with_the_last_exception_after_specified_number_of_exceptions_and_results_have_been_raised__breaking_on_result__when_configuring_result_first()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .Or<DivideByZeroException>()
                        .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        var exception = Should.Throw<BrokenCircuitException>(() => breaker.RaiseResultSequence(ResultPrimitive.Good));
        exception.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        exception.InnerException.ShouldBeOfType<DivideByZeroException>();

        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public void Should_open_circuit_with_the_last_handled_result_after_specified_number_of_exceptions_and_results_have_been_raised__breaking_on_result__when_configuring_result_first()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .Or<DivideByZeroException>()
                        .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        var exception = Should.Throw<BrokenCircuitException<ResultPrimitive>>(() => breaker.RaiseResultSequence(ResultPrimitive.Good));
        exception.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        exception.Result.ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public void Should_open_circuit_with_the_last_exception_after_specified_number_of_exceptions_and_results_have_been_raised__breaking_on_result__when_configuring_exception_first()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        var exception = Should.Throw<BrokenCircuitException>(() => breaker.RaiseResultSequence(ResultPrimitive.Good));
        exception.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        exception.InnerException.ShouldBeOfType<DivideByZeroException>();

        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public void Should_open_circuit_with_the_last_handled_result_after_specified_number_of_exceptions_and_results_have_been_raised__breaking_on_result__when_configuring_exception_first()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        var exception = Should.Throw<BrokenCircuitException<ResultPrimitive>>(() => breaker.RaiseResultSequence(ResultPrimitive.Good));
        exception.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        exception.Result.ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public void Should_open_circuit_if_results_and_exceptions_returned_match_combination_of_the_result_and_exception_predicates()
    {
        CircuitBreakerPolicy<ResultClass> breaker = Policy
            .Handle<ArgumentException>(e => e.ParamName == "key")
            .OrResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException("message", "key")));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.Fault))
              .ResultCode.ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        var exception = Should.Throw<BrokenCircuitException<ResultClass>>(() => breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.Good)));
        exception.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        exception.Result.ResultCode.ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public void Should_not_open_circuit_if_result_returned_is_not_one_of_the_configured_results_or_exceptions()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
               .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
               .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
               .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public void Should_not_open_circuit_if_exception_thrown_is_not_one_of_the_configured_results_or_exceptions()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
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
              .ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
              .ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(new ResultClass(ResultPrimitive.FaultAgain))
              .ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        // non-matched exception predicate
        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException("message", "value")));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException("message", "value")));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException("message", "value")));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public void Should_open_circuit_with_the_last_exception_after_specified_number_of_exceptions_and_results_have_been_raised__configuring_multiple_results_and_exceptions()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .Or<ArgumentException>()
            .OrResult(ResultPrimitive.FaultAgain)
            .CircuitBreaker(4, TimeSpan.FromMinutes(1));

        breaker.RaiseResultSequence(ResultPrimitive.Fault).ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(ResultPrimitive.FaultAgain).ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException()));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 4 exception raised, circuit is now open
        var exception = Should.Throw<BrokenCircuitException>(() => breaker.RaiseResultSequence(ResultPrimitive.Good));
        exception.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        exception.InnerException.ShouldBeOfType<ArgumentException>();

        breaker.CircuitState.ShouldBe(CircuitState.Open);
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

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
              .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 4 exception raised, circuit is now open
        var exception = Should.Throw<BrokenCircuitException<ResultPrimitive>>(() => breaker.RaiseResultSequence(ResultPrimitive.Good));
        exception.Message.ShouldBe("The circuit is now open and is not allowing calls.");
        exception.Result.ShouldBe(ResultPrimitive.FaultAgain);

        breaker.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public void Should_not_open_circuit_if_result_raised_or_exception_thrown_is_not_one_of_the_handled_results_or_exceptions()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .Or<DivideByZeroException>()
                        .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
              .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.RaiseResultSequence(ResultPrimitive.FaultAgain)
              .ShouldBe(ResultPrimitive.FaultAgain);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<ArgumentException>(() => breaker.RaiseResultAndOrExceptionSequence(new ArgumentException()));
        breaker.CircuitState.ShouldBe(CircuitState.Closed);
    }

    #endregion

    #region Circuit-breaker open->half-open->open/closed tests

    [Fact]
    public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_a_fault()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .Or<DivideByZeroException>()
                        .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        Should.Throw<BrokenCircuitException>(() => breaker.RaiseResultSequence(ResultPrimitive.Fault));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration returns a fault, so circuit should break again
        breaker.RaiseResultSequence(ResultPrimitive.Fault)
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        Should.Throw<BrokenCircuitException>(() => breaker.RaiseResultSequence(ResultPrimitive.Fault));
    }

    [Fact]
    public void Should_open_circuit_again_after_the_specified_duration_has_passed_if_the_next_call_raises_an_exception()
    {
        var time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SystemClock.UtcNow = () => time;

        var durationOfBreak = TimeSpan.FromMinutes(1);

        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
                        .HandleResult(ResultPrimitive.Fault)
                        .Or<DivideByZeroException>()
                        .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
              .ShouldBe(ResultPrimitive.Fault);
        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        // 2 exception raised, circuit is now open
        Should.Throw<BrokenCircuitException>(() => breaker.RaiseResultSequence(ResultPrimitive.Fault));
        breaker.CircuitState.ShouldBe(CircuitState.Open);

        SystemClock.UtcNow = () => time.Add(durationOfBreak);

        // duration has passed, circuit now half open
        breaker.CircuitState.ShouldBe(CircuitState.HalfOpen);

        // first call after duration returns a fault, so circuit should break again
        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));
        breaker.CircuitState.ShouldBe(CircuitState.Open);
        Should.Throw<BrokenCircuitException>(() => breaker.RaiseResultSequence(ResultPrimitive.Fault));

    }

    #endregion

    #region State-change delegate tests

    #region Tests of supplied parameters to onBreak delegate

    [Fact]
    public void Should_call_onbreak_with_the_last_handled_result()
    {
        ResultPrimitive? handledResult = null;

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (outcome, _, _) => handledResult = outcome.Result;
        Action<Context> onReset = _ => { };

        TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, durationOfBreak, onBreak, onReset);

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        handledResult?.ShouldBe(ResultPrimitive.Fault);
    }

    [Fact]
    public void Should_call_onbreak_with_the_last_raised_exception()
    {
        Exception? lastException = null;

        Action<DelegateResult<ResultPrimitive>, TimeSpan, Context> onBreak = (outcome, _, _) => lastException = outcome.Exception;
        Action<Context> onReset = _ => { };

        TimeSpan durationOfBreak = TimeSpan.FromMinutes(1);

        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, durationOfBreak, onBreak, onReset);

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
            .ShouldBe(ResultPrimitive.Fault);

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        lastException.ShouldBeOfType<DivideByZeroException>();
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

        breaker.LastHandledResult.ShouldBe(default);
        breaker.LastException.ShouldBeNull();
    }

    [Fact]
    public void Should_set_LastHandledResult_on_handling_result_even_when_not_breaking()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.LastHandledResult.ShouldBe(ResultPrimitive.Fault);
        breaker.LastException.ShouldBeNull();
    }

    [Fact]
    public void Should_set_LastException_on_exception_even_when_not_breaking()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));

        breaker.CircuitState.ShouldBe(CircuitState.Closed);

        breaker.LastHandledResult.ShouldBe(default);
        breaker.LastException.ShouldBeOfType<DivideByZeroException>();
    }

    [Fact]
    public void Should_set_LastHandledResult_to_last_handled_result_when_breaking()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        breaker.LastHandledResult.ShouldBe(ResultPrimitive.Fault);
        breaker.LastException.ShouldBeNull();
    }

    [Fact]
    public void Should_set_LastException_to_last_exception_when_breaking()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
            .ShouldBe(ResultPrimitive.Fault);

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        breaker.LastHandledResult.ShouldBe(default);
        breaker.LastException.ShouldBeOfType<DivideByZeroException>();
    }

    [Fact]
    public void Should_set_LastHandledResult_and_LastException_to_default_on_circuit_reset()
    {
        CircuitBreakerPolicy<ResultPrimitive> breaker = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        Should.Throw<DivideByZeroException>(() => breaker.RaiseResultAndOrExceptionSequence(new DivideByZeroException()));

        breaker.RaiseResultSequence(ResultPrimitive.Fault)
            .ShouldBe(ResultPrimitive.Fault);

        breaker.CircuitState.ShouldBe(CircuitState.Open);

        breaker.LastHandledResult.ShouldBe(ResultPrimitive.Fault);
        breaker.LastException.ShouldBeNull();

        breaker.Reset();

        breaker.LastHandledResult.ShouldBe(default);
        breaker.LastException.ShouldBeNull();
    }

    #endregion

    public void Dispose() =>
        SystemClock.Reset();
}
