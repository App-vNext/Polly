using System.Globalization;
using Polly.Hedging;
using Polly.Hedging.Controller;
using Polly.Hedging.Utils;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Hedging.Controller;

public class HedgingExecutionContextTests : IDisposable
{
    private const string Handled = "Handled";
    private static readonly TimeSpan AssertTimeout = TimeSpan.FromSeconds(5);
    private readonly ResiliencePropertyKey<string> _myKey = new("my-key");
    private readonly CancellationTokenSource _cts;
    private readonly HedgingTimeProvider _timeProvider;
    private readonly List<TaskExecution<DisposableResult>> _createdExecutions = [];
    private readonly List<TaskExecution<DisposableResult>> _returnedExecutions = [];
    private readonly List<HedgingExecutionContext<DisposableResult>> _resets = [];
    private readonly ResilienceContext _resilienceContext;
    private readonly AutoResetEvent _onReset = new(false);
    private int _maxAttempts = 2;
    private HedgingHandler<DisposableResult> _hedgingHandler;

    public HedgingExecutionContextTests()
    {
        _timeProvider = new HedgingTimeProvider();
        _cts = new CancellationTokenSource();
        _hedgingHandler = HedgingHelper.CreateHandler<DisposableResult>(outcome => outcome switch
        {
            { Exception: ApplicationException } => true,
            { Result: DisposableResult result } when result.Name == Handled => true,
            _ => false
        },
        args => Generator(args));
        _resilienceContext = ResilienceContextPool.Shared.Get().Initialize<string>(false);
        _resilienceContext.CancellationToken = _cts.Token;
        _resilienceContext.Properties.Set(_myKey, "dummy");

        // HedgingExecutionContext has some Debug.Assert pieces which trigger failures in Debug mode
        // just suppress these
        Trace.Listeners.Clear();
    }

    public void Dispose()
    {
        _cts.Dispose();
        _onReset.Dispose();
    }

    [Fact]
    public void Ctor_Ok()
    {
        var context = Create();

        context.LoadedTasks.Should().Be(0);
        context.PrimaryContext.Should().BeNull();

        context.Should().NotBeNull();
    }

    [Fact]
    public void Initialize_Ok()
    {
        var props = _resilienceContext.Properties;
        var context = Create();

        context.Initialize(_resilienceContext);

        context.PrimaryContext.Should().Be(_resilienceContext);
        context.IsInitialized.Should().BeTrue();
    }

    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [Theory]
    public async Task TryWaitForCompletedExecutionAsync_Initialized_Ok(int delay)
    {
        var context = Create();
        context.Initialize(_resilienceContext);
        var delayTimeSpan = TimeSpan.FromSeconds(delay);

        var task = context.TryWaitForCompletedExecutionAsync(delayTimeSpan);

        _timeProvider.Advance(TimeSpan.FromHours(1));

        (await task).Should().BeNull();
    }

    [Fact]
    public async Task TryWaitForCompletedExecutionAsync_FinishedTask_Ok()
    {
        var context = Create();
        context.Initialize(_resilienceContext);
        await context.LoadExecutionAsync((_, _) => Outcome.FromResultAsValueTask(new DisposableResult("dummy")), "state");

        var task = await context.TryWaitForCompletedExecutionAsync(TimeSpan.Zero);

        task.Should().NotBeNull();
        task!.ExecutionTaskSafe!.IsCompleted.Should().BeTrue();

        task.Outcome.Result!.Name.Should().Be("dummy");
        task.AcceptOutcome();
        context.LoadedTasks.Should().Be(1);
    }

    [Fact]
    public async Task TryWaitForCompletedExecutionAsync_ConcurrentExecution_Ok()
    {
        var context = Create();
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(TimeSpan.FromHours(1), TimeSpan.FromHours(1));

        for (int i = 0; i < _maxAttempts - 1; i++)
        {
            await LoadExecutionAsync(context, TimeSpan.FromHours(1));
        }

        for (int i = 0; i < _maxAttempts; i++)
        {
            (await context.TryWaitForCompletedExecutionAsync(TimeSpan.Zero)).Should().BeNull();
        }

        _timeProvider.Advance(TimeSpan.FromDays(1));
        await context.TryWaitForCompletedExecutionAsync(TimeSpan.Zero);
        await context.Tasks[0].ExecutionTaskSafe!;
        context.Tasks[0].AcceptOutcome();
    }

