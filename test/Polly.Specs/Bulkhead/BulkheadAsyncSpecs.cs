namespace Polly.Specs.Bulkhead;

[Collection(Constants.ParallelThreadDependentTestCollection)]
public class BulkheadAsyncSpecs(ITestOutputHelper testOutputHelper) : BulkheadSpecsBase(testOutputHelper)
{
    #region Configuration

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
            .BulkheadAsync(0, 1);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("maxParallelization");
    }

    [Fact]
    public void Should_throw_when_maxQueuingActions_less_than_zero()
    {
        Action policy = () => Policy
            .BulkheadAsync(1, -1);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("maxQueuingActions");
    }

    [Fact]
    public void Should_throw_when_onBulkheadRejected_is_null()
    {
        Action policy = () => Policy
            .BulkheadAsync(1, 0, null!);

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

        using var bulkhead = Policy.BulkheadAsync(1, onRejectedAsync);
        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
        using (var cancellationSource = new CancellationTokenSource())
        {
            _ = Task.Run(() => { bulkhead.ExecuteAsync(async () => { await tcs.Task; }); });

            Within(CohesionTimeLimit, () => Expect(0, () => bulkhead.BulkheadAvailableCount, nameof(bulkhead.BulkheadAvailableCount)));

            await bulkhead.Awaiting(b => b.ExecuteAsync(_ => TaskHelper.EmptyTask, contextPassedToExecute)).Should().ThrowAsync<BulkheadRejectedException>();

            cancellationSource.Cancel();
            tcs.SetCanceled();
        }

        contextPassedToOnRejected!.Should().NotBeNull();
        contextPassedToOnRejected!.OperationKey.Should().Be(operationKey);
        contextPassedToOnRejected!.Should().BeSameAs(contextPassedToExecute);
    }

    #endregion

    #region Bulkhead behaviour

    protected override IBulkheadPolicy GetBulkhead(int maxParallelization, int maxQueuingActions) =>
        Policy.BulkheadAsync(maxParallelization, maxQueuingActions);

    protected override Task ExecuteOnBulkhead(IBulkheadPolicy bulkhead, TraceableAction action) =>
        action.ExecuteOnBulkheadAsync((AsyncBulkheadPolicy)bulkhead);

    #endregion
}
