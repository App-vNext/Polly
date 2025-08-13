using Polly.Hedging;
using Polly.Hedging.Controller;
using Polly.Hedging.Utils;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Hedging.Controller;

public class TaskExecutionTests : IDisposable
{
    private const string Handled = "Handled";
    private readonly ResiliencePropertyKey<string> _myKey = new("my-key");
    private readonly CancellationTokenSource _cts;
    private readonly HedgingTimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly List<ExecutionAttemptArguments> _args = [];
    private HedgingHandler<DisposableResult> _hedgingHandler;
    private ResilienceContext _primaryContext;

    public TaskExecutionTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(args =>
        {
            if (args.Arguments is ExecutionAttemptArguments attempt)
            {
                _args.Add(attempt);
            }
        });

        _timeProvider = new HedgingTimeProvider();
        _cts = new CancellationTokenSource();
        _primaryContext = ResilienceContextPool.Shared.Get(_cts.Token);
        _hedgingHandler = HedgingHelper.CreateHandler<DisposableResult>(outcome => outcome switch
        {
            { Exception: ApplicationException } => true,
            { Result: DisposableResult result } when result.Name == Handled => true,
            _ => false
        }, args => Generator(args));

        CreateSnapshot();
    }

    public void Dispose() => _cts.Dispose();

    [InlineData(Handled, true)]
    [InlineData("Unhandled", false)]
    [Theory]
    public async Task Initialize_Primary_Ok(string value, bool handled)
    {
        var execution = Create();
        await execution.InitializeAsync(HedgedTaskType.Primary, _primaryContext,
            (context, state) =>
            {
                AssertContext(context);
                state.ShouldBe("dummy-state");
                return Outcome.FromResultAsValueTask(new DisposableResult { Name = value });
            },
            "dummy-state",
            99);

        await execution.ExecutionTaskSafe!;
        execution.Outcome.Result!.Name.ShouldBe(value);
        execution.IsHandled.ShouldBe(handled);
        AssertContext(execution.Context);

        _args.Count.ShouldBe(1);
        _args[0].Handled.ShouldBe(handled);
        _args[0].AttemptNumber.ShouldBe(99);
    }

    [Fact]
    public async Task Initialize_PrimaryCallbackThrows_EnsureExceptionHandled()
    {
        var execution = Create();
        await execution.InitializeAsync<string>(HedgedTaskType.Primary, _primaryContext,
            (_, _) => throw new InvalidOperationException(),
            "dummy-state",
            1);

        await execution.ExecutionTaskSafe!;

        execution.Outcome.Exception.ShouldBeOfType<InvalidOperationException>();
    }

    [InlineData(Handled, true)]
    [InlineData("Unhandled", false)]
    [Theory]
    public async Task Initialize_Secondary_Ok(string value, bool handled)
    {
        var execution = Create();
        Generator = args =>
        {
            AssertContext(args.ActionContext);
            args.AttemptNumber.ShouldBe(4);
            return () => Outcome.FromResultAsValueTask(new DisposableResult { Name = value });
        };

        (await execution.InitializeAsync<string>(HedgedTaskType.Secondary, _primaryContext, null!, "dummy-state", 4)).ShouldBeTrue();

        await execution.ExecutionTaskSafe!;

        execution.Outcome.Result!.Name.ShouldBe(value);
        execution.IsHandled.ShouldBe(handled);
        AssertContext(execution.Context);
    }

    [InlineData(false)]
    [InlineData(true)]
    [Theory]
    public async Task Initialize_Secondary_DefaultHandler_Ok(bool sync)
    {
        _hedgingHandler = HedgingHelper.CreateHandler(_ => false, new HedgingStrategyOptions<DisposableResult>().ActionGenerator);
        _primaryContext.Initialize<string>(sync);
        var execution = Create();

        Func<ResilienceContext, string, ValueTask<Outcome<DisposableResult>>> primaryCallback = (context, state) =>
        {
            AssertContext(context);
            state.ShouldBe("dummy-state");
            return Outcome.FromResultAsValueTask(new DisposableResult { Name = "Unhandled" });
        };

        (await execution.InitializeAsync(HedgedTaskType.Secondary, _primaryContext, primaryCallback, "dummy-state", 4)).ShouldBeTrue();

        await execution.ExecutionTaskSafe!;

        execution.Outcome.Result!.Name.ShouldBe("Unhandled");
        execution.IsHandled.ShouldBe(false);
        AssertContext(execution.Context);
    }

    [Fact]
    public async Task Initialize_SecondaryWhenTaskGeneratorReturnsNull_Ok()
    {
        var execution = Create();
        Generator = args => null;

        (await execution.InitializeAsync(HedgedTaskType.Secondary, _primaryContext, null!, "dummy-state", 4)).ShouldBeFalse();

        Should.Throw<InvalidOperationException>(() => execution.Context);
    }

    [Fact]
    public async Task Cancel_Accepted_NoEffect()
    {
        var execution = Create();
        var cancelled = false;

        await InitializePrimaryAsync(execution, onContext: context => context.CancellationToken.Register(() => cancelled = true));

        execution.AcceptOutcome();
        execution.Cancel();

        cancelled.ShouldBeFalse();
    }

    [Fact]
    public async Task Cancel_NotAccepted_Cancelled()
    {
        var execution = Create();
        var cancelled = false;

        await InitializePrimaryAsync(execution, onContext: context => context.CancellationToken.Register(() => cancelled = true));

        execution.Cancel();
        cancelled.ShouldBeTrue();
    }

    [Fact]
    public async Task Initialize_SecondaryWhenTaskGeneratorThrows_EnsureOutcome()
    {
        var execution = Create();
        Generator = args => throw new FormatException();

        (await execution.InitializeAsync<string>(HedgedTaskType.Secondary, _primaryContext, null!, "dummy-state", 4)).ShouldBeTrue();

        await execution.ExecutionTaskSafe!;
        execution.Outcome.Exception.ShouldBeOfType<FormatException>();
    }

    [Fact]
    public async Task Initialize_ExecutionTaskDoesNotThrows()
    {
        var execution = Create();
        Generator = args => throw new FormatException();

        (await execution.InitializeAsync<string>(HedgedTaskType.Secondary, _primaryContext, null!, "dummy-state", 4)).ShouldBeTrue();

        await Should.NotThrowAsync(async () => await execution.ExecutionTaskSafe!);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Initialize_Cancelled_EnsureRespected(bool primary)
    {
        // arrange
        Generator = (args) =>
        {
            return async () =>
            {
                await _timeProvider.Delay(TimeSpan.FromDays(1), args.ActionContext.CancellationToken);
                return Outcome.FromResult(new DisposableResult { Name = Handled });
            };
        };

        var execution = Create();

        await execution.InitializeAsync(primary ? HedgedTaskType.Primary : HedgedTaskType.Secondary, _primaryContext,
            async (context, _) =>
            {
                try
                {
                    await _timeProvider.Delay(TimeSpan.FromDays(1), context.CancellationToken);
                    return Outcome.FromResult(new DisposableResult());
                }
                catch (OperationCanceledException e)
                {
                    return Outcome.FromException<DisposableResult>(e);
                }
            },
            "dummy-state",
            1);

        // act
        _cts.Cancel();
        await execution.ExecutionTaskSafe!;

        // assert
        execution.Outcome.Exception.ShouldBeAssignableTo<OperationCanceledException>();
    }

    [Fact]
    public void AcceptOutcome_NotInitialized_Throws()
    {
        var execution = Create();

        Should.Throw<InvalidOperationException>(() => execution.AcceptOutcome());
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task ResetAsync_Ok(bool accept)
    {
        // arrange
        using var result = new DisposableResult();
        var execution = Create();
        var token = default(CancellationToken);
        await InitializePrimaryAsync(execution, result, context => token = context.CancellationToken);
        await execution.ExecutionTaskSafe!;
        var context = execution.Context;

        if (accept)
        {
            execution.AcceptOutcome();
        }

        // act
        await execution.ResetAsync();

        // assert
        execution.IsAccepted.ShouldBeFalse();
        execution.IsHandled.ShouldBeFalse();
        context.Properties.Options.ShouldBeEmpty();
        Should.Throw<InvalidOperationException>(() => execution.Context);
#if NETFRAMEWORK
        Should.Throw<InvalidOperationException>(() => token.WaitHandle);
#endif
        if (accept)
        {
            result.IsDisposed.ShouldBeFalse();
        }
        else
        {
            result.IsDisposed.ShouldBeTrue();
        }
    }

    private async Task InitializePrimaryAsync(TaskExecution<DisposableResult> execution, DisposableResult? result = null, Action<ResilienceContext>? onContext = null) =>
        await execution.InitializeAsync(HedgedTaskType.Primary, _primaryContext, (context, _) =>
        {
            onContext?.Invoke(context);
            return Outcome.FromResultAsValueTask(result ?? new DisposableResult { Name = Handled });
        }, "dummy-state", 1);

    private void AssertContext(ResilienceContext context)
    {
        context.ShouldNotBeSameAs(_primaryContext);
        context.Properties.ShouldNotBeSameAs(_primaryContext.Properties);
        context.CancellationToken.ShouldNotBeSameAs(_primaryContext.CancellationToken);
        context.CancellationToken.CanBeCanceled.ShouldBeTrue();

        context.Properties.Options.Count.ShouldBe(1);
        context.Properties.TryGetValue(_myKey, out var value).ShouldBeTrue();
        value.ShouldBe("dummy-value");
    }

    private void CreateSnapshot(CancellationToken? token = null)
    {
        _primaryContext = ResilienceContextPool.Shared.Get(token ?? _cts.Token);
        _primaryContext.Properties.Set(_myKey, "dummy-value");
    }

    private Func<HedgingActionGeneratorArguments<DisposableResult>, Func<ValueTask<Outcome<DisposableResult>>>?> Generator { get; set; } = args =>
    {
        return () => Outcome.FromResultAsValueTask(new DisposableResult { Name = Handled });
    };

    private TaskExecution<DisposableResult> Create() => new(_hedgingHandler, CancellationTokenSourcePool.Create(TimeProvider.System), _timeProvider, _telemetry);
}
