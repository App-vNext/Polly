using Scenario = Polly.Specs.Helpers.PolicyTResultExtensionsAsync.ResultAndOrCancellationScenario;

namespace Polly.Specs.Fallback;

public class FallbackTResultAsyncSpecs
{
    #region Configuration guard condition tests

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;
        PolicyBuilder<EmptyStruct> policyBuilder = new PolicyBuilder<EmptyStruct>(exception => exception);
        Func<DelegateResult<EmptyStruct>, Context, Task> onFallbackAsync = (_, _) => Task.CompletedTask;
        Func<DelegateResult<EmptyStruct>, Context, CancellationToken, Task<EmptyStruct>> fallbackAction = (_, _, _) => Task.FromResult(EmptyStruct.Instance);

        var instance = Activator.CreateInstance(
            typeof(AsyncFallbackPolicy<EmptyStruct>),
            flags,
            null,
            [policyBuilder, onFallbackAsync, fallbackAction],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null()
    {
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback()
    {
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = null!;
        Func<DelegateResult<ResultPrimitive>, Task> onFallbackAsync = _ => TaskHelper.EmptyTask;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback_with_context()
    {
        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = null!;
        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null()
    {
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => Task.FromResult(ResultPrimitive.Substitute);
        Func<DelegateResult<ResultPrimitive>, Task> onFallbackAsync = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallbackAsync");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_context()
    {
        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = (_, _) => Task.FromResult(ResultPrimitive.Substitute);
        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = null!;

        Action policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallbackAsync");
    }

    #endregion

    #region Policy operation tests

    [Fact]
    public async Task Should_not_execute_fallback_when_executed_delegate_does_not_raise_fault()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction);

        await fallbackPolicy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good));

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_execute_fallback_when_executed_delegate_raises_fault_not_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain))
            .ShouldBe(ResultPrimitive.FaultAgain);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_return_fallback_value_when_executed_delegate_raises_fault_handled_by_policy()
    {
        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(ResultPrimitive.Substitute);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Substitute);
    }

    [Fact]
    public async Task Should_execute_fallback_when_executed_delegate_raises_fault_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Substitute);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_execute_fallback_when_executed_delegate_raises_one_of_results_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.FaultAgain))
            .ShouldBe(ResultPrimitive.Substitute);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_not_execute_fallback_when_executed_delegate_raises_fault_not_one_of_faults_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.FaultYetAgain))
            .ShouldBe(ResultPrimitive.FaultYetAgain);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_execute_fallback_when_result_raised_does_not_match_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var fallbackPolicy = Policy
            .HandleResult<ResultPrimitive>(_ => false)
            .FallbackAsync(fallbackAction);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Fault);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_execute_fallback_when_executed_delegate_raises_fault_not_handled_by_any_of_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var fallbackPolicy = Policy
            .HandleResult<ResultPrimitive>(r => r == ResultPrimitive.Fault)
            .OrResult(r => r == ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.FaultYetAgain))
            .ShouldBe(ResultPrimitive.FaultYetAgain);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_execute_fallback_when_result_raised_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var fallbackPolicy = Policy
            .HandleResult<ResultPrimitive>(_ => true)
            .FallbackAsync(fallbackAction);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Undefined))
            .ShouldBe(ResultPrimitive.Substitute);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_execute_fallback_when_result_raised_matches_one_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var fallbackPolicy = Policy
            .HandleResult<ResultPrimitive>(_ => true)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Undefined))
            .ShouldBe(ResultPrimitive.Substitute);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_not_handle_result_raised_by_fallback_delegate_even_if_is_result_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultClass>> fallbackAction = _ =>
        {
            fallbackActionExecuted = true;
            return Task.FromResult(new ResultClass(ResultPrimitive.Fault, "FromFallbackAction"));
        };

        var fallbackPolicy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction);

        var result = await fallbackPolicy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault, "FromExecuteDelegate"));
        result.ResultCode.ShouldBe(ResultPrimitive.Fault);
        result.SomeString.ShouldBe("FromFallbackAction");

        fallbackActionExecuted.ShouldBeTrue();
    }

    #endregion

    #region onPolicyEvent delegate tests

    [Fact]
    public async Task Should_call_onFallback_passing_result_triggering_fallback()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultClass>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(new ResultClass(ResultPrimitive.Substitute)); };

        ResultClass? resultPassedToOnFallback = null;
        Func<DelegateResult<ResultClass>, Task> onFallbackAsync = r => { resultPassedToOnFallback = r.Result; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .HandleResult<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        ResultClass resultFromDelegate = new ResultClass(ResultPrimitive.Fault);
        await fallbackPolicy.ExecuteAsync(() => Task.FromResult(resultFromDelegate));

        fallbackActionExecuted.ShouldBeTrue();
        resultPassedToOnFallback.ShouldNotBeNull();
        resultPassedToOnFallback.ShouldBe(resultFromDelegate);
    }

    [Fact]
    public async Task Should_not_call_onFallback_when_executed_delegate_does_not_raise_fault()
    {
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => Task.FromResult(ResultPrimitive.Substitute);

        bool onFallbackExecuted = false;
        Func<DelegateResult<ResultPrimitive>, Task> onFallbackAsync = _ => { onFallbackExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        await fallbackPolicy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Good));

        onFallbackExecuted.ShouldBeFalse();
    }

    #endregion

    #region Context passing tests

    [Fact]
    public async Task Should_call_onFallback_with_the_passed_context()
    {
        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = (_, _) => Task.FromResult(ResultPrimitive.Substitute);

        IDictionary<string, object>? contextData = null;

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, ctx) => { contextData = ctx; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        (await fallbackPolicy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Fault), CreateDictionary("key1", "value1", "key2", "value2"))).ShouldBe(ResultPrimitive.Substitute);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
    {
        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = (_, _) => Task.FromResult(ResultPrimitive.Substitute);

        IDictionary<string, object>? contextData = null;

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, ctx) => { contextData = ctx; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        (await fallbackPolicy.ExecuteAndCaptureAsync(_ => Task.FromResult(ResultPrimitive.Fault), CreateDictionary("key1", "value1", "key2", "value2")))
            .Result.ShouldBe(ResultPrimitive.Substitute);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_onFallback_with_independent_context_for_independent_calls()
    {
        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = (_, _) => Task.FromResult(ResultPrimitive.Substitute);

        var contextData = new Dictionary<ResultPrimitive, object>();

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (dr, ctx) => { contextData[dr.Result] = ctx["key"]; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        (await fallbackPolicy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Fault), CreateDictionary("key", "value1")))
            .ShouldBe(ResultPrimitive.Substitute);

        (await fallbackPolicy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.FaultAgain), CreateDictionary("key", "value2")))
            .ShouldBe(ResultPrimitive.Substitute);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue(ResultPrimitive.Fault, "value1");
        contextData.ShouldContainKeyAndValue(ResultPrimitive.FaultAgain, "value2");
        contextData.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;
        bool onFallbackExecuted = false;

        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = (_, _) => Task.FromResult(ResultPrimitive.Substitute);
        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, ctx) => { onFallbackExecuted = true; capturedContext = ctx; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Substitute);

        onFallbackExecuted.ShouldBeTrue();
        capturedContext.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_call_fallbackAction_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackActionAsync = (ctx, _) => { contextData = ctx; return Task.FromResult(ResultPrimitive.Substitute); };

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        (await fallbackPolicy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Fault), CreateDictionary("key1", "value1", "key2", "value2")))
            .ShouldBe(ResultPrimitive.Substitute);

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_fallbackAction_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackActionAsync = (ctx, _) => { contextData = ctx; return Task.FromResult(ResultPrimitive.Substitute); };

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        await Should.NotThrowAsync(
            () =>
                fallbackPolicy.ExecuteAndCaptureAsync(
                    _ => Task.FromResult(ResultPrimitive.Fault),
                    CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Context_should_be_empty_at_fallbackAction_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;
        bool fallbackExecuted = false;

        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackActionAsync = (ctx, _) =>
        {
            fallbackExecuted = true;
            capturedContext = ctx;
            return Task.FromResult(ResultPrimitive.Substitute);
        };
        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .ShouldBe(ResultPrimitive.Substitute);

        fallbackExecuted.ShouldBeTrue();
        capturedContext.ShouldBeEmpty();
    }
    #endregion

    #region Fault passing tests

    [Fact]
    public async Task Should_call_fallbackAction_with_the_fault()
    {
        DelegateResult<ResultPrimitive>? fallbackOutcome = null;

        Func<DelegateResult<ResultPrimitive>, Context, CancellationToken, Task<ResultPrimitive>> fallbackAction =
            (outcome, _, _) => { fallbackOutcome = outcome; return Task.FromResult(ResultPrimitive.Substitute); };

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallback = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy<ResultPrimitive>
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallback);

        var result = await fallbackPolicy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.Fault));
        result.ShouldBe(ResultPrimitive.Substitute);

        fallbackOutcome!.ShouldNotBeNull();
        fallbackOutcome!.Exception.ShouldBeNull();
        fallbackOutcome!.Result.ShouldBe(ResultPrimitive.Fault);
    }

    [Fact]
    public async Task Should_call_fallbackAction_with_the_fault_when_execute_and_capture()
    {
        DelegateResult<ResultPrimitive>? fallbackOutcome = null;

        Func<DelegateResult<ResultPrimitive>, Context, CancellationToken, Task<ResultPrimitive>> fallbackAction =
            (outcome, _, _) => { fallbackOutcome = outcome; return Task.FromResult(ResultPrimitive.Substitute); };

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallback = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy<ResultPrimitive>
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallback);

        var result = await fallbackPolicy.ExecuteAndCaptureAsync(() => Task.FromResult(ResultPrimitive.Fault));
        result.ShouldNotBeNull();
        result.Result.ShouldBe(ResultPrimitive.Substitute);

        fallbackOutcome!.ShouldNotBeNull();
        fallbackOutcome!.Exception.ShouldBeNull();
        fallbackOutcome!.Result.ShouldBe(ResultPrimitive.Fault);
    }

    [Fact]
    public async Task Should_not_call_fallbackAction_with_the_fault_if_fault_unhandled()
    {
        DelegateResult<ResultPrimitive>? fallbackOutcome = null;

        Func<DelegateResult<ResultPrimitive>, Context, CancellationToken, Task<ResultPrimitive>> fallbackAction =
            (outcome, _, _) => { fallbackOutcome = outcome; return Task.FromResult(ResultPrimitive.Substitute); };

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallback = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy<ResultPrimitive>
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallback);

        var result = await fallbackPolicy.ExecuteAsync(() => Task.FromResult(ResultPrimitive.FaultAgain));
        result.ShouldBe(ResultPrimitive.FaultAgain);

        fallbackOutcome.ShouldBeNull();
    }

    #endregion

    #region Cancellation tests

    [Fact]
    public async Task Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good))
                .ShouldBe(ResultPrimitive.Good);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_execute_fallback_when_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault))
                .ShouldBe(ResultPrimitive.Substitute);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault));
            ex.CancellationToken.ShouldBe(cancellationToken);

        }

        attemptsInvoked.ShouldBe(0);

        fallbackActionExecuted.ShouldBeFalse();

    }

    [Fact]
    public async Task Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_does_not_handle_cancellations()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_handle_cancellation_and_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_handles_cancellations()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .Or<OperationCanceledException>()
            .FallbackAsync(fallbackAction);

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

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good))
                .ShouldBe(ResultPrimitive.Substitute);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_not_report_cancellation_and_not_execute_fallback_if_non_faulting_action_execution_completes_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

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

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good))
                .ShouldBe(ResultPrimitive.Good);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_report_unhandled_fault_and_not_execute_fallback_if_action_execution_raises_unhandled_fault_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

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

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.FaultYetAgain))
                .ShouldBe(ResultPrimitive.FaultYetAgain);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_handle_handled_fault_and_execute_fallback_following_faulting_action_execution_when_user_delegate_does_not_observe_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction);

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

            (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault))
                .ShouldBe(ResultPrimitive.Substitute);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    #endregion
}
