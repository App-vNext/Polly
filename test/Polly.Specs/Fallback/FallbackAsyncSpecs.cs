using Scenario = Polly.Specs.Helpers.PolicyExtensionsAsync.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.Fallback;

public class FallbackAsyncSpecs
{
    #region Configuration guard condition tests

    [Fact]
    public void Should_throw_when_fallback_func_is_null()
    {
        Func<CancellationToken, Task> fallbackActionAsync = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .FallbackAsync(fallbackActionAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_func_is_null_with_onFallback()
    {
        Func<CancellationToken, Task> fallbackActionAsync = null!;
        Func<Exception, Task> onFallbackAsync = _ => TaskHelper.EmptyTask;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_func_is_null_with_onFallback_with_context()
    {
        Func<Context, CancellationToken, Task> fallbackActionAsync = null!;
        Func<Exception, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null()
    {
        Func<CancellationToken, Task> fallbackActionAsync = _ => TaskHelper.EmptyTask;
        Func<Exception, Task> onFallbackAsync = null!;

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallbackAsync");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_context()
    {
        Func<Context, CancellationToken, Task> fallbackActionAsync = (_, _) => TaskHelper.EmptyTask;
        Func<Exception, Context, Task> onFallbackAsync = null!;

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallbackAsync");
    }

    #endregion

    #region Policy operation tests

    [Fact]
    public async Task Should_not_execute_fallback_when_executed_delegate_does_not_throw()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
                                .Handle<DivideByZeroException>()
                                .FallbackAsync(fallbackActionAsync);

        await fallbackPolicy.ExecuteAsync(() => TaskHelper.EmptyTask);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_execute_fallback_when_executed_delegate_throws_exception_not_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
                                .Handle<DivideByZeroException>()
                                .FallbackAsync(fallbackActionAsync);

        await Should.ThrowAsync<ArgumentNullException>(() => fallbackPolicy.RaiseExceptionAsync<ArgumentNullException>());

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_execute_fallback_when_executed_delegate_throws_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
                                .Handle<DivideByZeroException>()
                                .FallbackAsync(fallbackActionAsync);

        await Should.NotThrowAsync(() => fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_execute_fallback_when_executed_delegate_throws_one_of_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
                                .Handle<DivideByZeroException>()
                                .Or<ArgumentException>()
                                .FallbackAsync(fallbackActionAsync);

        await Should.NotThrowAsync(() => fallbackPolicy.RaiseExceptionAsync<ArgumentException>());

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_not_execute_fallback_when_executed_delegate_throws_exception_not_one_of_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
                                .Handle<DivideByZeroException>()
                                .Or<NullReferenceException>()
                                .FallbackAsync(fallbackActionAsync);

        await Should.ThrowAsync<ArgumentNullException>(() => fallbackPolicy.RaiseExceptionAsync<ArgumentNullException>());

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_execute_fallback_when_exception_thrown_does_not_match_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
                                .Handle<DivideByZeroException>(_ => false)
                                .FallbackAsync(fallbackActionAsync);

        await Should.ThrowAsync<DivideByZeroException>(() => fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_not_execute_fallback_when_exception_thrown_does_not_match_any_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
                                .Handle<DivideByZeroException>(_ => false)
                                .Or<ArgumentNullException>(_ => false)
                                .FallbackAsync(fallbackActionAsync);

        await Should.ThrowAsync<DivideByZeroException>(() => fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_execute_fallback_when_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
                                .Handle<DivideByZeroException>(_ => true)
                                .FallbackAsync(fallbackActionAsync);

        await Should.NotThrowAsync(() => fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_execute_fallback_when_exception_thrown_matches_one_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
                                .Handle<DivideByZeroException>(_ => true)
                                .Or<ArgumentNullException>()
                                .FallbackAsync(fallbackActionAsync);

        await Should.NotThrowAsync(() => fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_not_handle_exception_thrown_by_fallback_delegate_even_if_is_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ =>
        {
            fallbackActionExecuted = true;
            throw new DivideByZeroException { HelpLink = "FromFallbackAction" };
        };

        var fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .FallbackAsync(fallbackActionAsync);

        var ex = await Should.ThrowAsync<DivideByZeroException>(() => fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>((e, _) => e.HelpLink = "FromExecuteDelegate"));
        ex.HelpLink.ShouldBe("FromFallbackAction");

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_throw_for_generic_method_execution_on_non_generic_policy()
    {
        var fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .FallbackAsync(_ => TaskHelper.EmptyTask);

        await Should.ThrowAsync<InvalidOperationException>(() => fallbackPolicy.ExecuteAsync<int>(() => Task.FromResult(0)));
    }

    #endregion

    #region onPolicyEvent delegate tests

    [Fact]
    public async Task Should_call_onFallback_passing_exception_triggering_fallback()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        Exception? exceptionPassedToOnFallback = null;
        Func<Exception, Task> onFallbackAsync = ex => { exceptionPassedToOnFallback = ex; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        Exception instanceToThrow = new ArgumentNullException("myParam");
        await fallbackPolicy.RaiseExceptionAsync(instanceToThrow);

        fallbackActionExecuted.ShouldBeTrue();
        exceptionPassedToOnFallback.ShouldBeOfType<ArgumentNullException>();
        exceptionPassedToOnFallback.ShouldBe(instanceToThrow);
    }

    [Fact]
    public async Task Should_not_call_onFallback_when_executed_delegate_does_not_throw()
    {
        Func<CancellationToken, Task> fallbackActionAsync = _ => TaskHelper.EmptyTask;

        bool onFallbackExecuted = false;
        Func<Exception, Task> onFallbackAsync = _ => { onFallbackExecuted = true; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        await fallbackPolicy.ExecuteAsync(() => TaskHelper.EmptyTask);

        onFallbackExecuted.ShouldBeFalse();
    }

    #endregion

    #region Context passing tests

    [Fact]
    public async Task Should_call_onFallback_with_the_passed_context()
    {
        Func<Context, CancellationToken, Task> fallbackActionAsync = (_, _) => TaskHelper.EmptyTask;

        IDictionary<string, object>? contextData = null;

        Func<Exception, Context, Task> onFallbackAsync = (_, ctx) => { contextData = ctx; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        await Should.NotThrowAsync(
            () =>
                fallbackPolicy.ExecuteAsync(
                    _ => throw new ArgumentNullException(),
                    CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
    {
        Func<Context, CancellationToken, Task> fallbackActionAsync = (_, _) => TaskHelper.EmptyTask;

        IDictionary<string, object>? contextData = null;

        Func<Exception, Context, Task> onFallbackAsync = (_, ctx) => { contextData = ctx; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        await Should.NotThrowAsync(
            () =>
                fallbackPolicy.ExecuteAndCaptureAsync(
                    _ => throw new ArgumentNullException(),
                    CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_onFallback_with_independent_context_for_independent_calls()
    {
        Func<Context, CancellationToken, Task> fallbackActionAsync = (_, _) => TaskHelper.EmptyTask;

        var contextData = new Dictionary<Type, object>();

        Func<Exception, Context, Task> onFallbackAsync = (ex, ctx) => { contextData[ex.GetType()] = ctx["key"]; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Or<DivideByZeroException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        await Should.NotThrowAsync(() => fallbackPolicy.ExecuteAsync(_ => throw new ArgumentNullException(), CreateDictionary("key", "value1")));
        await Should.NotThrowAsync(() => fallbackPolicy.ExecuteAsync(_ => throw new DivideByZeroException(), CreateDictionary("key", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue(typeof(ArgumentNullException), "value1");
        contextData.ShouldContainKeyAndValue(typeof(DivideByZeroException), "value2");
        contextData.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;
        bool onFallbackExecuted = false;

        Func<Context, CancellationToken, Task> fallbackActionAsync = (_, _) => TaskHelper.EmptyTask;
        Func<Exception, Context, Task> onFallbackAsync = (_, ctx) => { onFallbackExecuted = true; capturedContext = ctx; return TaskHelper.EmptyTask; };

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Or<DivideByZeroException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        await fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>();

        onFallbackExecuted.ShouldBeTrue();
        capturedContext.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_call_fallbackAction_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Func<Context, CancellationToken, Task> fallbackActionAsync = (ctx, _) => { contextData = ctx; return TaskHelper.EmptyTask; };

        Func<Exception, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        await Should.NotThrowAsync(
            () =>
                fallbackPolicy.ExecuteAsync(
                    _ => throw new ArgumentNullException(),
                    CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public async Task Should_call_fallbackAction_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        Func<Context, CancellationToken, Task> fallbackActionAsync = (ctx, _) => { contextData = ctx; return TaskHelper.EmptyTask; };

        Func<Exception, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        await Should.NotThrowAsync(
            () =>
                fallbackPolicy.ExecuteAndCaptureAsync(
                    _ => throw new ArgumentNullException(),
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

        Func<Context, CancellationToken, Task> fallbackActionAsync = (ctx, _) => { fallbackExecuted = true; capturedContext = ctx; return TaskHelper.EmptyTask; };

        Func<Exception, Context, Task> onFallbackAsync = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Or<DivideByZeroException>()
            .FallbackAsync(fallbackActionAsync, onFallbackAsync);

        await fallbackPolicy.RaiseExceptionAsync<DivideByZeroException>();

        fallbackExecuted.ShouldBeTrue();
        capturedContext.ShouldBeEmpty();
    }

    #endregion

    #region Exception passing tests

    [Fact]
    public async Task Should_call_fallbackAction_with_the_exception()
    {
        Exception? fallbackException = null;

        Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, _, _) => { fallbackException = ex; return TaskHelper.EmptyTask; };

        Func<Exception, Context, Task> onFallback = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .FallbackAsync(fallbackFunc, onFallback);

        Exception instanceToThrow = new ArgumentNullException("myParam");
        await Should.NotThrowAsync(() => fallbackPolicy.RaiseExceptionAsync(instanceToThrow));

        fallbackException.ShouldBe(instanceToThrow);
    }

    [Fact]
    public async Task Should_call_fallbackAction_with_the_exception_when_execute_and_capture()
    {
        Exception? fallbackException = null;

        Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, _, _) => { fallbackException = ex; return TaskHelper.EmptyTask; };

        Func<Exception, Context, Task> onFallback = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .FallbackAsync(fallbackFunc, onFallback);

        await Should.NotThrowAsync(() => fallbackPolicy.ExecuteAndCaptureAsync(() => throw new ArgumentNullException()));

        fallbackException.ShouldNotBeNull()
            .ShouldBeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task Should_call_fallbackAction_with_the_matched_inner_exception_unwrapped()
    {
        Exception? fallbackException = null;

        Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, _, _) => { fallbackException = ex; return TaskHelper.EmptyTask; };

        Func<Exception, Context, Task> onFallback = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .HandleInner<ArgumentNullException>()
            .FallbackAsync(fallbackFunc, onFallback);

        Exception instanceToCapture = new ArgumentNullException("myParam");
        Exception instanceToThrow = new Exception(string.Empty, instanceToCapture);
        await Should.NotThrowAsync(() => fallbackPolicy.RaiseExceptionAsync(instanceToThrow));

        fallbackException.ShouldBe(instanceToCapture);
    }

    [Fact]
    public async Task Should_call_fallbackAction_with_the_matched_inner_of_aggregate_exception_unwrapped()
    {
        Exception? fallbackException = null;

        Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, _, _) => { fallbackException = ex; return TaskHelper.EmptyTask; };

        Func<Exception, Context, Task> onFallback = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .HandleInner<ArgumentNullException>()
            .FallbackAsync(fallbackFunc, onFallback);

        Exception instanceToCapture = new ArgumentNullException("myParam");
        Exception instanceToThrow = new AggregateException(instanceToCapture);
        await Should.NotThrowAsync(() => fallbackPolicy.RaiseExceptionAsync(instanceToThrow));

        fallbackException.ShouldBe(instanceToCapture);
    }

    [Fact]
    public async Task Should_not_call_fallbackAction_with_the_exception_if_exception_unhandled()
    {
        Exception? fallbackException = null;

        Func<Exception, Context, CancellationToken, Task> fallbackFunc = (ex, _, _) => { fallbackException = ex; return TaskHelper.EmptyTask; };

        Func<Exception, Context, Task> onFallback = (_, _) => TaskHelper.EmptyTask;

        var fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .FallbackAsync(fallbackFunc, onFallback);

        await Should.ThrowAsync<ArgumentNullException>(() => fallbackPolicy.ExecuteAsync(() => throw new ArgumentNullException()));

        fallbackException.ShouldBeNull();
    }

    #endregion

    #region Cancellation tests

    [Fact]
    public async Task Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var policy = Policy.Handle<DivideByZeroException>()
                           .FallbackAsync(fallbackActionAsync);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            await Should.NotThrowAsync(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_execute_fallback_when_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var policy = Policy.Handle<DivideByZeroException>()
                           .FallbackAsync(fallbackActionAsync);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = null,
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            await Should.NotThrowAsync(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var policy = Policy.Handle<DivideByZeroException>()
                           .FallbackAsync(fallbackActionAsync);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            cancellationTokenSource.Cancel();

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_does_not_handle_cancellations()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var policy = Policy.Handle<DivideByZeroException>()
                           .FallbackAsync(fallbackActionAsync);

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

            var ex = await Should.ThrowAsync<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
            ex.CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_handle_cancellation_and_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_handles_cancellations()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var policy = Policy.Handle<DivideByZeroException>()
                           .Or<OperationCanceledException>()
                           .FallbackAsync(fallbackActionAsync);

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

            await Should.NotThrowAsync(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_not_report_cancellation_and_not_execute_fallback_if_non_faulting_action_execution_completes_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var policy = Policy.Handle<DivideByZeroException>()
                           .FallbackAsync(fallbackActionAsync);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await Should.NotThrowAsync(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_report_unhandled_fault_and_not_execute_fallback_if_action_execution_raises_unhandled_fault_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var policy = Policy.Handle<DivideByZeroException>()
                           .FallbackAsync(fallbackActionAsync);

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
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await Should.ThrowAsync<NullReferenceException>(() => policy.RaiseExceptionAndOrCancellationAsync<NullReferenceException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_handle_handled_fault_and_execute_fallback_following_faulting_action_execution_when_user_delegate_does_not_observe_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Func<CancellationToken, Task> fallbackActionAsync = _ => { fallbackActionExecuted = true; return TaskHelper.EmptyTask; };

        var policy = Policy.Handle<DivideByZeroException>()
                           .FallbackAsync(fallbackActionAsync);

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
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            await Should.NotThrowAsync(() => policy.RaiseExceptionAndOrCancellationAsync<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    #endregion
}
