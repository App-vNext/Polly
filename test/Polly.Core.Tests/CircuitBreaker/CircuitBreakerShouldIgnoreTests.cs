using Microsoft.Extensions.Time.Testing;
using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerShouldIgnoreTests
{
    private static readonly TimeSpan BreakDuration = TimeSpan.FromSeconds(5);

    private readonly FakeTimeProvider _timeProvider = new();
    private readonly CircuitBreakerStateProvider _stateProvider = new();
    private readonly List<int> _halfOpenAttempts = [];

    private static CancellationToken Token => TestCancellation.Token;

    [Fact]
    public async Task ShouldIgnore_Null_IntermediateOutcomesCountAsSuccesses()
    {
        var pipeline = CreatePipeline(ignore: false);

        for (int i = 0; i < 10; i++)
        {
            await pipeline.ExecuteAsync(_ => new ValueTask<Result>(new Result(IsFailed: i >= 5, IsIgnored: i < 5)), Token);
        }

        _stateProvider.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task ShouldIgnore_IgnoredOutcomesNotCountedTowardsThroughput()
    {
        var pipeline = CreatePipeline(ignore: true);

        for (int i = 0; i < 10; i++)
        {
            await pipeline.ExecuteAsync(_ => new ValueTask<Result>(new Result(IsFailed: i >= 5, IsIgnored: i < 5)), Token);
        }

        _stateProvider.CircuitState.ShouldBe(CircuitState.Closed);

        for (int i = 0; i < 5; i++)
        {
            await pipeline.ExecuteAsync(_ => new ValueTask<Result>(Failure), Token);
        }

        _stateProvider.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public void ShouldIgnore_Synchronous_IgnoredOutcomesNotCountedTowardsThroughput()
    {
        var pipeline = CreatePipeline(ignore: true);

        for (int i = 0; i < 10; i++)
        {
            pipeline.Execute(_ => new Result(IsFailed: i >= 5, IsIgnored: i < 5), Token);
        }

        _stateProvider.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task ShouldIgnore_NonGeneric_IgnoredOutcomesNotCountedTowardsThroughput()
    {
        var pipeline = new ResiliencePipelineBuilder { TimeProvider = _timeProvider }
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 10,
                StateProvider = _stateProvider,
                ShouldHandle = args => new(args.Outcome.Exception is InvalidOperationException),
                ShouldIgnore = args => new(args.Outcome.Exception is OperationCanceledException),
            })
            .Build();

        for (int i = 0; i < 10; i++)
        {
            Exception exception = i < 5 ? new OperationCanceledException() : new InvalidOperationException();
            await Should.ThrowAsync<Exception>(async () => await pipeline.ExecuteAsync(_ => throw exception, Token));
        }

        _stateProvider.CircuitState.ShouldBe(CircuitState.Closed);

        for (int i = 0; i < 5; i++)
        {
            await Should.ThrowAsync<InvalidOperationException>(async () => await pipeline.ExecuteAsync(_ => throw new InvalidOperationException(), Token));
        }

        _stateProvider.CircuitState.ShouldBe(CircuitState.Open);
    }

    [Fact]
    public async Task ShouldIgnore_IgnoredProbe_ReturnsToOpenAndNextCallProbesImmediately()
    {
        var pipeline = CreatePipeline(ignore: true);
        await OpenCircuitAsync(pipeline);
        _timeProvider.Advance(BreakDuration);

        await pipeline.ExecuteAsync(_ => new ValueTask<Result>(Ignored), Token);
        _stateProvider.CircuitState.ShouldBe(CircuitState.Open);

        // Clock hasn't moved, but the next call should still get to probe.
        var executed = false;
        await pipeline.ExecuteAsync(
            _ =>
            {
                executed = true;
                return new ValueTask<Result>(Failure);
            },
            Token);

        executed.ShouldBeTrue();
        _stateProvider.CircuitState.ShouldBe(CircuitState.Open);

        // 0 = first break, 1 = the failed probe. The ignored probe doesn't count.
        _halfOpenAttempts.ShouldBe([0, 1]);
    }

    [Fact]
    public async Task ShouldIgnore_IgnoredProbe_NextSuccessfulCallClosesCircuit()
    {
        var pipeline = CreatePipeline(ignore: true);
        await OpenCircuitAsync(pipeline);
        _timeProvider.Advance(BreakDuration);

        await pipeline.ExecuteAsync(_ => new ValueTask<Result>(Ignored), Token);
        await pipeline.ExecuteAsync(_ => new ValueTask<Result>(Success), Token);

        _stateProvider.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task ShouldIgnore_IgnoredWhileOpen_DoesNotChangeBreakDuration()
    {
        var pipeline = CreatePipeline(ignore: true);

        var staleGate = new TaskCompletionSource<Result>();
        var stale = ExecuteHeldAsync(pipeline, staleGate);

        await OpenCircuitAsync(pipeline);
        _timeProvider.Advance(BreakDuration / 2);

        staleGate.SetResult(Ignored);
        await stale;
        _stateProvider.CircuitState.ShouldBe(CircuitState.Open);

        var rejected = await ExecuteOutcomeAsync(pipeline, Success);
        rejected.Exception.ShouldBeOfType<BrokenCircuitException>();

        _timeProvider.Advance(BreakDuration / 2);
        await pipeline.ExecuteAsync(_ => new ValueTask<Result>(Success), Token);
        _stateProvider.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task ShouldIgnore_IgnoredWhileIsolated_StaysIsolated()
    {
        var manualControl = new CircuitBreakerManualControl();
        var pipeline = CreatePipeline(ignore: true, manualControl: manualControl);

        var staleGate = new TaskCompletionSource<Result>();
        var stale = ExecuteHeldAsync(pipeline, staleGate);

        await manualControl.IsolateAsync(Token);
        staleGate.SetResult(Ignored);
        await stale;

        _stateProvider.CircuitState.ShouldBe(CircuitState.Isolated);
    }

    [Fact]
    public async Task ShouldIgnore_StaleIgnoredOutcomeDuringProbe_ReturnsToOpenAndDropsProbeResult()
    {
        // Known limitation: the controller can't tell which call is the probe, so an ignored result
        // from a call that started before the circuit broke kicks it out of half-open.
        var pipeline = CreatePipeline(ignore: true);

        var staleGate = new TaskCompletionSource<Result>();
        var stale = ExecuteHeldAsync(pipeline, staleGate);

        await OpenCircuitAsync(pipeline);
        _timeProvider.Advance(BreakDuration);

        var probeGate = new TaskCompletionSource<Result>();
        var probe = ExecuteHeldAsync(pipeline, probeGate);
        _stateProvider.CircuitState.ShouldBe(CircuitState.HalfOpen);

        staleGate.SetResult(Ignored);
        await stale;
        _stateProvider.CircuitState.ShouldBe(CircuitState.Open);

        probeGate.SetResult(Success);
        await probe;
        _stateProvider.CircuitState.ShouldBe(CircuitState.Open);

        await pipeline.ExecuteAsync(_ => new ValueTask<Result>(Success), Token);
        _stateProvider.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task ShouldIgnore_Null_StaleSuccessDuringProbe_ClosesCircuit()
    {
        var pipeline = CreatePipeline(ignore: false);

        var staleGate = new TaskCompletionSource<Result>();
        var stale = ExecuteHeldAsync(pipeline, staleGate);

        await OpenCircuitAsync(pipeline);
        _timeProvider.Advance(BreakDuration);

        var probeGate = new TaskCompletionSource<Result>();
        var probe = ExecuteHeldAsync(pipeline, probeGate);

        staleGate.SetResult(Success);
        await stale;
        _stateProvider.CircuitState.ShouldBe(CircuitState.Closed);

        probeGate.SetResult(Failure);
        await probe;
    }

    [Fact]
    public async Task ShouldIgnore_Null_CancelledProbe_ClosesCircuit()
    {
        var pipeline = CreatePipeline(
            ignore: false,
            shouldHandle: args => new(args.Outcome.Exception is { } and not OperationCanceledException || args.Outcome.Result?.IsFailed == true));

        await OpenCircuitAsync(pipeline);
        _timeProvider.Advance(BreakDuration);

        var outcome = await pipeline.ExecuteOutcomeAsync<Result, string>(
            (_, _) => Outcome.FromExceptionAsValueTask<Result>(new OperationCanceledException()),
            ResilienceContextPool.Shared.Get(Token),
            string.Empty);

        outcome.Exception.ShouldBeOfType<OperationCanceledException>();
        _stateProvider.CircuitState.ShouldBe(CircuitState.Closed);
    }

    private static Result Success => new(IsFailed: false, IsIgnored: false);

    private static Result Failure => new(IsFailed: true, IsIgnored: false);

    private static Result Ignored => new(IsFailed: true, IsIgnored: true);

    private static async Task OpenCircuitAsync(ResiliencePipeline<Result> pipeline)
    {
        for (int i = 0; i < 10; i++)
        {
            await pipeline.ExecuteAsync(_ => new ValueTask<Result>(Failure), Token);
        }
    }

    private static async Task ExecuteHeldAsync(ResiliencePipeline<Result> pipeline, TaskCompletionSource<Result> gate)
        => await pipeline.ExecuteAsync(async _ => await gate.Task, Token);

    private static async ValueTask<Outcome<Result>> ExecuteOutcomeAsync(ResiliencePipeline<Result> pipeline, Result result)
    {
        var context = ResilienceContextPool.Shared.Get(Token);
        try
        {
            return await pipeline.ExecuteOutcomeAsync(static (_, r) => Outcome.FromResultAsValueTask(r), context, result);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    private ResiliencePipeline<Result> CreatePipeline(
        bool ignore,
        Func<CircuitBreakerPredicateArguments<Result>, ValueTask<bool>>? shouldHandle = null,
        CircuitBreakerManualControl? manualControl = null)
    {
        return new ResiliencePipelineBuilder<Result> { TimeProvider = _timeProvider }
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<Result>
            {
                SamplingDuration = TimeSpan.FromSeconds(30),
                FailureRatio = 0.5,
                MinimumThroughput = 10,
                BreakDuration = BreakDuration,
                StateProvider = _stateProvider,
                ManualControl = manualControl,
                BreakDurationGenerator = args =>
                {
                    _halfOpenAttempts.Add(args.HalfOpenAttempts);
                    return new ValueTask<TimeSpan>(BreakDuration);
                },
                ShouldHandle = shouldHandle ?? (args => new(args.Outcome.Exception is null && args.Outcome.Result?.IsFailed == true)),
                ShouldIgnore = ignore ? args => new(args.Outcome.Result?.IsIgnored == true) : null,
            })
            .Build();
    }

    public sealed record Result(bool IsFailed, bool IsIgnored);
}
