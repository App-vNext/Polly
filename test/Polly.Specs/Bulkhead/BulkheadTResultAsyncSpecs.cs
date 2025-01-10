namespace Polly.Specs.Bulkhead;

[Collection(Constants.ParallelThreadDependentTestCollection)]
public class BulkheadTResultAsyncSpecs : BulkheadSpecsBase
{
    public BulkheadTResultAsyncSpecs(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

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
            typeof(AsyncBulkheadPolicy<EmptyStruct>),
            flags,
            null,
            [maxParallelization, maxQueueingActions, onBulkheadRejectedAsync],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken, false]);

        var exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.And.InnerException.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("action");
    }

    [Fact]
    public void Should_throw_when_maxParallelization_less_or_equal_to_zero_and_no_maxQueuingActions()
    {
        Action policy = () => Policy
            .BulkheadAsync(0);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("maxParallelization");
    }

    [Fact]
    public void Should_throw_when_maxParallelization_less_or_equal_to_zero()
    {
        Action policy = () => Policy
            .BulkheadAsync<int>(0, 1);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("maxParallelization");
    }

    [Fact]
    public void Should_throw_when_maxQueuingActions_less_than_zero()
    {
        Action policy = () => Policy
            .BulkheadAsync<int>(1, -1);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("maxQueuingActions");
    }

    [Fact]
    public void Should_throw_when_onBulkheadRejected_is_null()
    {
        Action policy = () => Policy
            .BulkheadAsync<int>(1, 0, null!);

        policy.Should().Throw<ArgumentNullException>().And
            .ParamName.Should().Be("onBulkheadRejectedAsync");
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

        using var bulkhead = Policy.BulkheadAsync<int>(1, onRejectedAsync);
        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
        using (var cancellationSource = new CancellationTokenSource())
        {
            _ = Task.Run(() =>
            {
                bulkhead.ExecuteAsync(async () =>
                {
                    await tcs.Task;
                    return 0;
                });
            }, CancellationToken);

            Within(CohesionTimeLimit, () => Expect(0, () => bulkhead.BulkheadAvailableCount, nameof(bulkhead.BulkheadAvailableCount)));

            await bulkhead.Awaiting(b => b.ExecuteAsync(_ => Task.FromResult(1), contextPassedToExecute)).Should().ThrowAsync<BulkheadRejectedException>();

            cancellationSource.Cancel();

#if NET
            tcs.SetCanceled(CancellationToken);
#else
            tcs.SetCanceled();
#endif
        }

        contextPassedToOnRejected!.Should().NotBeNull();
        contextPassedToOnRejected!.OperationKey.Should().Be(operationKey);
        contextPassedToOnRejected!.Should().BeSameAs(contextPassedToExecute);
    }

    #endregion

    #region Bulkhead behaviour

    protected override IBulkheadPolicy GetBulkhead(int maxParallelization, int maxQueuingActions) =>
        Policy.BulkheadAsync<ResultPrimitive>(maxParallelization, maxQueuingActions);

    protected override Task ExecuteOnBulkhead(IBulkheadPolicy bulkhead, TraceableAction action) =>
        action.ExecuteOnBulkheadAsync<ResultPrimitive>((AsyncBulkheadPolicy<ResultPrimitive>)bulkhead);

    #endregion
}