    [Fact]
    public async Task TryWaitForCompletedExecutionAsync_SynchronousExecution_Ok()
    {
        var context = Create();
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(TimeSpan.FromHours(1), TimeSpan.FromHours(1));

        for (int i = 0; i < _maxAttempts - 1; i++)
        {
            await LoadExecutionAsync(context, TimeSpan.FromHours(1));
        }

        var task = context.TryWaitForCompletedExecutionAsync(System.Threading.Timeout.InfiniteTimeSpan).AsTask();
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        task.Wait(20).Should().BeFalse();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
        _timeProvider.Advance(TimeSpan.FromDays(1));
        await task;
        context.Tasks[0].AcceptOutcome();
    }

    [Fact]
    public async Task TryWaitForCompletedExecutionAsync_HedgedExecution_Ok()
    {
        var context = Create();
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(TimeSpan.FromHours(1), TimeSpan.FromHours(1));

        for (int i = 0; i < _maxAttempts - 1; i++)
        {
            await LoadExecutionAsync(context, TimeSpan.FromHours(1));
        }

        var hedgingDelay = TimeSpan.FromSeconds(5);
        var count = _timeProvider.TimerEntries.Count;
        var task = context.TryWaitForCompletedExecutionAsync(hedgingDelay).AsTask();
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        task.Wait(20).Should().BeFalse();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
        _timeProvider.TimerEntries.Should().HaveCount(count + 1);
        _timeProvider.TimerEntries.Last().Delay.Should().Be(hedgingDelay);
        _timeProvider.Advance(TimeSpan.FromDays(1));
        await task;
        await context.Tasks[0].ExecutionTaskSafe!;
        context.Tasks[0].AcceptOutcome();
    }

    [Fact]
    public async Task TryWaitForCompletedExecutionAsync_TwiceWhenSecondaryGeneratorNotRegistered_Ok()
    {
        _hedgingHandler = HedgingHelper.CreateHandler<DisposableResult>(_ => true, args => null);

        var context = Create();
        context.Initialize(_resilienceContext);
        await context.LoadExecutionAsync((_, _) => Outcome.FromResultAsValueTask(new DisposableResult("dummy")), "state");
        await context.LoadExecutionAsync((_, _) => Outcome.FromResultAsValueTask(new DisposableResult("dummy")), "state");

        var task = await context.TryWaitForCompletedExecutionAsync(TimeSpan.Zero);

        task!.AcceptOutcome();
        context.LoadedTasks.Should().Be(1);
    }

    [Fact]
    public async Task TryWaitForCompletedExecutionAsync_TwiceWhenSecondaryGeneratorRegistered_Ok()
    {
        var context = Create();
        context.Initialize(_resilienceContext);
        await LoadExecutionAsync(context);
        await LoadExecutionAsync(context);

        Generator = args => () => Outcome.FromResultAsValueTask(new DisposableResult { Name = "secondary" });

        var task = await context.TryWaitForCompletedExecutionAsync(TimeSpan.Zero);
        task!.Type.Should().Be(HedgedTaskType.Primary);
        task!.AcceptOutcome();
        context.LoadedTasks.Should().Be(2);
        context.Tasks[0].Type.Should().Be(HedgedTaskType.Primary);
        context.Tasks[1].Type.Should().Be(HedgedTaskType.Secondary);
    }

    [Fact]
    public async Task LoadExecutionAsync_MaxTasks_NoMoreTasksAdded()
    {
        _maxAttempts = 3;
        var context = Create();
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.FromHours(1));

        for (int i = 0; i < _maxAttempts; i++)
        {
            (await LoadExecutionAsync(context)).Loaded.Should().BeTrue();
        }

        (await LoadExecutionAsync(context)).Loaded.Should().BeFalse();

