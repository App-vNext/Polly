using Scenario = Polly.Specs.Helpers.PolicyTResultExtensionsAsync.ResultAndOrCancellationScenario;

namespace Polly.Specs.Fallback;

public class FallbackTResultAsyncSpecs
{
    #region Configuration guard condition tests

    [Fact]
    public void Should_throw_when_fallback_action_is_null()
    {
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = null!;

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .FallbackAsync(fallbackAction);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback()
    {
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = null!;
        Func<DelegateResult<ResultPrimitive>, Task> onFallbackAsync = _ => TaskHelper.EmptyTask;

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .FallbackAsync(fallbackAction, onFallbackAsync);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback_with_context()
    {
        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = null!;
        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .FallbackAsync(fallbackAction, onFallbackAsync);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null()
    {
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => Task.FromResult(ResultPrimitive.Substitute);
        Func<DelegateResult<ResultPrimitive>, Task> onFallbackAsync = null!;

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .FallbackAsync(fallbackAction, onFallbackAsync);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onFallbackAsync");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_context()
    {
        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = (_, _) => Task.FromResult(ResultPrimitive.Substitute);
        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = null!;

        Action policy = () => Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .FallbackAsync(fallbackAction, onFallbackAsync);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onFallbackAsync");
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

        fallbackActionExecuted.Should().BeFalse();
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
            .Should().Be(ResultPrimitive.FaultAgain);

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_return_fallback_value_when_executed_delegate_raises_fault_handled_by_policy()
    {
        var fallbackPolicy = Policy
                                .HandleResult(ResultPrimitive.Fault)
                                .FallbackAsync(ResultPrimitive.Substitute);

        (await fallbackPolicy.RaiseResultSequenceAsync(ResultPrimitive.Fault))
            .Should().Be(ResultPrimitive.Substitute);
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
            .Should().Be(ResultPrimitive.Substitute);

        fallbackActionExecuted.Should().BeTrue();
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
            .Should().Be(ResultPrimitive.Substitute);

        fallbackActionExecuted.Should().BeTrue();
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
            .Should().Be(ResultPrimitive.FaultYetAgain);

        fallbackActionExecuted.Should().BeFalse();
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
            .Should().Be(ResultPrimitive.Fault);

        fallbackActionExecuted.Should().BeFalse();
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
            .Should().Be(ResultPrimitive.FaultYetAgain);

        fallbackActionExecuted.Should().BeFalse();
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
            .Should().Be(ResultPrimitive.Substitute);

        fallbackActionExecuted.Should().BeTrue();
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
            .Should().Be(ResultPrimitive.Substitute);

        fallbackActionExecuted.Should().BeTrue();
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

        (await fallbackPolicy.RaiseResultSequenceAsync(new ResultClass(ResultPrimitive.Fault, "FromExecuteDelegate")))
            .Should().Match<ResultClass>(r => r.ResultCode == ResultPrimitive.Fault && r.SomeString == "FromFallbackAction");

        fallbackActionExecuted.Should().BeTrue();
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

        fallbackActionExecuted.Should().BeTrue();
        resultPassedToOnFallback.Should().NotBeNull();
        resultPassedToOnFallback.Should().Be(resultFromDelegate);
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

        onFallbackExecuted.Should().BeFalse();
    }

    #endregion

    #region Context passing tests

    [Fact]
    public void Should_call_onFallback_with_the_passed_context()
    {
        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = (_, _) => Task.FromResult(ResultPrimitive.Substitute);

        IDictionary<string, object>? contextData = null;

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, ctx) => { contextData = ctx; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        fallbackPolicy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Fault),
            new { key1 = "value1", key2 = "value2" }.AsDictionary())
            .Result
            .Should().Be(ResultPrimitive.Substitute);

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
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

        (await fallbackPolicy.ExecuteAndCaptureAsync(_ => Task.FromResult(ResultPrimitive.Fault),
            new { key1 = "value1", key2 = "value2" }.AsDictionary()))
            .Result.Should().Be(ResultPrimitive.Substitute);

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Should_call_onFallback_with_independent_context_for_independent_calls()
    {
        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackAction = (_, _) => Task.FromResult(ResultPrimitive.Substitute);

        IDictionary<ResultPrimitive, object> contextData = new Dictionary<ResultPrimitive, object>();

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (dr, ctx) => { contextData[dr.Result] = ctx["key"]; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .FallbackAsync(fallbackAction, onFallbackAsync);

        fallbackPolicy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Fault), new { key = "value1" }.AsDictionary())
            .Result
            .Should().Be(ResultPrimitive.Substitute);

        fallbackPolicy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.FaultAgain), new { key = "value2" }.AsDictionary())
            .Result
            .Should().Be(ResultPrimitive.Substitute);

        contextData.Count.Should().Be(2);
        contextData.Keys.Should().Contain(ResultPrimitive.Fault);
        contextData.Keys.Should().Contain(ResultPrimitive.FaultAgain);
        contextData[ResultPrimitive.Fault].Should().Be("value1");
        contextData[ResultPrimitive.FaultAgain].Should().Be("value2");
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
            .Should().Be(ResultPrimitive.Substitute);

        onFallbackExecuted.Should().BeTrue();
        capturedContext.Should().BeEmpty();
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Func<Context, CancellationToken, Task<ResultPrimitive>> fallbackActionAsync = (ctx, _) => { contextData = ctx; return Task.FromResult(ResultPrimitive.Substitute); };

        Func<DelegateResult<ResultPrimitive>, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        fallbackPolicy.ExecuteAsync(_ => Task.FromResult(ResultPrimitive.Fault),
                new { key1 = "value1", key2 = "value2" }.AsDictionary())
            .Result
            .Should().Be(ResultPrimitive.Substitute);

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
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

        await fallbackPolicy.Awaiting(p => p.ExecuteAndCaptureAsync(_ => Task.FromResult(ResultPrimitive.Fault),
                new { key1 = "value1", key2 = "value2" }.AsDictionary()))
            .Should().NotThrowAsync();

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
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
            .Should().Be(ResultPrimitive.Substitute);

        fallbackExecuted.Should().BeTrue();
        capturedContext.Should().BeEmpty();
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
        result.Should().Be(ResultPrimitive.Substitute);

        fallbackOutcome!.Should().NotBeNull();
        fallbackOutcome!.Exception.Should().BeNull();
        fallbackOutcome!.Result.Should().Be(ResultPrimitive.Fault);
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
        result.Should().NotBeNull();
        result.Result.Should().Be(ResultPrimitive.Substitute);

        fallbackOutcome!.Should().NotBeNull();
        fallbackOutcome!.Exception.Should().BeNull();
        fallbackOutcome!.Result.Should().Be(ResultPrimitive.Fault);
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
        result.Should().Be(ResultPrimitive.FaultAgain);

        fallbackOutcome.Should().BeNull();
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

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good))
            .Should().Be(ResultPrimitive.Good);
        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeFalse();
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

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null,
        };

        (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault))
            .Should().Be(ResultPrimitive.Substitute);
        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeTrue();
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

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        cancellationTokenSource.Cancel();

        var ex = await policy.Awaiting(x => x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault))
            .Should().ThrowAsync<OperationCanceledException>();
        ex.And.CancellationToken.Should().Be(cancellationToken);
        attemptsInvoked.Should().Be(0);

        fallbackActionExecuted.Should().BeFalse();

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

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        var ex = await policy.Awaiting(x => x.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good))
            .Should().ThrowAsync<OperationCanceledException>();
        ex.And.CancellationToken.Should().Be(cancellationToken);
        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeFalse();
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

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good))
            .Should().Be(ResultPrimitive.Substitute);
        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeTrue();
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

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Good))
            .Should().Be(ResultPrimitive.Good);
        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeFalse();
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

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.FaultYetAgain))
            .Should().Be(ResultPrimitive.FaultYetAgain);
        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_handle_handled_fault_and_execute_fallback_following_faulting_action_execution_when_user_delegate_does_not_observe_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task<ResultPrimitive>> fallbackAction = _ => { fallbackActionExecuted = true; return Task.FromResult(ResultPrimitive.Substitute); };

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

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

        (await policy.RaiseResultSequenceAndOrCancellationAsync(scenario, cancellationTokenSource, onExecute, ResultPrimitive.Fault))
            .Should().Be(ResultPrimitive.Substitute);
        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeTrue();
    }

    #endregion
}
