namespace Polly.Specs;

public class PolicySpecs
{
    #region Execute tests

    [Fact]
    public void Executing_the_policy_action_should_execute_the_specified_action()
    {
        var executed = false;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _) => { });

        policy.Execute(() => executed = true);

        executed.ShouldBeTrue();
    }

    [Fact]
    public void Executing_the_policy_function_should_execute_the_specified_function_and_return_the_result()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _) => { });

        var result = policy.Execute(() => 2);

        result.ShouldBe(2);
    }

    #endregion

    #region ExecuteAndCapture tests

    [Fact]
    public void Executing_the_policy_action_successfully_should_return_success_result()
    {
        var result = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _) => { })
            .ExecuteAndCapture(() => { });

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Successful);
        result.FinalException.ShouldBeNull();
        result.ExceptionType.ShouldBeNull();
    }

    [Fact]
    public void Executing_the_policy_action_and_failing_with_a_handled_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
    {
        var handledException = new DivideByZeroException();

        var result = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _) => { })
            .ExecuteAndCapture(() => throw handledException);

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Failure);
        result.FinalException.ShouldBeSameAs(handledException);
        result.ExceptionType.ShouldBe(ExceptionType.HandledByThisPolicy);
    }

    [Fact]
    public void Executing_the_policy_action_and_failing_with_an_unhandled_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
    {
        var unhandledException = new Exception();

        var result = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _) => { })
            .ExecuteAndCapture(() => throw unhandledException);

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Failure);
        result.FinalException.ShouldBeSameAs(unhandledException);
        result.ExceptionType.ShouldBe(ExceptionType.Unhandled);
    }

    [Fact]
    public void Executing_the_policy_function_successfully_should_return_success_result()
    {
        var result = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _) => { })
            .ExecuteAndCapture(() => int.MaxValue);

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Successful);
        result.FinalException.ShouldBeNull();
        result.ExceptionType.ShouldBeNull();
        result.FaultType.ShouldBeNull();
        result.FinalHandledResult.ShouldBe(default);
        result.Result.ShouldBe(int.MaxValue);
    }

    [Fact]
    public void Executing_the_policy_function_and_failing_with_a_handled_exception_type_should_return_failure_result_indicating_that_exception_type_is_one_handled_by_this_policy()
    {
        var handledException = new DivideByZeroException();

        var result = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _) => { })
            .ExecuteAndCapture<int>(() => throw handledException);

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Failure);
        result.FinalException.ShouldBeSameAs(handledException);
        result.ExceptionType.ShouldBe(ExceptionType.HandledByThisPolicy);
        result.FaultType.ShouldBe(FaultType.ExceptionHandledByThisPolicy);
        result.FinalHandledResult.ShouldBe(default);
        result.Result.ShouldBe(default);
    }

    [Fact]
    public void Executing_the_policy_function_and_failing_with_an_unhandled_exception_type_should_return_failure_result_indicating_that_exception_type_is_unhandled_by_this_policy()
    {
        var unhandledException = new Exception();

        var result = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _) => { })
            .ExecuteAndCapture<int>(() => throw unhandledException);

        result.ShouldNotBeNull();
        result.Outcome.ShouldBe(OutcomeType.Failure);
        result.FinalException.ShouldBeSameAs(unhandledException);
        result.ExceptionType.ShouldBe(ExceptionType.Unhandled);
        result.FaultType.ShouldBe(FaultType.UnhandledException);
        result.FinalHandledResult.ShouldBe(default);
        result.Result.ShouldBe(default);
    }

    #endregion

    #region Context tests

    [Fact]
    public void Executing_the_policy_action_should_throw_when_context_data_is_null()
    {
        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.Execute(_ => { }, (IDictionary<string, object>)null!));
    }

    [Fact]
    public void Executing_the_policy_action_should_throw_when_context_is_null()
    {
        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.Execute(_ => { }, null!))
            .ParamName.ShouldBe("context");
    }

    [Fact]
    public void Executing_the_policy_function_should_throw_when_context_data_is_null()
    {
        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.Execute(_ => 2, (IDictionary<string, object>)null!));
    }

    [Fact]
    public void Executing_the_policy_function_should_throw_when_context_is_null()
    {
        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.Execute(_ => 2, null!))
            .ParamName.ShouldBe("context");
    }

    [Fact]
    public void Executing_the_policy_function_should_pass_context_to_executed_delegate()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);
        Context? capturedContext = null;

        Policy policy = Policy.NoOp();

        policy.Execute(context => capturedContext = context, executionContext);

        capturedContext.ShouldBeSameAs(executionContext);
    }

    [Fact]
    public void Execute_and_capturing_the_policy_action_should_throw_when_context_data_is_null()
    {
        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.ExecuteAndCapture(_ => { }, (IDictionary<string, object>)null!));
    }

    [Fact]
    public void Execute_and_capturing_the_policy_action_should_throw_when_context_is_null()
    {
        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.ExecuteAndCapture(_ => { }, null!))
            .ParamName.ShouldBe("context");
    }

    [Fact]
    public void Execute_and_capturing_the_policy_function_should_throw_when_context_data_is_null()
    {
        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.ExecuteAndCapture(_ => 2, (IDictionary<string, object>)null!));
    }

    [Fact]
    public void Execute_and_capturing_the_policy_function_should_throw_when_context_is_null()
    {
        Policy policy = Policy
            .Handle<DivideByZeroException>()
            .Retry((_, _, _) => { });

        Should.Throw<ArgumentNullException>(() => policy.ExecuteAndCapture(_ => 2, null!))
              .ParamName.ShouldBe("context");
    }

    [Fact]
    public void Execute_and_capturing_the_policy_function_should_pass_context_to_executed_delegate()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);
        Context? capturedContext = null;

        Policy policy = Policy.NoOp();

        policy.ExecuteAndCapture(context => { capturedContext = context; }, executionContext);

        capturedContext.ShouldBeSameAs(executionContext);
    }

    [Fact]
    public void Execute_and_capturing_the_policy_function_should_pass_context_to_PolicyResult()
    {
        string operationKey = "SomeKey";
        Context executionContext = new Context(operationKey);

        Policy policy = Policy.NoOp();

        policy.ExecuteAndCapture(_ => { }, executionContext)
            .Context.ShouldBeSameAs(executionContext);
    }

    [Fact]
    public void PolicyKey_Is_Correct()
    {
        // Arrange
        var policy = Policy.NoOp();

        // Act
        var actual = policy.PolicyKey;

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldStartWith("NoOpPolicy-");
    }

    [Fact]
    public void PolicyKey_Is_Immutable()
    {
        // Arrange
        var policy = Policy.NoOp();
        var expected = policy.PolicyKey;

        // Act
        var exception = Should.Throw<ArgumentException>(() => policy.WithPolicyKey("foo"));

        // Assert
        exception.ParamName.ShouldBe("policyKey");
        exception.Message.ShouldStartWith("PolicyKey cannot be changed once set; or (when using the default value after the PolicyKey property has been accessed.");
        policy.PolicyKey.ShouldBe(expected);
    }

    #endregion
}