        context.LoadedTasks.Should().Be(_maxAttempts);
        context.Tasks[0].AcceptOutcome();
        _returnedExecutions.Should().HaveCount(0);
    }

    [Fact]
    public async Task LoadExecutionAsync_EnsureCorrectAttemptNumber()
    {
        var attempt = -1;
        var context = Create();
        context.Initialize(_resilienceContext);
        Generator = args =>
        {
            attempt = args.AttemptNumber;
            return null;
        };

        await LoadExecutionAsync(context);
        await LoadExecutionAsync(context);

        // primary is 0, this one is 1
        attempt.Should().Be(1);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task LoadExecutionAsync_NoMoreSecondaryTasks_AcceptFinishedOutcome(bool allExecuted)
    {
        _maxAttempts = 4;
        var context = Create();
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(allExecuted ? TimeSpan.Zero : TimeSpan.FromHours(1));

        // primary
        await LoadExecutionAsync(context);

        // secondary
        await LoadExecutionAsync(context);

        // secondary couldn't be created
        if (allExecuted)
        {
            await context.TryWaitForCompletedExecutionAsync(TimeSpan.Zero);
            await context.TryWaitForCompletedExecutionAsync(TimeSpan.Zero);
        }

        var pair = await LoadExecutionAsync(context);
        pair.Loaded.Should().BeFalse();

        _returnedExecutions.Count.Should().Be(1);
        if (allExecuted)
        {
            pair.Outcome.Should().NotBeNull();
            context.Tasks[0].IsAccepted.Should().BeTrue();
        }
        else
        {
            pair.Outcome.Should().BeNull();
            context.Tasks[0].IsAccepted.Should().BeFalse();
        }

        context.Tasks[0].AcceptOutcome();
    }

    [Fact]
    public async Task LoadExecution_NoMoreTasks_Throws()
    {
        _maxAttempts = 0;
        var context = Create();
        context.Initialize(_resilienceContext);

        await context.Invoking(c => LoadExecutionAsync(c)).Should().ThrowAsync<InvalidOperationException>();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Complete_EnsureOriginalContextPreparedWithAcceptedOutcome(bool primary)
    {
        // arrange
        var type = primary ? HedgedTaskType.Primary : HedgedTaskType.Secondary;
        var context = Create();
        var originalProps = _resilienceContext.Properties;
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(TimeSpan.Zero);
        await ExecuteAllTasksAsync(context, 2);
        context.Tasks.First(v => v.Type == type).AcceptOutcome();

        // act
        await context.DisposeAsync();

        // assert
        _resilienceContext.Properties.Should().BeSameAs(originalProps);
        if (primary)
        {
            _resilienceContext.Properties.Options.Should().HaveCount(1);
        }
        else
        {
            _resilienceContext.Properties.Options.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task Complete_NoTasks_EnsureCleaned()
    {
        var props = _resilienceContext.Properties;
        var context = Create();
        context.Initialize(_resilienceContext);
        await context.DisposeAsync();
        _resilienceContext.Properties.Should().BeSameAs(props);
    }

    [Fact]
    public async Task Complete_NoAcceptedTasks_ShouldNotThrow()
    {
        var context = Create();
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(TimeSpan.Zero);
        await ExecuteAllTasksAsync(context, 2);

        context.Invoking(c => c.DisposeAsync().AsTask().Wait()).Should().NotThrow();
    }

    [Fact]
    public async Task Complete_MultipleAcceptedTasks_ShouldNotThrow()
    {
        var context = Create();
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(TimeSpan.Zero);
        await ExecuteAllTasksAsync(context, 2);
        context.Tasks[0].AcceptOutcome();
        context.Tasks[1].AcceptOutcome();

        context.Invoking(c => c.DisposeAsync().AsTask().Wait()).Should().NotThrow();
    }

    [Fact]
    public async Task Complete_EnsurePendingTasksCleaned()
    {
        using var assertPrimary = new ManualResetEvent(false);
        using var assertSecondary = new ManualResetEvent(false);

        var context = Create();
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(TimeSpan.FromHours(1));
        (await LoadExecutionAsync(context)).Execution!.OnReset = (execution) =>
        {
            execution.Outcome.Result.Should().BeOfType<DisposableResult>();
            execution.Outcome.Exception.Should().BeNull();
            assertPrimary.Set();
        };
        (await LoadExecutionAsync(context)).Execution!.OnReset = (execution) =>
        {
            execution.Outcome.Exception.Should().BeAssignableTo<OperationCanceledException>();
            assertSecondary.Set();
        };

        await context.TryWaitForCompletedExecutionAsync(System.Threading.Timeout.InfiniteTimeSpan);

        var pending = context.Tasks[1].ExecutionTaskSafe!;
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
        pending.Wait(10).Should().BeFalse();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

        context.Tasks[0].AcceptOutcome();
        await context.DisposeAsync();

        await pending;

        assertPrimary.WaitOne(AssertTimeout).Should().BeTrue();
        assertSecondary.WaitOne(AssertTimeout).Should().BeTrue();
    }

    [Fact]
    public async Task Complete_EnsureCleaned()
    {
        var context = Create();
        context.Initialize(_resilienceContext);
        ConfigureSecondaryTasks(TimeSpan.Zero);
        await ExecuteAllTasksAsync(context, 2);
        context.Tasks[0].AcceptOutcome();

        await context.DisposeAsync();

        context.LoadedTasks.Should().Be(0);
        context.PrimaryContext!.Should().BeNull();

        _onReset.WaitOne(AssertTimeout);
        _resets.Count.Should().Be(1);
        _returnedExecutions.Count.Should().Be(2);
    }

    private async Task ExecuteAllTasksAsync(HedgingExecutionContext<DisposableResult> context, int count)
    {
        for (int i = 0; i < count; i++)
        {
            (await LoadExecutionAsync(context)).Loaded.Should().BeTrue();
            await context.TryWaitForCompletedExecutionAsync(System.Threading.Timeout.InfiniteTimeSpan);
        }
    }

    private async Task<HedgingExecutionContext<DisposableResult>.ExecutionInfo<DisposableResult>> LoadExecutionAsync(
        HedgingExecutionContext<DisposableResult> context,
        TimeSpan? primaryDelay = null,
        bool error = false) =>
        await context.LoadExecutionAsync(
            async (c, _) =>
            {
                if (primaryDelay != null)
                {
                    await _timeProvider.Delay(primaryDelay.Value, c.CancellationToken);
                }

                if (error)
                {
                    throw new InvalidOperationException("Forced error.");
                }

                return Outcome.FromResult(new DisposableResult { Name = "primary" });
            },
            "state");

    private void ConfigureSecondaryTasks(params TimeSpan[] delays) =>
        Generator = args =>
        {
            var attempt = args.AttemptNumber - 1;

            if (attempt >= delays.Length)
            {
                return null;
            }

            return async () =>
            {
                args.ActionContext.Properties.Set(new ResiliencePropertyKey<int>(attempt.ToString(CultureInfo.InvariantCulture)), attempt);
                await _timeProvider.Delay(delays[attempt], args.ActionContext.CancellationToken);
                return Outcome.FromResult(new DisposableResult(delays[attempt].ToString()));
            };
        };

    private Func<HedgingActionGeneratorArguments<DisposableResult>, Func<ValueTask<Outcome<DisposableResult>>>?> Generator { get; set; } = args =>
    {
        return () => Outcome.FromResultAsValueTask(new DisposableResult { Name = Handled });
    };

    private HedgingExecutionContext<DisposableResult> Create()
    {
        var pool = new ObjectPool<TaskExecution<DisposableResult>>(
            () =>
            {
                var telemetry = TestUtilities.CreateResilienceTelemetry(_ => { });
                var execution = new TaskExecution<DisposableResult>(_hedgingHandler, CancellationTokenSourcePool.Create(_timeProvider), _timeProvider, telemetry);
                _createdExecutions.Add(execution);
                return execution;
            },
            execution =>
            {
                _returnedExecutions.Add(execution);
                return true;
            });

        return new(pool, _timeProvider, _maxAttempts, context =>
        {
            _resets.Add(context);
            _onReset.Set();
        });
    }
}
