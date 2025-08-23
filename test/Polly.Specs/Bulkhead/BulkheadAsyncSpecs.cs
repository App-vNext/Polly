namespace Polly.Specs.Bulkhead;

[Collection(Constants.ParallelThreadDependentTestCollection)]
public class BulkheadAsyncSpecs(ITestOutputHelper testOutputHelper) : BulkheadSpecsBase(testOutputHelper)
{
    #region Configuration

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;
        var maxParallelization = 1;
        var maxQueueingActions = 1;
        Func<Context, Task> onBulkheadRejectedAsync = (_) => Task.CompletedTask;

        var instance = Activator.CreateInstance(
            typeof(AsyncBulkheadPolicy),
            flags,
            null,
            [maxParallelization, maxQueueingActions, onBulkheadRejectedAsync],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });
        var generic = methodInfo.MakeGenericMethod(typeof(EmptyStruct));

        var func = () => generic.Invoke(instance, [action, new Context(), CancellationToken, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_throw_when_maxParallelization_less_or_equal_to_zero_and_no_maxQueuingActions()
    {
        Action policy = () => Policy
            .BulkheadAsync(0);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("maxParallelization");
    }

    [Fact]
    public void Should_throw_when_maxParallelization_less_or_equal_to_zero()
    {
        Action policy = () => Policy
            .BulkheadAsync(0, 1);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("maxParallelization");
    }

    [Fact]
    public void Should_throw_when_maxQueuingActions_less_than_zero()
    {
        Action policy = () => Policy
            .BulkheadAsync(1, -1);

        Should.Throw<ArgumentOutOfRangeException>(policy)
            .ParamName.ShouldBe("maxQueuingActions");
    }

    [Fact]
    public void Should_not_throw_when_maxQueuingActions_is_int_MaxValue()
    {
        Action policy = () => Policy
            .BulkheadAsync(1, int.MaxValue);

        policy.ShouldNotThrow();
    }

    [Fact]
    public void Should_throw_when_onBulkheadRejected_is_null()
    {
        Action policy = () => Policy
            .BulkheadAsync(1, 0, null!);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onBulkheadRejectedAsync");
    }

    #endregion

    #region onBulkheadRejected delegate

    [Fact]
    public async Task Should_call_onBulkheadRejected_with_passed_context()
    {
        string operationKey = "SomeKey";
        Context contextPassedToExecute = new Context(operationKey);

        Context? contextPassedToOnRejected = null;
        Func<Context, Task> onRejectedAsync = async ctx => { contextPassedToOnRejected = ctx; await TaskHelper.EmptyTask; };

        using var bulkhead = Policy.BulkheadAsync(1, onRejectedAsync);
        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
        using (var cancellationSource = new CancellationTokenSource())
        {
            var task = Task.Run(() => { bulkhead.ExecuteAsync(async () => { await tcs.Task; }); }, CancellationToken);

            Within(CohesionTimeLimit, () => Expect(0, () => bulkhead.BulkheadAvailableCount, nameof(bulkhead.BulkheadAvailableCount)));

            await Should.ThrowAsync<BulkheadRejectedException>(
                () => bulkhead.ExecuteAsync(_ => TaskHelper.EmptyTask, contextPassedToExecute));

            cancellationSource.Cancel();

#if NET
            tcs.SetCanceled(CancellationToken);
#else
            tcs.SetCanceled();
#endif

            await task;
        }

        contextPassedToOnRejected!.ShouldNotBeNull();
        contextPassedToOnRejected!.OperationKey.ShouldBe(operationKey);
        contextPassedToOnRejected!.ShouldBeSameAs(contextPassedToExecute);
    }

    #endregion

    #region Bulkhead behaviour

    protected override IBulkheadPolicy GetBulkhead(int maxParallelization, int maxQueuingActions) =>
        Policy.BulkheadAsync(maxParallelization, maxQueuingActions);

    protected override Task ExecuteOnBulkhead(IBulkheadPolicy bulkhead, TraceableAction action) =>
        action.ExecuteOnBulkheadAsync((AsyncBulkheadPolicy)bulkhead);

    #endregion
}
