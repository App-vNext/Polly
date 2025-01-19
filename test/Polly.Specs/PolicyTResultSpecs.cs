namespace Polly.Specs;

public class PolicyTResultSpecs
{
    #region Execute tests

    [Fact]
    public void Executing_the_policy_function_should_execute_the_specified_function_and_return_the_result()
    {
        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _) => { });

        var result = policy.Execute(() => ResultPrimitive.Good);

        result.ShouldBe(ResultPrimitive.Good);
    }

    #endregion

    #region ExecuteAndCapture tests

    [Fact]
    public void Executing_the_policy_function_successfully_should_return_success_result()
    {
        var result = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _) => { })
            .ExecuteAndCapture(() => ResultPrimitive.Good);

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Successful);
        result.FinalException.ShouldBeNull();
        result.ExceptionType.ShouldBeNull();
        result.Result.ShouldBe(ResultPrimitive.Good);
        result.FinalHandledResult.ShouldBe(default);
        result.FaultType.ShouldBeNull();
    }

    [Fact]
    public void Executing_the_policy_function_and_failing_with_a_handled_result_should_return_failure_result_indicating_that_result_is_one_handled_by_this_policy()
    {
        var handledResult = ResultPrimitive.Fault;

        var result = Policy
            .HandleResult(handledResult)
            .Retry((_, _) => { })
            .ExecuteAndCapture(() => handledResult);

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Failure);
        result.FinalException.ShouldBeNull();
        result.ExceptionType.ShouldBeNull();
        result.FaultType.ShouldBe(FaultType.ResultHandledByThisPolicy);
        result.FinalHandledResult.ShouldBe(handledResult);
        result.Result.ShouldBe(default);
    }

    [Fact]
    public void Executing_the_policy_function_and_returning_an_unhandled_result_should_return_result_not_indicating_any_failure()
    {
        var handledResult = ResultPrimitive.Fault;
        var unhandledResult = ResultPrimitive.Good;

        var result = Policy
            .HandleResult(handledResult)
            .Retry((_, _) => { })
            .ExecuteAndCapture(() => unhandledResult);

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Successful);
        result.FinalException.ShouldBeNull();
        result.ExceptionType.ShouldBeNull();
        result.Result.ShouldBe(unhandledResult);
        result.FinalHandledResult.ShouldBe(default);
        result.FaultType.ShouldBeNull();
    }

    #endregion

    #region Context tests

    [Fact]
    public void Executing_the_policy_function_should_throw_when_context_data_is_null()
    {
        Policy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.Execute(_ => ResultPrimitive.Good, (IDictionary<string, object>)null!));
    }

    [Fact]
    public void Executing_the_policy_function_should_throw_when_context_is_null()
    {
        Policy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.Execute(_ => ResultPrimitive.Good, null!))
            .ParamName.ShouldBe("context");
    }

    [Fact]
    public void Executing_the_policy_function_should_pass_context_to_executed_delegate()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);
        Context? capturedContext = null;

        Policy<ResultPrimitive> policy = Policy.NoOp<ResultPrimitive>();

        policy.Execute(context => { capturedContext = context; return ResultPrimitive.Good; }, executionContext);

        capturedContext.ShouldNotBeNull();
        capturedContext.ShouldBeSameAs(executionContext);
    }

    [Fact]
    public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
    {
        Policy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.ExecuteAndCapture(_ => ResultPrimitive.Good, (IDictionary<string, object>)null!));
    }

    [Fact]
    public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
    {
        Policy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.ExecuteAndCapture(_ => ResultPrimitive.Good, null!))
              .ParamName.ShouldBe("context");
    }

    [Fact]
    public void Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);
        Context? capturedContext = null;

        Policy<ResultPrimitive> policy = Policy.NoOp<ResultPrimitive>();

        policy.ExecuteAndCapture(context => { capturedContext = context; return ResultPrimitive.Good; }, executionContext);

        capturedContext.ShouldNotBeNull();
        capturedContext.ShouldBeSameAs(executionContext);
    }

    [Fact]
    public void Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);

        Policy<ResultPrimitive> policy = Policy.NoOp<ResultPrimitive>();

        policy.ExecuteAndCapture(_ => ResultPrimitive.Good, executionContext)
            .Context.ShouldBeSameAs(executionContext);
    }

    #endregion
}
