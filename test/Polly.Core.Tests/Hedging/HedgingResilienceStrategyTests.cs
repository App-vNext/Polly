using System.Threading.Tasks;
using Polly.Hedging;
using Polly.Hedging.Utils;
using Polly.Telemetry;
using Polly.Testing;
using Xunit.Abstractions;

namespace Polly.Core.Tests.Hedging;

public class HedgingResilienceStrategyTests : IDisposable
{
    private const string Success = "Success";

    private const string Failure = "Failure";

    private static readonly TimeSpan LongDelay = TimeSpan.FromDays(1);
    private static readonly TimeSpan AssertTimeout = TimeSpan.FromSeconds(15);

    private readonly HedgingStrategyOptions<string> _options = new();
    private readonly ConcurrentQueue<TelemetryEventArguments<object, object>> _events = new();
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly HedgingTimeProvider _timeProvider;
    private readonly HedgingActions _actions;
    private readonly PrimaryStringTasks _primaryTasks;
    private readonly CancellationTokenSource _cts = new();
    private readonly ITestOutputHelper _testOutput;
    private HedgingHandler<string>? _handler;

    public HedgingResilienceStrategyTests(ITestOutputHelper testOutput)
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(_events.Enqueue);
        _timeProvider = new HedgingTimeProvider { AutoAdvance = _options.Delay };
        _actions = new HedgingActions(_timeProvider);
        _primaryTasks = new PrimaryStringTasks(_timeProvider);
        _options.Delay = TimeSpan.FromSeconds(1);
        _options.MaxHedgedAttempts = _actions.MaxHedgedTasks - 1;
        _testOutput = testOutput;
    }

    private CancellationToken CancellationToken => _cts.Token;

    public void Dispose()
    {
        _cts.Dispose();
        _timeProvider.Advance(TimeSpan.FromDays(365));
    }

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        ConfigureHedging();
        var strategy = (HedgingResilienceStrategy<string>)Create().GetPipelineDescriptor().FirstStrategy.StrategyInstance;

        strategy.TotalAttempts.ShouldBe(_options.MaxHedgedAttempts + 1);
        strategy.HedgingDelay.ShouldBe(_options.Delay);
        strategy.DelayGenerator.ShouldBeNull();
        strategy.HedgingHandler.ShouldNotBeNull();
    }

    [Fact]
    public async Task Execute_CancellationRequested_Throws()
    {
        ConfigureHedging();

        var strategy = (HedgingResilienceStrategy<string>)Create().GetPipelineDescriptor().FirstStrategy.StrategyInstance;
        _cts.Cancel();
        var context = ResilienceContextPool.Shared.Get(CancellationToken);
        context.CancellationToken = _cts.Token;

        var outcome = await strategy.ExecuteCore((_, _) => Outcome.FromResultAsValueTask("dummy"), context, "state");
        outcome.Exception.ShouldBeOfType<OperationCanceledException>();
        outcome.Exception.StackTrace.ShouldNotBeNull();
        outcome.Exception.StackTrace.ShouldContain("Execute_CancellationRequested_Throws");
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
        ConfigureHedging(_ => true, args => () => Outcome.FromResultAsValueTask("any"));
        var strategy = Create();

        strategy.Execute(_ => "dummy", CancellationToken);

        var attempts = _events.Select(v => v.Arguments).OfType<ExecutionAttemptArguments>().ToArray();

        attempts[0].Handled.ShouldBeTrue();
        attempts[0].Duration.ShouldBeGreaterThan(TimeSpan.Zero);
        attempts[0].AttemptNumber.ShouldBe(0);

        attempts[1].Handled.ShouldBeTrue();
        attempts[1].Duration.ShouldBeGreaterThan(TimeSpan.Zero);
        attempts[1].AttemptNumber.ShouldBe(1);
    }

    [Fact]
    public async Task ExecutePrimary_Cancelled_SecondaryShouldBeExecuted()
    {
        _options.MaxHedgedAttempts = 2;

        ConfigureHedging(o => o.Result == "primary", args => () => Outcome.FromResultAsValueTask("secondary"));
        var strategy = Create();

        var result = await strategy.ExecuteAsync(
            context =>
            {
                var source = new CancellationTokenSource();
                source.Cancel();
                context.CancellationToken = source.Token;

                return new ValueTask<string>("primary");
            },
            ResilienceContextPool.Shared.Get(CancellationToken));

        result.ShouldBe("secondary");
    }

    [InlineData(-1)]
    [InlineData(-1000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    [Theory]
    public async Task GetHedgingDelayAsync_GeneratorSet_EnsureCorrectGeneratedValue(int seconds)
    {
        _options.DelayGenerator = args => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(seconds));

        var strategy = (HedgingResilienceStrategy<string>)Create().GetPipelineDescriptor().FirstStrategy.StrategyInstance;

        var result = await strategy.GetHedgingDelayAsync(ResilienceContextPool.Shared.Get(CancellationToken), 0);

        result.ShouldBe(TimeSpan.FromSeconds(seconds));
    }

    [Fact]
    public async Task GetHedgingDelayAsync_NoGeneratorSet_EnsureCorrectValue()
    {
        _options.Delay = TimeSpan.FromMilliseconds(123);

        var strategy = (HedgingResilienceStrategy<string>)Create().GetPipelineDescriptor().FirstStrategy.StrategyInstance;

        var result = await strategy.GetHedgingDelayAsync(ResilienceContextPool.Shared.Get(CancellationToken), 0);

        result.ShouldBe(TimeSpan.FromMilliseconds(123));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAnyPossibleResult()
    {
        ConfigureHedging();

        var strategy = Create();
        var result = await strategy.ExecuteAsync(_primaryTasks.SlowTask, CancellationToken);

        result.ShouldNotBeNull();
#if NET8_0_OR_GREATER
        _timeProvider.TimerEntries.Count.ShouldBe(8);
#else
        _timeProvider.TimerEntries.Count.ShouldBe(5);
#endif
        result.ShouldBe("Oranges");
    }

    [Fact]
    public async Task ExecuteAsync_EnsurePrimaryContextFlows()
    {
        var primaryContext = ResilienceContextPool.Shared.Get(CancellationToken);
        var attempts = 0;
        var key = new ResiliencePropertyKey<string>("primary-key");

        _options.MaxHedgedAttempts = 3;
        _options.OnHedging = args =>
        {
            args.ActionContext.ShouldNotBe(args.PrimaryContext);
            args.PrimaryContext.ShouldBe(primaryContext);
            args.PrimaryContext.Properties.GetValue(key, string.Empty).ShouldBe("dummy");
            attempts++;
            return default;
        };

        ConfigureHedging(args =>
        {
            if (args.AttemptNumber == 1)
            {
                args.PrimaryContext.Properties.Set(key, "dummy");
            }

            args.PrimaryContext.ShouldBe(primaryContext);
            return () => Outcome.FromResultAsValueTask(Failure);
        });

        var strategy = Create();
        var result = await strategy.ExecuteAsync(_ => new ValueTask<string>(Failure), primaryContext);

        attempts.ShouldBe(3);
        primaryContext.Properties.GetValue(key, string.Empty).ShouldBe("dummy");
    }

    [Fact]
    public async Task ExecuteAsync_EnsureHedgedTasksCancelled_Ok()
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
        }, CancellationToken);

        // assert
        _timeProvider.Advance(_options.Delay);
        await Task.Delay(20, CancellationToken);
        _timeProvider.Advance(_options.Delay);
        await Task.Delay(20, CancellationToken);

        _timeProvider.Advance(TimeSpan.FromHours(1));
        (await result).ShouldBe(Success);
        cancelled.WaitOne(AssertTimeout).ShouldBeTrue();
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
        }, CancellationToken);

        // assert
        _timeProvider.Advance(TimeSpan.FromHours(2));

        await TestUtilities.AssertWithTimeoutAsync(() =>
        {
            cancelled.WaitOne(TimeSpan.FromMilliseconds(10)).ShouldBeTrue();
        });

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
                return Outcome.FromResultAsValueTask(secondaryResult);
            };
        });

        var pipeline = Create(handler).AsPipeline();

        // act
        var resultTask = pipeline.ExecuteAsync(async token =>
        {
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            await _timeProvider.Delay(LongDelay);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            return primaryResult;
        }, CancellationToken);

        // assert
        _timeProvider.Advance(LongDelay);

        await primaryResult.WaitForDisposalAsync();
        await resultTask;

        primaryResult.IsDisposed.ShouldBeTrue();
        secondaryResult.IsDisposed.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_EveryHedgedTaskShouldHaveDifferentContexts()
    {
        // arrange
        using var cancellationSource = new CancellationTokenSource();
        var beforeKey = new ResiliencePropertyKey<string>("before");
        var afterKey = new ResiliencePropertyKey<string>("after");

        var primaryContext = ResilienceContextPool.Shared.Get(CancellationToken);
        primaryContext.Properties.Set(beforeKey, "before");
        var contexts = new List<ResilienceContext>();
        var tokenHashCodes = new List<long>();

        ConfigureHedging(_ => false, args =>
        {
            return async () =>
            {
                tokenHashCodes.Add(args.ActionContext.CancellationToken.GetHashCode());
                args.ActionContext.CancellationToken.CanBeCanceled.ShouldBeTrue();
                args.ActionContext.Properties.GetValue(beforeKey, "wrong").ShouldBe("before");
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
                context.CancellationToken.CanBeCanceled.ShouldBeTrue();
                tokenHashCodes.Add(context.CancellationToken.GetHashCode());
                context.Properties.GetValue(beforeKey, "wrong").ShouldBe("before");
                context.ShouldNotBe(primaryContext);
                contexts.Add(context);
                await _timeProvider.Delay(LongDelay, context.CancellationToken);
                return "primary";
            },
            primaryContext,
            "dummy");

        // assert
        contexts.Count.ShouldBeGreaterThan(1);
        contexts.Count.ShouldBe(contexts.Distinct().Count());
        _timeProvider.Advance(LongDelay);
        tokenHashCodes.Distinct().Count().ShouldBeGreaterThan(1);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureOriginalCancellationTokenRestored()
    {
        // arrange
        using var cancellationSource = new CancellationTokenSource();
        var primaryContext = ResilienceContextPool.Shared.Get(CancellationToken);
        primaryContext.CancellationToken = cancellationSource.Token;
        ConfigureHedging(TimeSpan.Zero);
        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync((_, _) => new ValueTask<string>("primary"), primaryContext, "dummy");

        // assert
        primaryContext.CancellationToken.ShouldBe(cancellationSource.Token);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task ExecuteAsync_EnsurePropertiesConsistency(bool primaryFails)
    {
        // arrange
        _options.MaxHedgedAttempts = 1;
        var attempts = _options.MaxHedgedAttempts;
        var primaryContext = ResilienceContextPool.Shared.Get(CancellationToken);
        var storedProps = primaryContext.Properties;
        var contexts = new List<ResilienceContext>();
        var primaryKey = new ResiliencePropertyKey<string>("primary-key");
        var primaryKey2 = new ResiliencePropertyKey<string>("primary-key-2");
        var secondaryKey = new ResiliencePropertyKey<string>("secondary-key");
        storedProps.Set(primaryKey, "primary");

        ConfigureHedging(args =>
        {
            return () =>
            {
                contexts.Add(args.ActionContext);
                args.ActionContext.Properties.GetValue(primaryKey, string.Empty).ShouldBe("primary");
                args.ActionContext.Properties.Set(secondaryKey, "secondary");
                return Outcome.FromResultAsValueTask(primaryFails ? Success : Failure);
            };
        });
        var strategy = Create();

        // act
        await strategy.ExecuteAsync(
            (context, _) =>
            {
                context.ShouldNotBe(primaryContext);
                contexts.Add(context);
                context.Properties.GetValue(primaryKey, string.Empty).ShouldBe("primary");
                context.Properties.Set(primaryKey2, "primary-2");
                return new ValueTask<string>(primaryFails ? Failure : Success);
            },
            primaryContext,
            "state");

        // assert

        primaryContext.Properties.GetValue(primaryKey, string.Empty).ShouldBe("primary");
        primaryContext.Properties.Options.Count.ShouldBe(2);
        primaryContext.Properties.ShouldBeSameAs(storedProps);

        if (primaryFails)
        {
            contexts.Count.ShouldBe(2);
            primaryContext.Properties.GetValue(secondaryKey, string.Empty).ShouldBe("secondary");
            primaryContext.Properties.GetValue(primaryKey2, string.Empty).ShouldBeEmpty();
        }
        else
        {
            contexts.Count.ShouldBe(1);
            primaryContext.Properties.GetValue(primaryKey2, string.Empty).ShouldBe("primary-2");
            primaryContext.Properties.GetValue(secondaryKey, string.Empty).ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task ExecuteAsync_Primary_CustomPropertiesAvailable()
    {
        // arrange
        var key = new ResiliencePropertyKey<string>("my-key");
        var key2 = new ResiliencePropertyKey<string>("my-key-2");
        using var cancellationSource = new CancellationTokenSource();
        var primaryContext = ResilienceContextPool.Shared.Get(CancellationToken);
        primaryContext.Properties.Set(key2, "my-value-2");
        primaryContext.CancellationToken = cancellationSource.Token;
        var props = primaryContext.Properties;
        ConfigureHedging(TimeSpan.Zero);
        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync(
            (context, _) =>
            {
                primaryContext.Properties.TryGetValue(key2, out var val).ShouldBeTrue();
                val.ShouldBe("my-value-2");
                context.Properties.Set(key, "my-value");
                return new ValueTask<string>("primary");
            },
            primaryContext, "dummy");

        // assert
        primaryContext.Properties.TryGetValue(key, out var val).ShouldBeTrue();
        val.ShouldBe("my-value");
        primaryContext.Properties.ShouldBeSameAs(props);
    }

    [Fact]
    public async Task ExecuteAsync_Secondary_CustomPropertiesAvailable()
    {
        // arrange
        var key = new ResiliencePropertyKey<string>("my-key");
        var key2 = new ResiliencePropertyKey<string>("my-key-2");
        var primaryContext = ResilienceContextPool.Shared.Get(CancellationToken);
        var storedProps = primaryContext.Properties;
        primaryContext.Properties.Set(key2, "my-value-2");
        ConfigureHedging(args =>
        {
            return () =>
            {
                args.ActionContext.Properties.TryGetValue(key2, out var val).ShouldBeTrue();
                val.ShouldBe("my-value-2");
                args.ActionContext.Properties.Set(key, "my-value");
                return Outcome.FromResultAsValueTask(Success);
            };
        });
        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync((_, _) => new ValueTask<string>(Failure), primaryContext, "state");

        // assert
        result.ShouldBe(Success);
        primaryContext.Properties.TryGetValue(key, out var val).ShouldBeTrue();
        primaryContext.Properties.ShouldBeSameAs(storedProps);
        val.ShouldBe("my-value");
    }

    [Fact]
    public async Task ExecuteAsync_OnHedgingEventThrows_EnsureExceptionRethrown()
    {
        // arrange
        _options.OnHedging = _ => throw new InvalidOperationException("my-exception");
        ConfigureHedging(args => () => Outcome.FromResultAsValueTask(Success));
        var strategy = Create();

        // act and assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => strategy.ExecuteAsync(_ => new ValueTask<string>(Failure)).AsTask());
        exception.Message.ShouldBe("my-exception");
    }

    [InlineData(-1)] // Fallback mode
    [InlineData(0)] // Parallel mode
    [InlineData(1)] // Latency mode
    [Theory]
    public async Task ExecuteAsync_AllAttemptsFailAndTheOriginalCallIsTheSlowest_EnsureOriginalCallsResultReturned(int delaySeconds)
    {
        // arrange
        int callCounter = 0;
        async ValueTask<string> Execute(CancellationToken token)
        {
            if (callCounter == 0)
            {
                await Task.Delay(200, token);
                return "1st";
            }

            return (++callCounter).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        _options.ShouldHandle = new PredicateBuilder<string>().HandleResult(s => s.Length < 5);
        _options.MaxHedgedAttempts = 2;
        _options.Delay = TimeSpan.FromSeconds(delaySeconds);

        ConfigureHedging(async context => Outcome.FromResult(await Execute(context.CancellationToken)));

        // act
        var actual = await Create().ExecuteAsync(Execute, _cts.Token);

        // assert
        actual.ShouldBe("1st");
    }

    [Fact]
    public async Task ExecuteAsync_CancellationLinking_Ok()
    {
        // arrange
        _options.MaxHedgedAttempts = 2;
        using var primaryCancelled = new ManualResetEvent(false);
        using var secondaryCancelled = new ManualResetEvent(false);
        using var cancellationSource = new CancellationTokenSource();
        var context = ResilienceContextPool.Shared.Get(CancellationToken);
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
        await Should.ThrowAsync<OperationCanceledException>(async () => await task);

        primaryCancelled.WaitOne(AssertTimeout).ShouldBeTrue();
        secondaryCancelled.WaitOne(AssertTimeout).ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_EnsureBackgroundWorkInSuccessfulCallNotCancelled()
    {
        // arrange
        using var cts = new CancellationTokenSource();
        List<Task> backgroundTasks = [];
        ConfigureHedging(BackgroundWork);
        var strategy = Create();

        // act
        var task = await strategy.ExecuteAsync(SlowTask, cts.Token);

        // assert
        _timeProvider.Advance(TimeSpan.FromDays(2));

        await Assert.ThrowsAsync<TaskCanceledException>(() => backgroundTasks[0]);

        // background task is still pending
        backgroundTasks[1].IsCompleted.ShouldBeFalse();

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
            return Outcome.FromResultAsValueTask(Success);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ZeroHedgingDelay_EnsureAllTasksSpawnedAtOnce()
    {
        // arrange
        int executions = 0;
        using var allExecutionsReached = new ManualResetEvent(false);
        ConfigureHedging(context => Execute(context.CancellationToken));
        _options.Delay = TimeSpan.Zero;

        // act
        var task = Create().ExecuteAsync(async c => (await Execute(c)).Result!, CancellationToken);

        // assert
        allExecutionsReached.WaitOne(AssertTimeout).ShouldBeTrue();
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
    public async Task ExecuteAsync_InfiniteHedgingDelay_EnsureNoConcurrentExecutions()
    {
        // arrange
        bool executing = false;
        int executions = 0;
        using var allExecutions = new ManualResetEvent(true);
        ConfigureHedging(async context => Outcome.FromResult(await Execute(context.CancellationToken)));

        // act
        var pending = Create().ExecuteAsync(Execute, _cts.Token);

        // assert
        allExecutions.WaitOne(AssertTimeout).ShouldBeTrue();

        await Should.ThrowAsync<InvalidOperationException>(async () => await pending);

        async ValueTask<string> Execute(CancellationToken token)
        {
            if (executing)
            {
                throw new InvalidOperationException("Concurrent execution detected!");
            }

            executing = true;
            try
            {
                if (Interlocked.Increment(ref executions) == _options.MaxHedgedAttempts)
                {
                    allExecutions.Set();
                }

                await _timeProvider.Delay(LongDelay, token);

                return "dummy";
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
        await Should.ThrowAsync<BadImageFormatException>(() => strategy.ExecuteAsync<string>(_ => throw new InvalidCastException()).AsTask());
        attempts.ShouldBe(3);
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
        await Should.ThrowAsync<InvalidCastException>(() => strategy.ExecuteAsync<string>(_ => throw new InvalidCastException()).AsTask());
        attempts.ShouldBe(3);
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
        await Should.ThrowAsync<InvalidCastException>(() => strategy.ExecuteAsync<string>(_ => throw new InvalidCastException()).AsTask());
        attempts.ShouldBe(0);
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
                        return Outcome.FromExceptionAsValueTask<string>(exception);
                    }

                    return Outcome.FromResultAsValueTask(Success);
                };
            });

        var strategy = Create();
        var result = await strategy.ExecuteAsync<string>(_ => throw new InvalidCastException(), CancellationToken);
        result.ShouldBe(Success);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureHedgingDelayGeneratorRespected()
    {
        var delay = TimeSpan.FromMilliseconds(12345);
        _options.DelayGenerator = _ => new ValueTask<TimeSpan>(TimeSpan.FromMilliseconds(12345));

        ConfigureHedging(res => false, args => () => Outcome.FromResultAsValueTask(Success));

        var strategy = Create();
        var task = strategy.ExecuteAsync<string>(async token =>
        {
            await _timeProvider.Delay(TimeSpan.FromDays(1), token);
            throw new InvalidCastException();
        }, CancellationToken);

        _timeProvider.Advance(TimeSpan.FromHours(5));
        (await task).ShouldBe(Success);
        _timeProvider.TimerEntries.ShouldContain(e => e.Delay == delay);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureExceptionStackTracePreserved()
    {
        ConfigureHedging(res => false, args => null);

        var strategy = Create();

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => strategy.ExecuteAsync(PrimaryTaskThatThrowsError).AsTask());

        exception.Message.ShouldBe("Forced Error");
        exception.StackTrace.ShouldNotBeNull();
        exception.StackTrace.ShouldContain(nameof(PrimaryTaskThatThrowsError));

        static ValueTask<string> PrimaryTaskThatThrowsError(CancellationToken cancellationToken) => throw new InvalidOperationException("Forced Error");
    }

    [Fact]
    public async Task ExecuteAsync_EnsureOnHedgingCalled()
    {
        var primaryContext = ResilienceContextPool.Shared.Get(CancellationToken);
        var key = new ResiliencePropertyKey<string>("my-key");
        primaryContext.Properties.Set(key, "my-value");

        var attempts = new List<int>();
        _options.OnHedging = args =>
        {
            args.PrimaryContext.ShouldBe(primaryContext);
            args.ActionContext.ShouldNotBe(args.PrimaryContext);
            args.PrimaryContext.Properties.GetValue(key, string.Empty).ShouldBe("my-value");
            args.ActionContext.Properties.GetValue(key, string.Empty).ShouldBe("my-value");
            args.ActionContext.Properties.Set(key, "new-value");
            attempts.Add(args.AttemptNumber);
            return default;
        };

        ConfigureHedging(res => res.Result == Failure,
            args => () =>
            {
                args.ActionContext.Properties.GetValue(key, string.Empty).ShouldBe("new-value");
                return Outcome.FromResultAsValueTask(Failure);
            });

        var strategy = Create();
        await strategy.ExecuteAsync(_ => new ValueTask<string>(Failure), primaryContext);

        attempts.Count.ShouldBe(_options.MaxHedgedAttempts);
        attempts.ShouldBeInOrder();
        attempts[0].ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureOnHedgingTelemetry()
    {
        var context = ResilienceContextPool.Shared.Get(CancellationToken);

        ConfigureHedging(res => res.Result == Failure, args => () => Outcome.FromResultAsValueTask(Failure));

        var strategy = Create();
        await strategy.ExecuteAsync((_, _) => new ValueTask<string>(Failure), context, "state");

        _events.Count.ShouldBe(_options.MaxHedgedAttempts + 4);
        _events.Select(v => v.Event.EventName).Distinct().Count().ShouldBe(2);
    }

    private void ConfigureHedging() =>
        ConfigureHedging(_ => false, _actions.Generator);

    private void ConfigureHedging(Func<ResilienceContext, ValueTask<Outcome<string>>> background) =>
        ConfigureHedging(args => () => background(args.ActionContext));

    private void ConfigureHedging(Func<HedgingActionGeneratorArguments<string>, Func<ValueTask<Outcome<string>>>?> generator) =>
        ConfigureHedging(res => res.Result == Failure, generator);

    private void ConfigureHedging(
        Func<Outcome<string>, bool> shouldHandle,
        Func<HedgingActionGeneratorArguments<string>, Func<ValueTask<Outcome<string>>>?> generator) =>
        _handler = HedgingHelper.CreateHandler(shouldHandle, generator, _options.OnHedging);

    private void ConfigureHedging(TimeSpan delay) => ConfigureHedging(args => async () =>
    {
        await Task.Delay(delay);
        return Outcome.FromResult("secondary");
    });

    private ResiliencePipeline<string> Create() => Create(_handler!).AsPipeline();

    private HedgingResilienceStrategy<T> Create<T>(HedgingHandler<T> handler) => new(
        _options.Delay,
        _options.MaxHedgedAttempts,
        handler,
        _options.DelayGenerator,
        _timeProvider,
        _telemetry);
}
