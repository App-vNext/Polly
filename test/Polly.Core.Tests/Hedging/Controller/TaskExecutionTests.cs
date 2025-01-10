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
    private readonly HedgingHandler<DisposableResult> _hedgingHandler;
    private readonly CancellationTokenSource _cts;
    private readonly HedgingTimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly List<ExecutionAttemptArguments> _args = [];
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
                state.Should().Be("dummy-state");
                return Outcome.FromResultAsValueTask(new DisposableResult { Name = value });
            },
            "dummy-state",
            99);

        await execution.ExecutionTaskSafe!;
        execution.Outcome.Result!.Name.Should().Be(value);
        execution.IsHandled.Should().Be(handled);
        AssertContext(execution.Context);

        _args.Should().HaveCount(1);
        _args[0].Handled.Should().Be(handled);
        _args[0].AttemptNumber.Should().Be(99);
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

        execution.Outcome.Exception.Should().BeOfType<InvalidOperationException>();
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
            args.AttemptNumber.Should().Be(4);
            return () => Outcome.FromResultAsValueTask(new DisposableResult { Name = value });
        };

        (await execution.InitializeAsync<string>(HedgedTaskType.Secondary, _primaryContext, null!, "dummy-state", 4)).Should().BeTrue();

        await execution.ExecutionTaskSafe!;

        execution.Outcome.Result!.Name.Should().Be(value);
        execution.IsHandled.Should().Be(handled);
        AssertContext(execution.Context);
    }

    [Fact]
    public async Task Initialize_SecondaryWhenTaskGeneratorReturnsNull_Ok()
    {
        var execution = Create();
        Generator = args => null;

        (await execution.InitializeAsync<string>(HedgedTaskType.Secondary, _primaryContext, null!, "dummy-state", 4)).Should().BeFalse();

        execution.Invoking(e => e.Context).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Cancel_Accepted_NoEffect()
    {
        var execution = Create();
        var cancelled = false;

        await InitializePrimaryAsync(execution, onContext: context => context.CancellationToken.Register(() => cancelled = true));

        execution.AcceptOutcome();
        execution.Cancel();

        cancelled.Should().BeFalse();
    }

    [Fact]
    public async Task Cancel_NotAccepted_Cancelled()
    {
        var execution = Create();
        var cancelled = false;

        await InitializePrimaryAsync(execution, onContext: context => context.CancellationToken.Register(() => cancelled = true));

        execution.Cancel();
        cancelled.Should().BeTrue();
    }

    [Fact]
    public async Task Initialize_SecondaryWhenTaskGeneratorThrows_EnsureOutcome()
    {
        var execution = Create();
        Generator = args => throw new FormatException();

        (await execution.InitializeAsync<string>(HedgedTaskType.Secondary, _primaryContext, null!, "dummy-state", 4)).Should().BeTrue();

        await execution.ExecutionTaskSafe!;
        execution.Outcome.Exception.Should().BeOfType<FormatException>();
    }

    [Fact]
    public async Task Initialize_ExecutionTaskDoesNotThrows()
    {
        var execution = Create();
        Generator = args => throw new FormatException();

        (await execution.InitializeAsync<string>(HedgedTaskType.Secondary, _primaryContext, null!, "dummy-state", 4)).Should().BeTrue();

        await execution.ExecutionTaskSafe!.Invoking(async t => await t).Should().NotThrowAsync();
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
        execution.Outcome.Exception.Should().BeAssignableTo<OperationCanceledException>();
    }

    [Fact]
    public void AcceptOutcome_NotInitialized_Throws()
    {
        var execution = Create();

        execution.Invoking(e => e.AcceptOutcome()).Should().Throw<InvalidOperationException>();
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
        execution.IsAccepted.Should().BeFalse();
        execution.IsHandled.Should().BeFalse();
        context.Properties.Options.Should().HaveCount(0);
        execution.Invoking(e => e.Context).Should().Throw<InvalidOperationException>();
#if NETFRAMEWORK
        token.Invoking(t => t.WaitHandle).Should().Throw<InvalidOperationException>();
#endif
        if (accept)
        {
            result.IsDisposed.Should().BeFalse();
        }
        else
        {
            result.IsDisposed.Should().BeTrue();
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
        context.Should().NotBeSameAs(_primaryContext);
        context.Properties.Should().NotBeSameAs(_primaryContext.Properties);
        context.CancellationToken.Should().NotBeSameAs(_primaryContext.CancellationToken);
        context.CancellationToken.CanBeCanceled.Should().BeTrue();

        context.Properties.Options.Should().HaveCount(1);
        context.Properties.TryGetValue(_myKey, out var value).Should().BeTrue();
        value.Should().Be("dummy-value");
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
