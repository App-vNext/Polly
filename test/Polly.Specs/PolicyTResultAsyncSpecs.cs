namespace Polly.Specs;

public class PolicyTResultAsyncSpecs
{
    #region Execute tests

    [Fact]
    public async Task Executing_the_policy_function_should_execute_the_specified_function_and_return_the_result()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _) => { });

        var result = await policy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good));

        result.ShouldBe(ResultPrimitive.Good);
    }

    #endregion

    #region ExecuteAndCapture tests

    [Fact]
    public async Task Executing_the_policy_function_successfully_should_return_success_result()
    {
        var result = await Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _) => { })
            .ExecuteAndCaptureAsync(() => Task.FromResult(ResultPrimitive.Good));

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Successful);
        result.FinalException.ShouldBeNull();
        result.ExceptionType.ShouldBeNull();
        result.Result.ShouldBe(ResultPrimitive.Good);
        result.FinalHandledResult.ShouldBe(default);
        result.FaultType.ShouldBeNull();
    }

    [Fact]
    public async Task Executing_the_policy_function_and_failing_with_a_handled_result_should_return_failure_result_indicating_that_result_is_one_handled_by_this_policy()
    {
        var handledResult = ResultPrimitive.Fault;

        var result = await Policy
            .HandleResult(handledResult)
            .RetryAsync((_, _) => { })
            .ExecuteAndCaptureAsync(() => Task.FromResult(handledResult));

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Failure);
        result.FinalException.ShouldBeNull();
        result.ExceptionType.ShouldBeNull();
        result.FaultType.ShouldBe(FaultType.ResultHandledByThisPolicy);
        result.FinalHandledResult.ShouldBe(handledResult);
        result.Result.ShouldBe(default);
    }

    [Fact]
    public async Task Executing_the_policy_function_and_returning_an_unhandled_result_should_return_result_not_indicating_any_failure()
    {
        var handledResult = ResultPrimitive.Fault;
        var unhandledResult = ResultPrimitive.Good;

        var result = await Policy
            .HandleResult(handledResult)
            .RetryAsync((_, _) => { })
            .ExecuteAndCaptureAsync(() => Task.FromResult(unhandledResult));

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Successful);
        result.FinalException.ShouldBeNull();
        result.ExceptionType.ShouldBeNull();
        result.Result.ShouldBe(unhandledResult);
        result.FaultType.ShouldBeNull();
        result.FinalHandledResult.ShouldBe(default);
    }

    #endregion

    #region Context tests

    [Fact]
    public async Task Executing_the_policy_function_should_throw_when_context_data_is_null()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, _) => { });

        await Should.ThrowAsync<ArgumentNullException>(() => policy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Good), (IDictionary<string, object>)null!));
    }

    [Fact]
    public async Task Executing_the_policy_function_should_throw_when_context_is_null()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, _) => { });

        var ex = await Should.ThrowAsync<ArgumentNullException>(() => policy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Good), null!));
        ex.ParamName.ShouldBe("context");
    }

    [Fact]
    public async Task Executing_the_policy_function_should_pass_context_to_executed_delegate()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);
        Context? capturedContext = null;

        var policy = Policy.NoOpAsync<ResultPrimitive>();

        await policy.ExecuteAsync(context => { capturedContext = context; return Task.FromResult(ResultPrimitive.Good); }, executionContext);

        capturedContext.ShouldBeSameAs(executionContext);
    }

    [Fact]
    public async Task Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, _) => { });

        var ex = await Should.ThrowAsync<ArgumentNullException>(() => policy.ExecuteAndCaptureAsync(_ => Task.FromResult(ResultPrimitive.Good), null!));
        ex.ParamName.ShouldBe("context");
    }

    [Fact]
    public async Task Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);
        Context? capturedContext = null;

        var policy = Policy.NoOpAsync<ResultPrimitive>();

        await policy.ExecuteAndCaptureAsync(context => { capturedContext = context; return Task.FromResult(ResultPrimitive.Good); }, executionContext);

        capturedContext.ShouldBeSameAs(executionContext);
    }

    [Fact]
    public async Task Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);

        var policy = Policy.NoOpAsync<ResultPrimitive>();

        (await policy.ExecuteAndCaptureAsync(_ => Task.FromResult(ResultPrimitive.Good), executionContext))
            .Context.ShouldBeSameAs(executionContext);
    }

    #endregion
}
