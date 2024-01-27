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

        result.Should()
            .Be(ResultPrimitive.Good);
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

        result.Should().BeEquivalentTo(new
        {
            Outcome = OutcomeType.Successful,
            FinalException = (Exception?)null,
            ExceptionType = (ExceptionType?)null,
            Result = ResultPrimitive.Good,
            FinalHandledResult = default(ResultPrimitive),
            FaultType = (FaultType?)null
        });
    }

    [Fact]
    public async Task Executing_the_policy_function_and_failing_with_a_handled_result_should_return_failure_result_indicating_that_result_is_one_handled_by_this_policy()
    {
        var handledResult = ResultPrimitive.Fault;

        var result = await Policy
            .HandleResult(handledResult)
            .RetryAsync((_, _) => { })
            .ExecuteAndCaptureAsync(() => Task.FromResult(handledResult));

        result.Should().BeEquivalentTo(new
        {
            Outcome = OutcomeType.Failure,
            FinalException = (Exception?)null,
            ExceptionType = (ExceptionType?)null,
            FaultType = FaultType.ResultHandledByThisPolicy,
            FinalHandledResult = handledResult,
            Result = default(ResultPrimitive)
        });
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

        result.Should().BeEquivalentTo(new
        {
            Outcome = OutcomeType.Successful,
            FinalException = (Exception?)null,
            ExceptionType = (ExceptionType?)null,
            Result = unhandledResult,
            FinalHandledResult = default(ResultPrimitive),
            FaultType = (FaultType?)null
        });
    }

    #endregion

    #region Context tests

    [Fact]
    public async Task Executing_the_policy_function_should_throw_when_context_data_is_null()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, _) => { });

        await policy.Awaiting(p => p.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Good), (IDictionary<string, object>)null!))
              .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Executing_the_policy_function_should_throw_when_context_is_null()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, _) => { });

        var ex = await policy.Awaiting(p => p.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Good), null!))
              .Should().ThrowAsync<ArgumentNullException>();
        ex.And.ParamName.Should().Be("context");
    }

    [Fact]
    public async Task Executing_the_policy_function_should_pass_context_to_executed_delegate()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);
        Context? capturedContext = null;

        var policy = Policy.NoOpAsync<ResultPrimitive>();

        await policy.ExecuteAsync(context => { capturedContext = context; return Task.FromResult(ResultPrimitive.Good); }, executionContext);

        capturedContext.Should().BeSameAs(executionContext);
    }

    [Fact]
    public async Task Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryAsync((_, _, _) => { });

        var ex = await policy.Awaiting(p => p.ExecuteAndCaptureAsync(_ => Task.FromResult(ResultPrimitive.Good), null!))
              .Should().ThrowAsync<ArgumentNullException>();
        ex.And.ParamName.Should().Be("context");
    }

    [Fact]
    public async Task Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);
        Context? capturedContext = null;

        var policy = Policy.NoOpAsync<ResultPrimitive>();

        await policy.ExecuteAndCaptureAsync(context => { capturedContext = context; return Task.FromResult(ResultPrimitive.Good); }, executionContext);

        capturedContext.Should().BeSameAs(executionContext);
    }

    [Fact]
    public async Task Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);

        var policy = Policy.NoOpAsync<ResultPrimitive>();

        (await policy.ExecuteAndCaptureAsync(_ => Task.FromResult(ResultPrimitive.Good), executionContext))
            .Context.Should().BeSameAs(executionContext);
    }

    #endregion
}
