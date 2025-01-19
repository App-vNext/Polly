using Scenario = Polly.Specs.Helpers.PolicyTResultExtensions.ResultAndOrCancellationScenario;

namespace Polly.Specs.Fallback;

public class FallbackTResultSpecs
{
    #region Configuration guard condition tests

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, EmptyStruct> action = null!;
        PolicyBuilder<EmptyStruct> policyBuilder = new PolicyBuilder<EmptyStruct>(exception => exception);
        Action<DelegateResult<EmptyStruct>, Context> onFallback = (_, _) => { };
        Func<DelegateResult<EmptyStruct>, Context, CancellationToken, EmptyStruct> fallbackAction = (_, _, _) => EmptyStruct.Instance;

        var instance = Activator.CreateInstance(
            typeof(FallbackPolicy<EmptyStruct>),
            flags,
            null,
            [policyBuilder, onFallback, fallbackAction],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "EmptyStruct" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null()
    {
        Func<ResultPrimitive> fallbackAction = null!;

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_with_cancellation_is_null()
    {
        Func<CancellationToken, ResultPrimitive> fallbackAction = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback()
    {
        Func<ResultPrimitive> fallbackAction = null!;
        Action<DelegateResult<ResultPrimitive>> onFallback = _ => { };

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback()
    {
        Func<CancellationToken, ResultPrimitive> fallbackAction = null!;
        Action<DelegateResult<ResultPrimitive>> onFallback = _ => { };

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback_with_context()
    {
        Func<Context, ResultPrimitive> fallbackAction = null!;
        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, _) => { };

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback_with_context()
    {
        Func<Context, CancellationToken, ResultPrimitive> fallbackAction = null!;
        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, _) => { };

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null()
    {
        Func<ResultPrimitive> fallbackAction = () => ResultPrimitive.Substitute;
        Action<DelegateResult<ResultPrimitive>> onFallback = null!;

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallback");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_action_with_cancellation()
    {
        Func<CancellationToken, ResultPrimitive> fallbackAction = _ => ResultPrimitive.Substitute;
        Action<DelegateResult<ResultPrimitive>> onFallback = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallback");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_context()
    {
        Func<Context, ResultPrimitive> fallbackAction = _ => ResultPrimitive.Substitute;
        Action<DelegateResult<ResultPrimitive>, Context> onFallback = null!;

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallback");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_context_with_action_with_cancellation()
    {
        Func<Context, CancellationToken, ResultPrimitive> fallbackAction = (_, _) => ResultPrimitive.Substitute;
        Action<DelegateResult<ResultPrimitive>, Context> onFallback = null!;

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallback");
    }

    #endregion

    #region Policy operation tests

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_does_not_raise_fault()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction);

        fallbackPolicy.Execute(() => ResultPrimitive.Good);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_raises_fault_not_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.FaultAgain).ShouldBe(ResultPrimitive.FaultAgain);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_return_fallback_value_when_executed_delegate_raises_fault_handled_by_policy()
    {
        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(ResultPrimitive.Substitute);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.Fault).ShouldBe(ResultPrimitive.Substitute);
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_raises_fault_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .Fallback(fallbackAction);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.Fault).ShouldBe(ResultPrimitive.Substitute);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_raises_one_of_results_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .OrResult(ResultPrimitive.FaultAgain)
                                .Fallback(fallbackAction);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.FaultAgain).ShouldBe(ResultPrimitive.Substitute);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_raises_fault_not_one_of_faults_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .OrResult(ResultPrimitive.FaultAgain)
                                .Fallback(fallbackAction);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.FaultYetAgain).ShouldBe(ResultPrimitive.FaultYetAgain);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_result_raised_does_not_match_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult<ResultPrimitive>(_ => false)
                                .Fallback(fallbackAction);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.Fault).ShouldBe(ResultPrimitive.Fault);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_raises_fault_not_handled_by_any_of_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult<ResultPrimitive>(r => r == ResultPrimitive.Fault)
                                .OrResult(r => r == ResultPrimitive.FaultAgain)
                                .Fallback(fallbackAction);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.FaultYetAgain).ShouldBe(ResultPrimitive.FaultYetAgain);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_result_raised_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult<ResultPrimitive>(_ => true)
                                .Fallback(fallbackAction);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.Undefined).ShouldBe(ResultPrimitive.Substitute);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_result_raised_matches_one_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
                                .HandleResult<ResultPrimitive>(_ => true)
                                .OrResult(ResultPrimitive.FaultAgain)
                                .Fallback(fallbackAction);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.Undefined).ShouldBe(ResultPrimitive.Substitute);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_handle_result_raised_by_fallback_delegate_even_if_is_result_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<ResultClass> fallbackAction = () =>
        {
            fallbackActionExecuted = true;
            return new ResultClass(ResultPrimitive.Fault, "FromFallbackAction");
        };

        FallbackPolicy<ResultClass> fallbackPolicy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .Fallback(fallbackAction);

        var result = fallbackPolicy.RaiseResultSequence(new ResultClass(ResultPrimitive.Fault, "FromExecuteDelegate"));

        result.ShouldNotBeNull();
        result.ResultCode.ShouldBe(ResultPrimitive.Fault);
        result.SomeString.ShouldBe("FromFallbackAction");
        fallbackActionExecuted.ShouldBeTrue();
    }

    #endregion

    #region onPolicyEvent delegate tests

    [Fact]
    public void Should_call_onFallback_passing_result_triggering_fallback()
    {
        bool fallbackActionExecuted = false;
        Func<ResultClass> fallbackAction = () => { fallbackActionExecuted = true; return new ResultClass(ResultPrimitive.Substitute); };

        ResultClass? resultPassedToOnFallback = null;
        Action<DelegateResult<ResultClass>> onFallback = r => { resultPassedToOnFallback = r.Result; };

        FallbackPolicy<ResultClass> fallbackPolicy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        ResultClass resultFromDelegate = new ResultClass(ResultPrimitive.Fault);
        fallbackPolicy.Execute(() => resultFromDelegate);

        fallbackActionExecuted.ShouldBeTrue();
        resultPassedToOnFallback.ShouldNotBeNull();
        resultPassedToOnFallback.ShouldBe(resultFromDelegate);
    }

    [Fact]
    public void Should_not_call_onFallback_when_executed_delegate_does_not_raise_fault()
    {
        Func<ResultPrimitive> fallbackAction = () => ResultPrimitive.Substitute;

        bool onFallbackExecuted = false;
        Action<DelegateResult<ResultPrimitive>> onFallback = _ => { onFallbackExecuted = true; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Execute(() => ResultPrimitive.Good);

        onFallbackExecuted.ShouldBeFalse();
    }

    #endregion

    #region Context passing tests

    [Fact]
    public void Should_call_onFallback_with_the_passed_context()
    {
        Func<Context, ResultPrimitive> fallbackAction = _ => ResultPrimitive.Substitute;

        IDictionary<string, object>? contextData = null;

        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, ctx) => { contextData = ctx; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Execute(_ => ResultPrimitive.Fault,
            CreateDictionary("key1", "value1", "key2", "value2"))
            .ShouldBe(ResultPrimitive.Substitute);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
    {
        Func<Context, ResultPrimitive> fallbackAction = _ => ResultPrimitive.Substitute;

        IDictionary<string, object>? contextData = null;

        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, ctx) => { contextData = ctx; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.ExecuteAndCapture(_ => ResultPrimitive.Fault,
            CreateDictionary("key1", "value1", "key2", "value2"))
            .Result.ShouldBe(ResultPrimitive.Substitute);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_call_onFallback_with_independent_context_for_independent_calls()
    {
        Func<Context, ResultPrimitive> fallbackAction = _ => ResultPrimitive.Substitute;

        IDictionary<ResultPrimitive, object> contextData = new Dictionary<ResultPrimitive, object>();

        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (dr, ctx) => { contextData[dr.Result] = ctx["key"]; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Execute(_ => ResultPrimitive.Fault, CreateDictionary("key", "value1"))
            .ShouldBe(ResultPrimitive.Substitute);

        fallbackPolicy.Execute(_ => ResultPrimitive.FaultAgain, CreateDictionary("key", "value2"))
            .ShouldBe(ResultPrimitive.Substitute);

        contextData.Count.ShouldBe(2);
        contextData.Keys.ShouldContain(ResultPrimitive.Fault);
        contextData.Keys.ShouldContain(ResultPrimitive.FaultAgain);
        contextData[ResultPrimitive.Fault].ShouldBe("value1");
        contextData[ResultPrimitive.FaultAgain].ShouldBe("value2");
    }

    [Fact]
    public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;
        bool onFallbackExecuted = false;

        Func<Context, ResultPrimitive> fallbackAction = _ => ResultPrimitive.Substitute;
        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, ctx) => { onFallbackExecuted = true; capturedContext = ctx; };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.Fault);

        onFallbackExecuted.ShouldBeTrue();
        capturedContext.ShouldBeEmpty();
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Func<Context, CancellationToken, ResultPrimitive> fallbackAction = (ctx, _) => { contextData = ctx; return ResultPrimitive.Substitute; };

        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, _) => { };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy<ResultPrimitive>
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Execute(_ => ResultPrimitive.Fault,
                CreateDictionary("key1", "value1", "key2", "value2"))
            .ShouldBe(ResultPrimitive.Substitute);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        Func<Context, CancellationToken, ResultPrimitive> fallbackAction = (ctx, _) => { contextData = ctx; return ResultPrimitive.Substitute; };

        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, _) => { };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.ExecuteAndCapture(_ => ResultPrimitive.Fault,
                CreateDictionary("key1", "value1", "key2", "value2"))
            .Result.ShouldBe(ResultPrimitive.Substitute);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Context_should_be_empty_at_fallbackAction_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;
        bool fallbackExecuted = false;

        Func<Context, CancellationToken, ResultPrimitive> fallbackAction = (ctx, _) => { fallbackExecuted = true; capturedContext = ctx; return ResultPrimitive.Substitute; };

        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, _) => { };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.RaiseResultSequence(ResultPrimitive.Fault);

        fallbackExecuted.ShouldBeTrue();
        capturedContext.ShouldBeEmpty();
    }
    #endregion

    #region Fault passing tests

    [Fact]
    public void Should_call_fallbackAction_with_the_fault()
    {
        DelegateResult<ResultPrimitive>? fallbackOutcome = null;

        Func<DelegateResult<ResultPrimitive>, Context, CancellationToken, ResultPrimitive> fallbackAction =
            (outcome, _, _) => { fallbackOutcome = outcome; return ResultPrimitive.Substitute; };

        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, _) => { };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy<ResultPrimitive>
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Execute(() => ResultPrimitive.Fault)
            .ShouldBe(ResultPrimitive.Substitute);

        fallbackOutcome!.ShouldNotBeNull();
        fallbackOutcome!.Exception.ShouldBeNull();
        fallbackOutcome!.Result.ShouldBe(ResultPrimitive.Fault);
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_fault_when_execute_and_capture()
    {
        DelegateResult<ResultPrimitive>? fallbackOutcome = null;

        Func<DelegateResult<ResultPrimitive>, Context, CancellationToken, ResultPrimitive> fallbackAction =
            (outcome, _, _) => { fallbackOutcome = outcome; return ResultPrimitive.Substitute; };

        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, _) => { };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy<ResultPrimitive>
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        var result = fallbackPolicy.ExecuteAndCapture(() => ResultPrimitive.Fault);
        result.ShouldNotBeNull();
        result.Result.ShouldBe(ResultPrimitive.Substitute);

        fallbackOutcome!.ShouldNotBeNull();
        fallbackOutcome!.Exception.ShouldBeNull();
        fallbackOutcome!.Result.ShouldBe(ResultPrimitive.Fault);
    }

    [Fact]
    public void Should_not_call_fallbackAction_with_the_fault_if_fault_unhandled()
    {
        DelegateResult<ResultPrimitive>? fallbackOutcome = null;

        Func<DelegateResult<ResultPrimitive>, Context, CancellationToken, ResultPrimitive> fallbackAction =
            (outcome, _, _) => { fallbackOutcome = outcome; return ResultPrimitive.Substitute; };

        Action<DelegateResult<ResultPrimitive>, Context> onFallback = (_, _) => { };

        FallbackPolicy<ResultPrimitive> fallbackPolicy = Policy<ResultPrimitive>
            .HandleResult(ResultPrimitive.Fault)
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Execute(() => ResultPrimitive.FaultAgain)
            .ShouldBe(ResultPrimitive.FaultAgain);

        fallbackOutcome.ShouldBeNull();
    }

    #endregion

    #region Cancellation tests

    [Fact]
    public void Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good)
                .ShouldBe(ResultPrimitive.Good);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault)
            .ShouldBe(ResultPrimitive.Substitute);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            Should.Throw<OperationCanceledException>(() => policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);

        fallbackActionExecuted.ShouldBeFalse();

    }

    [Fact]
    public void Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_does_not_handle_cancellations()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Should.Throw<OperationCanceledException>(() => policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_handle_cancellation_and_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_handles_cancellations()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Or<OperationCanceledException>()
            .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good)
            .ShouldBe(ResultPrimitive.Substitute);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_report_cancellation_and_not_execute_fallback_if_non_faulting_action_execution_completes_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good)
            .ShouldBe(ResultPrimitive.Good);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_report_unhandled_fault_and_not_execute_fallback_if_action_execution_raises_unhandled_fault_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute, ResultPrimitive.FaultYetAgain)
            .ShouldBe(ResultPrimitive.FaultYetAgain);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_handle_handled_fault_and_execute_fallback_following_faulting_action_execution_when_user_delegate_does_not_observe_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<ResultPrimitive> fallbackAction = () => { fallbackActionExecuted = true; return ResultPrimitive.Substitute; };

        FallbackPolicy<ResultPrimitive> policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            policy.RaiseResultSequenceAndOrCancellation(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault)
            .ShouldBe(ResultPrimitive.Substitute);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    #endregion
}
