namespace Polly.Specs.Retry;

public class RetryTResultMixedResultExceptionSpecs
{
    [Fact]
    public void Should_handle_exception_when_TResult_policy_handling_exceptions_only()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>
            .Handle<DivideByZeroException>().Retry(1);

        ResultPrimitive result = policy.RaiseResultAndOrExceptionSequence(new DivideByZeroException(), ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_throw_unhandled_exception_when_TResult_policy_handling_exceptions_only()
    {
        Policy<ResultPrimitive> policy = Policy<ResultPrimitive>
            .Handle<DivideByZeroException>().Retry(1);

        Should.Throw<ArgumentException>(() => policy.RaiseResultAndOrExceptionSequence(new ArgumentException(), ResultPrimitive.Good));
    }

    [Fact]
    public void Should_handle_both_exception_and_specified_result_if_raised_same_number_of_times_as_retry_count__when_configuring_results_before_exceptions()
    {
        Policy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Or<DivideByZeroException>()
            .Retry(2);

        ResultPrimitive result = policy.RaiseResultAndOrExceptionSequence(ResultPrimitive.Fault, new DivideByZeroException(), ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_handle_both_exception_and_specified_result_if_raised_same_number_of_times_as_retry_count__when_configuring_exception_before_result()
    {
        Policy<ResultPrimitive> policy = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .Retry(2);

        ResultPrimitive result = policy.RaiseResultAndOrExceptionSequence(ResultPrimitive.Fault, new DivideByZeroException(), ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_handle_both_exceptions_and_specified_results_if_raised_same_number_of_times_as_retry_count__mixing_exceptions_and_results_specifying_exceptions_first()
    {
        Policy<ResultPrimitive> policy = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .Or<ArgumentException>()
            .OrResult(ResultPrimitive.FaultAgain)
            .Retry(4);

        ResultPrimitive result = policy.RaiseResultAndOrExceptionSequence(ResultPrimitive.Fault, new DivideByZeroException(), new ArgumentException(), ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_handle_both_exceptions_and_specified_results_if_raised_same_number_of_times_as_retry_count__mixing_exceptions_and_results_specifying_results_first()
    {
        Policy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Or<DivideByZeroException>()
            .OrResult(ResultPrimitive.FaultAgain)
            .Or<ArgumentException>()
            .Retry(4);

        ResultPrimitive result = policy.RaiseResultAndOrExceptionSequence(ResultPrimitive.Fault, new DivideByZeroException(), new ArgumentException(), ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_return_handled_result_when_handled_result_returned_next_after_retries_exhaust_handling_both_exceptions_and_specified_results__mixing_exceptions_and_results_specifying_results_first()
    {
        Policy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Or<DivideByZeroException>()
            .OrResult(ResultPrimitive.FaultAgain)
            .Or<ArgumentException>()
            .Retry(3);

        ResultPrimitive result = policy.RaiseResultAndOrExceptionSequence(ResultPrimitive.Fault, new DivideByZeroException(), new ArgumentException(), ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.FaultAgain);
    }

    [Fact]
    public void Should_throw_when_exception_thrown_next_after_retries_exhaust_handling_both_exceptions_and_specified_results__mixing_exceptions_and_results_specifying_results_first()
    {
        Policy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Or<DivideByZeroException>()
            .OrResult(ResultPrimitive.FaultAgain)
            .Or<ArgumentException>()
            .Retry(3);

        Should.Throw<ArgumentException>(() => policy.RaiseResultAndOrExceptionSequence(ResultPrimitive.Fault, new DivideByZeroException(), ResultPrimitive.FaultAgain, new ArgumentException(), ResultPrimitive.Good));
    }

    [Fact]
    public void Should_return_handled_result_when_handled_result_returned_next_after_retries_exhaust_handling_both_exceptions_and_specified_results__mixing_exceptions_and_results_specifying_exceptions_first()
    {
        Policy<ResultPrimitive> policy = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .Or<ArgumentException>()
            .OrResult(ResultPrimitive.FaultAgain)
            .Retry(3);

        ResultPrimitive result = policy.RaiseResultAndOrExceptionSequence(ResultPrimitive.Fault, new DivideByZeroException(), new ArgumentException(), ResultPrimitive.FaultAgain, ResultPrimitive.Good);
        result.ShouldBe(ResultPrimitive.FaultAgain);
    }

    [Fact]
    public void Should_throw_when_exception_thrown_next_after_retries_exhaust_handling_both_exceptions_and_specified_results__mixing_exceptions_and_results_specifying_exceptions_first()
    {
        Policy<ResultPrimitive> policy = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .Or<ArgumentException>()
            .OrResult(ResultPrimitive.FaultAgain)
            .Retry(3);

        Should.Throw<ArgumentException>(() => policy.RaiseResultAndOrExceptionSequence(ResultPrimitive.Fault, new DivideByZeroException(), ResultPrimitive.FaultAgain, new ArgumentException(), ResultPrimitive.Good));
    }

    [Fact]
    public void Should_return_unhandled_result_if_not_one_of_results_or_exceptions_specified()
    {
        Policy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Or<DivideByZeroException>()
            .Retry(2);

        ResultPrimitive result = policy.RaiseResultSequence(ResultPrimitive.FaultAgain);
        result.ShouldBe(ResultPrimitive.FaultAgain);
    }

    [Fact]
    public void Should_throw_if_not_one_of_results_or_exceptions_handled()
    {
        Policy<ResultPrimitive> policy = Policy
            .Handle<DivideByZeroException>()
            .OrResult(ResultPrimitive.Fault)
            .Retry(2);

        Should.Throw<ArgumentException>(() => policy.RaiseResultAndOrExceptionSequence(new ArgumentException(), ResultPrimitive.Good));
    }

    [Fact]
    public void Should_handle_both_exceptions_and_specified_results_with_predicates()
    {
        Policy<ResultClass> policy = Policy
            .Handle<ArgumentException>(e => e.ParamName == "key")
            .OrResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .Retry(2);

        ResultClass result = policy.RaiseResultAndOrExceptionSequence(new ResultClass(ResultPrimitive.Fault), new ArgumentException("message", "key"), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.ShouldBe(ResultPrimitive.Good);
    }

    [Fact]
    public void Should_throw_if_exception_predicate_not_matched()
    {
        Policy<ResultClass> policy = Policy
            .Handle<ArgumentException>(e => e.ParamName == "key")
            .OrResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .Retry(2);

        Should.Throw<ArgumentException>(() => policy.RaiseResultAndOrExceptionSequence(new ResultClass(ResultPrimitive.Fault), new ArgumentException("message", "value"), new ResultClass(ResultPrimitive.Good)));
    }

    [Fact]
    public void Should_return_unhandled_result_if_result_predicate_not_matched()
    {
        Policy<ResultClass> policy = Policy
            .Handle<ArgumentException>(e => e.ParamName == "key")
            .OrResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .Retry(2);

        ResultClass result = policy.RaiseResultAndOrExceptionSequence(new ArgumentException("message", "key"), new ResultClass(ResultPrimitive.FaultAgain), new ResultClass(ResultPrimitive.Good));
        result.ResultCode.ShouldBe(ResultPrimitive.FaultAgain);
    }
}
