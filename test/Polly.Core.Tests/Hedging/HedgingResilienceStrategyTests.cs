using Polly.Hedging;
using Polly.Hedging.Utils;
using Polly.Telemetry;
using Polly.Utils;
using Xunit.Abstractions;

namespace Polly.Core.Tests.Hedging;

public class HedgingResilienceStrategyTests : IDisposable
{
    private const string Success = "Success";

    private const string Failure = "Failure";

    private static readonly TimeSpan LongDelay = TimeSpan.FromDays(1);
    private static readonly TimeSpan AssertTimeout = TimeSpan.FromSeconds(15);

    private readonly HedgingStrategyOptions<string> _options = new();
    private readonly ConcurrentQueue<TelemetryEventArguments> _events = new();
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly HedgingTimeProvider _timeProvider;
    private readonly HedgingActions _actions;
    private readonly PrimaryStringTasks _primaryTasks;
    private readonly List<object?> _results = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly ITestOutputHelper _testOutput;
    private HedgingHandler<string>? _handler;

    public HedgingResilienceStrategyTests(ITestOutputHelper testOutput)
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(_events.Enqueue);
        _timeProvider = new HedgingTimeProvider { AutoAdvance = _options.HedgingDelay };
        _actions = new HedgingActions(_timeProvider);
        _primaryTasks = new PrimaryStringTasks(_timeProvider);
        _options.HedgingDelay = TimeSpan.FromSeconds(1);
        _options.MaxHedgedAttempts = _actions.MaxHedgedTasks;
        _testOutput = testOutput;
    }

    public void Dispose()
    {
        _cts.Dispose();
        _timeProvider.Advance(TimeSpan.FromDays(365));
    }

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        ConfigureHedging();
        var strategy = (HedgingResilienceStrategy<string>)Create().Strategy;

        strategy.MaxHedgedAttempts.Should().Be(_options.MaxHedgedAttempts);
        strategy.HedgingDelay.Should().Be(_options.HedgingDelay);
        strategy.HedgingDelayGenerator.Should().BeNull();
        strategy.HedgingHandler.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_CancellationRequested_Throws()
    {
        ConfigureHedging();

        var strategy = Create();
        _cts.Cancel();
        var context = ResilienceContextPool.Shared.Get();
        context.CancellationToken = _cts.Token;

        var outcome = await strategy.ExecuteOutcomeAsync((_, _) => Outcome.FromResultAsTask("dummy"), context, "state");
        outcome.Exception.Should().BeOfType<OperationCanceledException>();
        outcome.Exception!.StackTrace.Should().Contain("Execute_CancellationRequested_Throws");
    }

    [Fact]
    public void ExecutePrimaryAndSecondary_EnsureAttemptReported()
    {
        int timeStamp = 0;
        _timeProvider.TimeStampProvider = () =>
        {
            timeStamp += 1000;
            return timeStamp;
        };
        _options.MaxHedgedAttempts = 2;
        ConfigureHedging(_ => true, args => () => Outcome.FromResultAsTask("any"));
        var strategy = Create();

        strategy.Execute(_ => "dummy");

        var attempts = _events.Select(v => v.Arguments).OfType<ExecutionAttemptArguments>().ToArray();

        attempts[0].Handled.Should().BeTrue();
        attempts[0].Duration.Should().BeGreaterThan(TimeSpan.Zero);
        attempts[0].AttemptNumber.Should().Be(0);

        attempts[1].Handled.Should().BeTrue();
        attempts[1].Duration.Should().BeGreaterThan(TimeSpan.Zero);
        attempts[1].AttemptNumber.Should().Be(1);
    }

    [Fact]
    public async Task ExecutePrimary_Cancelled_SecondaryShouldBeExecuted()
    {
        _options.MaxHedgedAttempts = 2;

        ConfigureHedging(o => o.Result == "primary", args => () => Outcome.FromResultAsTask("secondary"));
        var strategy = Create();

        var result = await strategy.ExecuteAsync(
            context =>
            {
                var source = new CancellationTokenSource();
                source.Cancel();
                context.CancellationToken = source.Token;

                return new ValueTask<string>("primary");
            },
            ResilienceContextPool.Shared.Get());

        result.Should().Be("secondary");
    }

    [InlineData(-1)]
    [InlineData(-1000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    [Theory]
    public async Task GetHedgingDelayAsync_GeneratorSet_EnsureCorrectGeneratedValue(int seconds)
    {
        _options.HedgingDelayGenerator = args => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(seconds));

        var strategy = (HedgingResilienceStrategy<string>)Create().Strategy;

        var result = await strategy.GetHedgingDelayAsync(ResilienceContextPool.Shared.Get(), 0);

        result.Should().Be(TimeSpan.FromSeconds(seconds));
    }

    [Fact]
    public async Task GetHedgingDelayAsync_NoGeneratorSet_EnsureCorrectValue()
    {
        _options.HedgingDelay = TimeSpan.FromMilliseconds(123);

        var strategy = (HedgingResilienceStrategy<string>)Create().Strategy;

        var result = await strategy.GetHedgingDelayAsync(ResilienceContextPool.Shared.Get(), 0);

        result.Should().Be(TimeSpan.FromMilliseconds(123));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAnyPossibleResult()
    {
        ConfigureHedging();

        var strategy = Create();
        var result = await strategy.ExecuteAsync(_primaryTasks.SlowTask);

        result.Should().NotBeNull();
        _timeProvider.TimerEntries.Should().HaveCount(5);
        result.Should().Be("Oranges");
    }

    [Fact]
    public async Task ExecuteAsync_EnsurePrimaryContextFlows()
    {
        var primaryContext = ResilienceContextPool.Shared.Get();
        var attempts = 0;
        var key = new ResiliencePropertyKey<string>("primary-key");

        _options.MaxHedgedAttempts = 4;
        _options.OnHedging = args =>
        {
            args.Context.Should().Be(primaryContext);

            if (args.Arguments.AttemptNumber == 0)
            {
                args.Context.Properties.Set(key, "dummy");
            }

            attempts++;

            return default;
        };

        ConfigureHedging(args =>
        {
            args.PrimaryContext.Properties.GetValue(key, string.Empty).Should().Be("dummy");
            args.PrimaryContext.Should().Be(primaryContext);
            return () => Outcome.FromResultAsTask(Failure);
        });

        var strategy = Create();
        var result = await strategy.ExecuteAsync(_ => new ValueTask<string>(Failure), primaryContext);

        attempts.Should().Be(4);
        primaryContext.Properties.GetValue(key, string.Empty).Should().Be("dummy");
    }

    [Fact]
    public async void ExecuteAsync_EnsureHedgedTasksCancelled_Ok()
    {
        // arrange
        _testOutput.WriteLine("ExecuteAsync_EnsureHedgedTasksCancelled_Ok executing...");

        _options.MaxHedgedAttempts = 2;
        using var cancelled = new ManualResetEvent(false);
        ConfigureHedging(async context =>
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                _testOutput.WriteLine("Hedged task executing...");
                await Task.Delay(LongDelay, context.CancellationToken);
                _testOutput.WriteLine("Hedged task executing...done (not-cancelled)");
            }
            catch (OperationCanceledException)
            {
                _testOutput.WriteLine("Hedged task executing...cancelled");
                cancelled.Set();
            }
            catch (Exception e)
            {
                _testOutput.WriteLine($"Hedged task executing...error({e})");
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return Outcome.FromResult(Failure);
        });

        var strategy = Create();

        // act
        var result = strategy.ExecuteAsync(async token =>
        {
            await _timeProvider.Delay(TimeSpan.FromHours(1), token);
            return Success;
        });

        // assert
        _timeProvider.Advance(_options.HedgingDelay);
        await Task.Delay(20);
        _timeProvider.Advance(_options.HedgingDelay);
        await Task.Delay(20);

        _timeProvider.Advance(TimeSpan.FromHours(1));
        (await result).Should().Be(Success);
        cancelled.WaitOne(AssertTimeout).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_EnsurePrimaryTaskCancelled_Ok()
    {
        // arrange
        using var cancelled = new ManualResetEvent(false);
        ConfigureHedging(async context =>
        {
            await _timeProvider.Delay(TimeSpan.FromHours(1), context.CancellationToken);
            return Outcome.FromResult(Success);
        });

        var strategy = Create();

        // act
        var task = strategy.ExecuteAsync(async token =>
        {
            try
            {
                await _timeProvider.Delay(TimeSpan.FromHours(24), token);
            }
            catch (OperationCanceledException)
            {
                cancelled.Set();
            }

            return Success;
        });

        // assert
        _timeProvider.Advance(TimeSpan.FromHours(2));

        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            cancelled.WaitOne(TimeSpan.FromMilliseconds(10)).Should().BeTrue();
        });

        await task;
    }

    [Fact]
    public async Task ExecuteAsync_EnsureSecondaryHedgedTaskReportedWithNoOutcome()
    {
        // arrange
        using var cancelled = new ManualResetEvent(false);
        var hasOutcome = true;
        _options.OnHedging = args =>
        {
            hasOutcome = args.Arguments.HasOutcome;
            return default;
        };

        ConfigureHedging(context => Outcome.FromResultAsTask(Success));

        var strategy = Create();

        // act
        var task = strategy.ExecuteAsync(async token =>
        {
            await _timeProvider.Delay(TimeSpan.FromHours(24), token);
            return Success;
        });

        // assert
        _timeProvider.Advance(TimeSpan.FromHours(2));
        hasOutcome.Should().BeFalse();
        await task;
    }

    [Fact]
    public async Task ExecuteAsync_EnsureDiscardedResultDisposed()
    {
        // arrange
        using var primaryResult = new DisposableResult();
        using var secondaryResult = new DisposableResult();

        var handler = HedgingHelper.CreateHandler<DisposableResult>(_ => false, args =>
        {
            return () =>
            {
                return Outcome.FromResultAsTask(secondaryResult);
            };
        });

        var strategy = new ReactiveResilienceStrategyBridge<DisposableResult>(Create(handler, null));

        // act
        var resultTask = strategy.ExecuteAsync(async token =>
        {
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            await _timeProvider.Delay(LongDelay);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            return primaryResult;
        });

        // assert
        _timeProvider.Advance(LongDelay);

        await primaryResult.WaitForDisposalAsync();
        await resultTask;

        primaryResult.IsDisposed.Should().BeTrue();
        secondaryResult.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_EveryHedgedTaskShouldHaveDifferentContexts()
    {
        // arrange
        using var cancellationSource = new CancellationTokenSource();
        var beforeKey = new ResiliencePropertyKey<string>("before");
        var afterKey = new ResiliencePropertyKey<string>("after");

        var primaryContext = ResilienceContextPool.Shared.Get();
        primaryContext.Properties.Set(beforeKey, "before");
        var contexts = new List<ResilienceContext>();
        var tokenHashCodes = new List<long>();

        ConfigureHedging(_ => false, args =>
        {
            return async () =>
            {
                tokenHashCodes.Add(args.ActionContext.CancellationToken.GetHashCode());
                args.ActionContext.CancellationToken.CanBeCanceled.Should().BeTrue();
                args.ActionContext.Properties.GetValue(beforeKey, "wrong").Should().Be("before");
                contexts.Add(args.ActionContext);
                await Task.Yield();
                args.ActionContext.Properties.Set(afterKey, "after");
                return Outcome.FromResult("secondary");
            };
        });

        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync(
            async (context, _) =>
            {
                context.CancellationToken.CanBeCanceled.Should().BeTrue();
                tokenHashCodes.Add(context.CancellationToken.GetHashCode());
                context.Properties.GetValue(beforeKey, "wrong").Should().Be("before");
                context.Should().Be(primaryContext);
                contexts.Add(context);
                await _timeProvider.Delay(LongDelay, context.CancellationToken);
                return "primary";
            },
            primaryContext,
            "dummy");

        // assert
        contexts.Should().HaveCountGreaterThan(1);
        contexts.Count.Should().Be(contexts.Distinct().Count());
        _timeProvider.Advance(LongDelay);
        tokenHashCodes.Distinct().Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureOriginalCancellationTokenRestored()
    {
        // arrange
        using var cancellationSource = new CancellationTokenSource();
        var primaryContext = ResilienceContextPool.Shared.Get();
        primaryContext.CancellationToken = cancellationSource.Token;
        ConfigureHedging(TimeSpan.Zero);
        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync((_, _) => new ValueTask<string>("primary"), primaryContext, "dummy");

        // assert
        primaryContext.CancellationToken.Should().Be(cancellationSource.Token);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task ExecuteAsync_EnsurePropertiesConsistency(bool primaryFails)
    {
        // arrange
        _options.MaxHedgedAttempts = 2;
        var attempts = _options.MaxHedgedAttempts;
        var primaryContext = ResilienceContextPool.Shared.Get();
        var storedProps = primaryContext.Properties;
        var contexts = new List<ResilienceContext>();
        var primaryKey = new ResiliencePropertyKey<string>("primary-key");
        var primaryKey2 = new ResiliencePropertyKey<string>("primary-key-2");
        var secondaryKey = new ResiliencePropertyKey<string>("secondary-key");
        storedProps.Set(primaryKey, "primary");

        ConfigureHedging(args =>
        {
            return async () =>
            {
                contexts.Add(args.ActionContext);
                args.ActionContext.Properties.GetValue(primaryKey, string.Empty).Should().Be("primary");
                args.ActionContext.Properties.Set(secondaryKey, "secondary");
                await _timeProvider.Delay(TimeSpan.FromHours(1), args.ActionContext.CancellationToken);
                return Outcome.FromResult(primaryFails ? Success : Failure);
            };
        });
        var strategy = Create();

        // act
        var executeTask = strategy.ExecuteAsync(
            async (context, _) =>
            {
                contexts.Add(context);
                context.Properties.GetValue(primaryKey, string.Empty).Should().Be("primary");
                context.Properties.Set(primaryKey2, "primary-2");
                await _timeProvider.Delay(TimeSpan.FromHours(2), context.CancellationToken);
                return primaryFails ? Failure : Success;
            },
            primaryContext,
            "state").AsTask();

        // assert

        contexts.Should().HaveCount(2);
        primaryContext.Properties.Options.Should().HaveCount(2);
        primaryContext.Properties.GetValue(primaryKey, string.Empty).Should().Be("primary");

        if (primaryFails)
        {
            _timeProvider.Advance(TimeSpan.FromHours(1));
            await executeTask;
            primaryContext.Properties.GetValue(secondaryKey, string.Empty).Should().Be("secondary");
        }
        else
        {
            _timeProvider.Advance(TimeSpan.FromHours(2));
            await executeTask;
            primaryContext.Properties.GetValue(primaryKey2, string.Empty).Should().Be("primary-2");
        }

        primaryContext.Properties.Should().BeSameAs(storedProps);
    }

    [Fact]
    public async Task ExecuteAsync_Primary_CustomPropertiesAvailable()
    {
        // arrange
        var key = new ResiliencePropertyKey<string>("my-key");
        var key2 = new ResiliencePropertyKey<string>("my-key-2");
        using var cancellationSource = new CancellationTokenSource();
        var primaryContext = ResilienceContextPool.Shared.Get();
        primaryContext.Properties.Set(key2, "my-value-2");
        primaryContext.CancellationToken = cancellationSource.Token;
        var props = primaryContext.Properties;
        ConfigureHedging(TimeSpan.Zero);
        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync(
            (context, _) =>
            {
                primaryContext.Properties.TryGetValue(key2, out var val).Should().BeTrue();
                val.Should().Be("my-value-2");
                context.Properties.Set(key, "my-value");
                return new ValueTask<string>("primary");
            },
            primaryContext, "dummy");

        // assert
        primaryContext.Properties.TryGetValue(key, out var val).Should().BeTrue();
        val.Should().Be("my-value");
        primaryContext.Properties.Should().BeSameAs(props);
    }

    [Fact]
    public async Task ExecuteAsync_Secondary_CustomPropertiesAvailable()
    {
        // arrange
        var key = new ResiliencePropertyKey<string>("my-key");
        var key2 = new ResiliencePropertyKey<string>("my-key-2");
        var primaryContext = ResilienceContextPool.Shared.Get();
        var storedProps = primaryContext.Properties;
        primaryContext.Properties.Set(key2, "my-value-2");
        ConfigureHedging(args =>
        {
            return () =>
            {
                args.ActionContext.Properties.TryGetValue(key2, out var val).Should().BeTrue();
                val.Should().Be("my-value-2");
                args.ActionContext.Properties.Set(key, "my-value");
                return Outcome.FromResultAsTask(Success);
            };
        });
        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync((_, _) => new ValueTask<string>(Failure), primaryContext, "state");

        // assert
        result.Should().Be(Success);
        primaryContext.Properties.TryGetValue(key, out var val).Should().BeTrue();
        primaryContext.Properties.Should().BeSameAs(storedProps);
        val.Should().Be("my-value");
    }

    [Fact]
    public async Task ExecuteAsync_OnHedgingEventThrows_EnsureExceptionRethrown()
    {
        // arrange
        ConfigureHedging(args => () => Outcome.FromResultAsTask(Success));
        _options.OnHedging = _ => throw new InvalidOperationException("my-exception");
        var strategy = Create();

        // act
        (await strategy
            .Invoking(s => s.ExecuteAsync(_ => new ValueTask<string>(Failure)).AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>())
            .WithMessage("my-exception");
    }

    [Fact]
    public async Task ExecuteAsync_CancellationLinking_Ok()
    {
        // arrange
        _options.MaxHedgedAttempts = 2;
        using var primaryCancelled = new ManualResetEvent(false);
        using var secondaryCancelled = new ManualResetEvent(false);
        using var cancellationSource = new CancellationTokenSource();
        var context = ResilienceContextPool.Shared.Get();
        context.CancellationToken = cancellationSource.Token;

        ConfigureHedging(async context =>
        {
            try
            {
                await _timeProvider.Delay(TimeSpan.FromDays(0.5), context.CancellationToken);
            }
            catch (OperationCanceledException e)
            {
                secondaryCancelled.Set();
                return Outcome.FromException<string>(e);
            }

            return Outcome.FromResult(Success);
        });

        var strategy = Create();

        // act
        var task = strategy.ExecuteAsync(
            async (context, _) =>
            {
                try
                {
                    await _primaryTasks.SlowTask(context.CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    primaryCancelled.Set();
                    throw;
                }

                return Success;
            },
            context, "dummy");

        // assert
        _timeProvider.Advance(TimeSpan.FromHours(4));
        cancellationSource.Cancel();
        _timeProvider.Advance(TimeSpan.FromHours(1));
        await task.Invoking(async t => await t).Should().ThrowAsync<OperationCanceledException>();

        primaryCancelled.WaitOne(AssertTimeout).Should().BeTrue();
        secondaryCancelled.WaitOne(AssertTimeout).Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_EnsureBackgroundWorkInSuccessfulCallNotCancelled()
    {
        // arrange
        using var cts = new CancellationTokenSource();
        List<Task> backgroundTasks = new List<Task>();
        ConfigureHedging(BackgroundWork);
        var strategy = Create();

        // act
        var task = await strategy.ExecuteAsync(SlowTask, cts.Token);

        // assert
        _timeProvider.Advance(TimeSpan.FromDays(2));

        await Assert.ThrowsAsync<TaskCanceledException>(() => backgroundTasks[0]);

        // background task is still pending
        Assert.False(backgroundTasks[1].IsCompleted);

        cts.Cancel();

        async ValueTask<string> SlowTask(CancellationToken cancellationToken)
        {
            var delay = Task.Delay(TimeSpan.FromDays(1), cancellationToken);
            backgroundTasks.Add(delay);
            await delay;
            return Success;
        }

        ValueTask<Outcome<string>> BackgroundWork(ResilienceContext resilienceContext)
        {
            var delay = Task.Delay(TimeSpan.FromDays(24), resilienceContext.CancellationToken);
            backgroundTasks.Add(delay);
            return Outcome.FromResultAsTask(Success);
        }
    }

    [Fact]
    public async void ExecuteAsync_ZeroHedgingDelay_EnsureAllTasksSpawnedAtOnce()
    {
        // arrange
        int executions = 0;
        using var allExecutionsReached = new ManualResetEvent(false);
        ConfigureHedging(context => Execute(context.CancellationToken));
        _options.HedgingDelay = TimeSpan.Zero;

        // act
        var task = Create().ExecuteAsync(async c => (await Execute(c)).Result, default);

        // assert
        Assert.True(allExecutionsReached.WaitOne(AssertTimeout));
        _timeProvider.Advance(LongDelay);
        await task;

        async ValueTask<Outcome<string>> Execute(CancellationToken token)
        {
            if (Interlocked.Increment(ref executions) == _options.MaxHedgedAttempts)
            {
                allExecutionsReached.Set();
            }

            await _timeProvider.Delay(LongDelay, token);
            return Outcome.FromResult(Success);
        }
    }

    [Fact]
    public void ExecuteAsync_InfiniteHedgingDelay_EnsureNoConcurrentExecutions()
    {
        // arrange
        bool executing = false;
        int executions = 0;
        using var allExecutions = new ManualResetEvent(true);
        ConfigureHedging(context => Execute(context.CancellationToken));

        // act
        var pending = Create().ExecuteAsync(Execute, _cts.Token);

        // assert
        Assert.True(allExecutions.WaitOne(AssertTimeout));

        async ValueTask<Outcome<string>> Execute(CancellationToken token)
        {
            if (executing)
            {
                return Outcome.FromException<string>(new InvalidOperationException("Concurrent execution detected!"));
            }

            executing = true;
            try
            {
                if (Interlocked.Increment(ref executions) == _options.MaxHedgedAttempts)
                {
                    allExecutions.Set();
                }

                await _timeProvider.Delay(LongDelay, token);

                return Outcome.FromResult("dummy");
            }
            finally
            {
                executing = false;
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionsHandled_ShouldThrowAnyException()
    {
        int attempts = 0;

        ConfigureHedging(outcome => outcome.Exception switch
        {
            InvalidOperationException => true,
            ArgumentException => true,
            InvalidCastException => true,
            _ => false
        },
        args =>
        {
            Exception exception = args.AttemptNumber switch
            {
                1 => new ArgumentException(),
                2 => new InvalidOperationException(),
                3 => new BadImageFormatException(),
                _ => new NotSupportedException()
            };

            attempts++;
            return () => throw exception;
        });

        var strategy = Create();
        await strategy.Invoking(s => s.ExecuteAsync<string>(_ => throw new InvalidCastException()).AsTask()).Should().ThrowAsync<BadImageFormatException>();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionsHandled_ShouldThrowLastException()
    {
        int attempts = 0;

        ConfigureHedging(outcome => outcome.Exception switch
        {
            InvalidOperationException => true,
            ArgumentException => true,
            InvalidCastException => true,
            BadImageFormatException => true,
            _ => false
        },
        args =>
        {
            Exception exception = args.AttemptNumber switch
            {
                1 => new ArgumentException(),
                2 => new InvalidOperationException(),
                3 => new BadImageFormatException(),
                _ => new NotSupportedException()
            };

            attempts++;
            return () => throw exception;
        });

        var strategy = Create();
        await strategy.Invoking(s => s.ExecuteAsync<string>(_ => throw new InvalidCastException()).AsTask()).Should().ThrowAsync<InvalidCastException>();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_PrimaryExceptionNotHandled_Rethrow()
    {
        int attempts = 0;

        ConfigureHedging(outcome => outcome.Exception is InvalidOperationException, args =>
        {
            attempts++;
            return null;
        });

        var strategy = Create();
        await strategy.Invoking(s => s.ExecuteAsync<string>(_ => throw new InvalidCastException()).AsTask()).Should().ThrowAsync<InvalidCastException>();
        attempts.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionsHandled_ShouldReturnLastResult()
    {
        int attempts = 0;

        ConfigureHedging(
            outcome => outcome.Exception switch
            {
                InvalidOperationException => true,
                ArgumentException => true,
                InvalidCastException => true,
                BadImageFormatException => true,
                _ => false
            },
            args =>
            {
                Exception? exception = args.AttemptNumber switch
                {
                    1 => new ArgumentException(),
                    2 => new InvalidOperationException(),
                    3 => null,
                    _ => new NotSupportedException()
                };

                attempts++;
                return () =>
                {
                    if (exception != null)
                    {
                        return Outcome.FromExceptionAsTask<string>(exception);
                    }

                    return Outcome.FromResultAsTask(Success);
                };
            });

        var strategy = Create();
        var result = await strategy.ExecuteAsync<string>(_ => throw new InvalidCastException());
        result.Should().Be(Success);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureHedgingDelayGeneratorRespected()
    {
        var delay = TimeSpan.FromMilliseconds(12345);
        _options.HedgingDelayGenerator = _ => new ValueTask<TimeSpan>(TimeSpan.FromMilliseconds(12345));

        ConfigureHedging(res => false, args => () => Outcome.FromResultAsTask(Success));

        var strategy = Create();
        var task = strategy.ExecuteAsync<string>(async token =>
        {
            await _timeProvider.Delay(TimeSpan.FromDays(1), token);
            throw new InvalidCastException();
        });

        _timeProvider.Advance(TimeSpan.FromHours(5));
        (await task).Should().Be(Success);
        _timeProvider.TimerEntries.Should().Contain(e => e.Delay == delay);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureExceptionStackTracePreserved()
    {
        ConfigureHedging(res => false, args => null);

        var strategy = Create();

        var exception = await strategy.Invoking(s => s.ExecuteAsync(PrimaryTaskThatThrowsError).AsTask()).Should().ThrowAsync<InvalidOperationException>();

        exception.WithMessage("Forced Error");
        exception.And.StackTrace.Should().Contain(nameof(PrimaryTaskThatThrowsError));

        static ValueTask<string> PrimaryTaskThatThrowsError(CancellationToken cancellationToken) => throw new InvalidOperationException("Forced Error");
    }

    [Fact]
    public async Task ExecuteAsync_EnsureOnHedgingCalled()
    {
        var attempts = new List<int>();
        _options.OnHedging = args =>
        {
            args.Arguments.HasOutcome.Should().BeTrue();
            args.Result.Should().Be(Failure);
            attempts.Add(args.Arguments.AttemptNumber);
            return default;
        };

        ConfigureHedging(res => res.Result == Failure, args => () => Outcome.FromResultAsTask(Failure));

        var strategy = Create();
        await strategy.ExecuteAsync(_ => new ValueTask<string>(Failure));

        attempts.Should().HaveCount(_options.MaxHedgedAttempts);
        attempts.Should().BeInAscendingOrder();
        attempts[0].Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureOnHedgingTelemetry()
    {
        var context = ResilienceContextPool.Shared.Get();

        ConfigureHedging(res => res.Result == Failure, args => () => Outcome.FromResultAsTask(Failure));

        var strategy = Create();
        await strategy.ExecuteAsync((_, _) => new ValueTask<string>(Failure), context, "state");

        context.ResilienceEvents.Should().HaveCount(_options.MaxHedgedAttempts + 1);
        context.ResilienceEvents.Select(v => v.EventName).Distinct().Should().HaveCount(2);
    }

    private void ConfigureHedging()
    {
        _options.OnHedging = args =>
        {
            lock (_results)
            {
                _results.Add(args.Result!);
            }

            return default;
        };

        ConfigureHedging(_ => false, _actions.Generator);
    }

    private void ConfigureHedging(Func<ResilienceContext, ValueTask<Outcome<string>>> background)
    {
        ConfigureHedging(args => () => background(args.ActionContext));
    }

    private void ConfigureHedging(Func<HedgingActionGeneratorArguments<string>, Func<ValueTask<Outcome<string>>>?> generator)
    {
        ConfigureHedging(res => res.Result == Failure, generator);
    }

    private void ConfigureHedging(
        Func<Outcome<string>, bool> shouldHandle,
        Func<HedgingActionGeneratorArguments<string>, Func<ValueTask<Outcome<string>>>?> generator)
    {
        _handler = HedgingHelper.CreateHandler(shouldHandle, generator);
    }

    private void ConfigureHedging(TimeSpan delay) => ConfigureHedging(args => async () =>
    {
        await Task.Delay(delay);
        return Outcome.FromResult("secondary");
    });

    private ReactiveResilienceStrategyBridge<string> Create() => new(Create(_handler!, _options.OnHedging));

    private HedgingResilienceStrategy<T> Create<T>(
        HedgingHandler<T> handler,
        Func<OutcomeArguments<T, OnHedgingArguments>, ValueTask>? onHedging) => new(
        _options.HedgingDelay,
        _options.MaxHedgedAttempts,
        handler,
        onHedging,
        _options.HedgingDelayGenerator,
        _timeProvider,
        _telemetry);
}
