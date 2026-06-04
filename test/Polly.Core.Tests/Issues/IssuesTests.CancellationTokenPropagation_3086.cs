using Polly.Hedging;
using Polly.Timeout;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    // https://github.com/App-vNext/Polly/issues/3086
    // When a strategy substitutes the caller's CancellationToken with an internal one (timeout, hedging),
    // a caller-initiated cancellation must still surface an OperationCanceledException carrying the
    // caller's token, so that callers can distinguish caller cancellation from other failures.
    [Fact]
    public async Task Timeout_CallerCancellation_ExceptionCarriesCallerToken_3086()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromMinutes(1))
            .Build();

        using var cts = new CancellationTokenSource();

        var exception = await Should.ThrowAsync<OperationCanceledException>(() =>
            pipeline.ExecuteAsync(
                async token =>
                {
                    await cts.CancelAsync(); // simulate cancellation request from upstream caller
                    token.ThrowIfCancellationRequested(); // simulate cancellation response from downstream code
                },
                cts.Token).AsTask());

        exception.CancellationToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task TimeoutThenRetry_CallerCancellation_ExceptionCarriesCallerToken_3086()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromMinutes(1))
            .AddRetry(new() { MaxRetryAttempts = 3, Delay = TimeSpan.Zero })
            .Build();

        using var cts = new CancellationTokenSource();

        var exception = await Should.ThrowAsync<OperationCanceledException>(() =>
            pipeline.ExecuteAsync(
                async token =>
                {
                    await cts.CancelAsync();
                    token.ThrowIfCancellationRequested();
                },
                cts.Token).AsTask());

        exception.CancellationToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task RetryThenTimeout_CallerCancellation_ExceptionCarriesCallerToken_3086()
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new() { MaxRetryAttempts = 3, Delay = TimeSpan.Zero })
            .AddTimeout(TimeSpan.FromMinutes(1))
            .Build();

        using var cts = new CancellationTokenSource();

        var exception = await Should.ThrowAsync<OperationCanceledException>(() =>
            pipeline.ExecuteAsync(
                async token =>
                {
                    await cts.CancelAsync();
                    token.ThrowIfCancellationRequested();
                },
                cts.Token).AsTask());

        exception.CancellationToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task NestedTimeouts_CallerCancellation_ExceptionCarriesCallerToken_3086()
    {
        var innermost = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromMinutes(1))
            .Build();

        var inner = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromMinutes(2))
            .AddPipeline(innermost)
            .Build();

        var pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromMinutes(3))
            .AddPipeline(inner)
            .Build();

        using var cts = new CancellationTokenSource();

        var exception = await Should.ThrowAsync<OperationCanceledException>(() =>
            pipeline.ExecuteAsync(
                async token =>
                {
                    await cts.CancelAsync();
                    token.ThrowIfCancellationRequested();
                },
                cts.Token).AsTask());

        exception.CancellationToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task Hedging_CallerCancellation_ExceptionCarriesCallerToken_3086()
    {
        var pipeline = new ResiliencePipelineBuilder<string>()
            .AddHedging(new HedgingStrategyOptions<string>
            {
                MaxHedgedAttempts = 1,
                Delay = TimeSpan.FromMinutes(1), // ensure only the primary attempt runs before cancellation
                ShouldHandle = _ => PredicateResult.False(), // accept the primary outcome as-is
            })
            .Build();

        using var cts = new CancellationTokenSource();

        var exception = await Should.ThrowAsync<OperationCanceledException>(() =>
            pipeline.ExecuteAsync(
                async token =>
                {
                    await cts.CancelAsync();
                    token.ThrowIfCancellationRequested();
                    return "unreachable";
                },
                cts.Token).AsTask());

        exception.CancellationToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task WithoutTokenSubstitution_CallerCancellation_ExceptionCarriesCallerToken_3086()
    {
        // A pipeline that does not substitute the token has always surfaced the caller's token
        // and this case simply guards against regressions for the common path.
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new() { MaxRetryAttempts = 3, Delay = TimeSpan.Zero })
            .Build();

        using var cts = new CancellationTokenSource();

        var exception = await Should.ThrowAsync<OperationCanceledException>(() =>
            pipeline.ExecuteAsync(
                async token =>
                {
                    await cts.CancelAsync();
                    token.ThrowIfCancellationRequested();
                },
                cts.Token).AsTask());

        exception.CancellationToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task Timeout_RealTimeout_StillThrowsTimeoutRejectedException_3086()
    {
        // "only if": a real timeout (caller token not cancelled) must remain a TimeoutRejectedException,
        // and must not be rewritten into a caller-cancellation.
        var pipeline = new ResiliencePipelineBuilder { TimeProvider = TimeProvider }
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        await Should.ThrowAsync<TimeoutRejectedException>(() =>
            pipeline.ExecuteAsync(
                async token =>
                {
                    var delay = Task.Delay(TimeSpan.FromSeconds(10), TimeProvider, token);
                    TimeProvider.Advance(TimeSpan.FromSeconds(2));
                    await delay;
                },
                CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Timeout_UnrelatedCancellation_ExceptionPreserved_3086()
    {
        // "only if": when the caller token is not cancelled, an unrelated OperationCanceledException
        // thrown by user code must propagate unchanged (its own token must be preserved).
        var pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromMinutes(1))
            .Build();

        using var callerCts = new CancellationTokenSource();
        using var unrelatedCts = new CancellationTokenSource();
        await unrelatedCts.CancelAsync();

        var exception = await Should.ThrowAsync<OperationCanceledException>(() =>
            pipeline.ExecuteAsync(
                async token =>
                {
                    await Task.Yield();
                    throw new OperationCanceledException(unrelatedCts.Token);
                },
                callerCts.Token).AsTask());

        exception.CancellationToken.ShouldBe(unrelatedCts.Token);
    }
}
