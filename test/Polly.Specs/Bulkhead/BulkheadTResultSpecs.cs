﻿namespace Polly.Specs.Bulkhead;

[Collection(Constants.ParallelThreadDependentTestCollection)]
public class BulkheadTResultSpecs : BulkheadSpecsBase
{
    public BulkheadTResultSpecs(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    #region Configuration

    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, EmptyStruct> action = null!;
        var maxParallelization = 1;
        var maxQueueingActions = 1;
        Action<Context> onBulkheadRejected = _ => { };

        var instance = Activator.CreateInstance(
            typeof(BulkheadPolicy<EmptyStruct>),
            flags,
            null,
            [maxParallelization, maxQueueingActions, onBulkheadRejected],
            null)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "EmptyStruct" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken]);

        var exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.And.InnerException.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("action");
    }

    [Fact]
    public void Should_throw_when_maxParallelization_less_or_equal_to_zero_and_no_maxQueuingActions()
    {
        Action policy = () => Policy
            .Bulkhead<int>(0);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("maxParallelization");
    }

    [Fact]
    public void Should_throw_when_maxParallelization_less_or_equal_to_zero()
    {
        Action policy = () => Policy
            .Bulkhead<int>(0, 1);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("maxParallelization");
    }

    [Fact]
    public void Should_throw_when_maxQueuingActions_less_than_zero()
    {
        Action policy = () => Policy
            .Bulkhead<int>(1, -1);

        policy.Should().Throw<ArgumentOutOfRangeException>().And
            .ParamName.Should().Be("maxQueuingActions");
    }

    [Fact]
    public void Should_throw_when_onBulkheadRejected_is_null()
    {
        Action policy = () => Policy
            .Bulkhead<int>(1, 0, null!);

        policy.Should().Throw<ArgumentNullException>().And
            .ParamName.Should().Be("onBulkheadRejected");
    }

    #endregion

    #region onBulkheadRejected delegate

    [Fact]
    public void Should_call_onBulkheadRejected_with_passed_context()
    {
        string operationKey = "SomeKey";
        Context contextPassedToExecute = new Context(operationKey);

        Context? contextPassedToOnRejected = null;
        Action<Context> onRejected = ctx => { contextPassedToOnRejected = ctx; };

        using BulkheadPolicy<int> bulkhead = Policy.Bulkhead<int>(1, onRejected);
        TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
        using (var cancellationSource = new CancellationTokenSource())
        {
            Task.Run(() =>
            {
                bulkhead.Execute(() =>
                {
                    tcs.Task.Wait();
                    return 0;
                });
            }, CancellationToken.None);

            Within(CohesionTimeLimit, () => Expect(0, () => bulkhead.BulkheadAvailableCount, nameof(bulkhead.BulkheadAvailableCount)));

            bulkhead.Invoking(b => b.Execute(_ => 1, contextPassedToExecute)).Should().Throw<BulkheadRejectedException>();

            cancellationSource.Cancel();

#if NET
            tcs.SetCanceled(CancellationToken.None);
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
        Policy.Bulkhead<ResultPrimitive>(maxParallelization, maxQueuingActions);

    protected override Task ExecuteOnBulkhead(IBulkheadPolicy bulkhead, TraceableAction action) =>
        action.ExecuteOnBulkhead<ResultPrimitive>((BulkheadPolicy<ResultPrimitive>)bulkhead);

    #endregion
}
