using System;
using Polly.Hedging;
using Polly.Strategy;

namespace Polly.Core.Tests.Hedging;

public class HedgingResilienceStrategyTests : IDisposable
{
    private const string Success = "Success";

    private const string Failure = "Failure";

    private static readonly TimeSpan LongDelay = TimeSpan.FromDays(1);
    private static readonly TimeSpan AssertTimeout = TimeSpan.FromSeconds(10);

    private readonly HedgingStrategyOptions _options = new();
    private readonly List<IResilienceArguments> _events = new();
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly HedgingTimeProvider _timeProvider;
    private readonly HedgingActions _actions;
    private readonly PrimaryStringTasks _primaryTasks;
    private readonly List<object?> _results = new();
    private readonly CancellationTokenSource _cts = new();

    public HedgingResilienceStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(args => _events.Add(args));
        _timeProvider = new HedgingTimeProvider { AutoAdvance = _options.HedgingDelay };
        _actions = new HedgingActions(_timeProvider);
        _primaryTasks = new PrimaryStringTasks(_timeProvider);
        _options.HedgingDelay = TimeSpan.FromSeconds(1);
        _options.MaxHedgedAttempts = _actions.MaxHedgedTasks;
    }

    public void Dispose()
    {
        _cts.Dispose();
        _timeProvider.Advance(TimeSpan.FromDays(365));
    }

    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var strategy = Create();

        strategy.MaxHedgedAttempts.Should().Be(_options.MaxHedgedAttempts);
        strategy.HedgingDelay.Should().Be(_options.HedgingDelay);
        strategy.HedgingDelayGenerator.Should().BeNull();
        strategy.HedgingHandler.Should().BeNull();
        strategy.HedgingHandler.Should().BeNull();
    }

    [Fact]
    public void Execute_Skipped_Ok()
    {
        var strategy = Create();

        strategy.Execute(_ => 10).Should().Be(10);
    }

    [Fact]
    public async Task Execute_CancellationRequested_Throws()
    {
        ConfigureHedging();

        var strategy = Create();
        _cts.Cancel();

        await strategy.Invoking(s => s.ExecuteAsync(_ => Task.FromResult(Success), _cts.Token))
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }

    [InlineData(-1)]
    [InlineData(-1000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    [Theory]
    public async Task GetHedgingDelayAsync_GeneratorSet_EnsureCorrectGeneratedValue(int seconds)
    {
        _options.HedgingDelayGenerator.SetGenerator(args => TimeSpan.FromSeconds(seconds));

        var strategy = Create();

        var result = await strategy.GetHedgingDelayAsync(ResilienceContext.Get(), 0);

        result.Should().Be(TimeSpan.FromSeconds(seconds));
    }

    [Fact]
    public async Task GetHedgingDelayAsync_NoGeneratorSet_EnsureCorrectValue()
    {
        _options.HedgingDelay = TimeSpan.FromMilliseconds(123);

        var strategy = Create();

        var result = await strategy.GetHedgingDelayAsync(ResilienceContext.Get(), 0);

        result.Should().Be(TimeSpan.FromMilliseconds(123));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAnyPossibleResult()
    {
        ConfigureHedging();

        var strategy = Create();
        var result = await strategy.ExecuteAsync(_primaryTasks.SlowTask);

        result.Should().NotBeNull();
        _timeProvider.DelayEntries.Should().HaveCount(5);
        result.Should().Be("Oranges");
    }

    [Fact]
    public async void ExecuteAsync_EnsureHedgedTasksCancelled_Ok()
    {
        // arrange
        _options.MaxHedgedAttempts = 2;
        using var cancelled = new ManualResetEvent(false);
        ConfigureHedging(async context =>
        {
            try
            {
                await _timeProvider.Delay(LongDelay, context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                cancelled.Set();
            }

            return Failure;
        });

        var strategy = Create();

        // act
        var result = strategy.ExecuteAsync(async token =>
        {
            await _timeProvider.Delay(TimeSpan.FromHours(1), token);
            return Success;
        });

        // assert
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
            return Success;
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
        cancelled.WaitOne(TimeSpan.FromSeconds(1)).Should().BeTrue();
        await task;
    }

    [Fact]
    public async Task ExecuteAsync_EnsureDiscardedResultDisposed()
    {
        // arrange
        using var primaryResult = new DisposableResult();
        using var secondaryResult = new DisposableResult();

        ConfigureHedging<DisposableResult>(handler =>
        {
            handler.HedgingActionGenerator = args =>
            {
                return () =>
                {
                    return Task.FromResult(secondaryResult);
                };
            };
        });

        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync(async token =>
        {
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            await _timeProvider.Delay(LongDelay);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            return primaryResult;
        });

        // assert
        _timeProvider.Advance(LongDelay);

        await primaryResult.WaitForDisposalAsync();
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

        var primaryContext = ResilienceContext.Get();
        primaryContext.Properties.Set(beforeKey, "before");
        var contexts = new List<ResilienceContext>();
        var tokenHashCodes = new List<long>();

        ConfigureHedging<string>(handler =>
        {
            handler.HedgingActionGenerator = args =>
            {
                return async () =>
                {
                    tokenHashCodes.Add(args.Context.CancellationToken.GetHashCode());
                    args.Context.CancellationToken.CanBeCanceled.Should().BeTrue();
                    args.Context.Properties.GetValue(beforeKey, "wrong").Should().Be("before");
                    contexts.Add(args.Context);
                    await Task.Yield();
                    args.Context.Properties.Set(afterKey, "after");
                    return "secondary";
                };
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
        var primaryContext = ResilienceContext.Get();
        primaryContext.CancellationToken = cancellationSource.Token;
        ConfigureHedging(TimeSpan.Zero);
        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync((_, _) => Task.FromResult("primary"), primaryContext, "dummy");

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
        var primaryContext = ResilienceContext.Get();
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
                contexts.Add(args.Context);
                args.Context.Properties.GetValue(primaryKey, string.Empty).Should().Be("primary");
                args.Context.Properties.Set(secondaryKey, "secondary");
                await _timeProvider.Delay(TimeSpan.FromHours(1), args.Context.CancellationToken);
                return primaryFails ? Success : Failure;
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
            "state");

        // assert

        contexts.Should().HaveCount(2);
        primaryContext.Properties.Should().HaveCount(2);
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
        var primaryContext = ResilienceContext.Get();
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
                return Task.FromResult("primary");
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
        var primaryContext = ResilienceContext.Get();
        var storedProps = primaryContext.Properties;
        primaryContext.Properties.Set(key2, "my-value-2");
        ConfigureHedging(args =>
        {
            return () =>
            {
                args.Context.Properties.TryGetValue(key2, out var val).Should().BeTrue();
                val.Should().Be("my-value-2");
                args.Context.Properties.Set(key, "my-value");
                return Task.FromResult(Success);
            };
        });
        var strategy = Create();

        // act
        var result = await strategy.ExecuteAsync((_, _) => Task.FromResult(Failure), primaryContext, "state");

        // assert
        result.Should().Be(Success);
        primaryContext.Properties.TryGetValue(key, out var val).Should().BeTrue();
        primaryContext.Properties.Should().BeSameAs(storedProps);
        val.Should().Be("my-value");
    }

    [Fact]
    public async Task ExecuteAsync_CancellationLinking_Ok()
    {
        // arrange
        _options.MaxHedgedAttempts = 2;
        using var primaryCancelled = new ManualResetEvent(false);
        using var secondaryCancelled = new ManualResetEvent(false);
        using var cancellationSource = new CancellationTokenSource();
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellationSource.Token;

        ConfigureHedging(async context =>
        {
            try
            {
                await _timeProvider.Delay(TimeSpan.FromDays(0.5), context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                secondaryCancelled.Set();
                throw;
            }

            return Success;
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

        async Task<string> SlowTask(CancellationToken cancellationToken)
        {
            var delay = Task.Delay(TimeSpan.FromDays(1), cancellationToken);
            backgroundTasks.Add(delay);
            await delay;
            return Success;
        }

        Task<string> BackgroundWork(ResilienceContext resilienceContext)
        {
            var delay = Task.Delay(TimeSpan.FromDays(24), resilienceContext.CancellationToken);
            backgroundTasks.Add(delay);
            return Task.FromResult(Success);
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
        var task = Create().ExecuteAsync(Execute);

        // assert
        Assert.True(allExecutionsReached.WaitOne(AssertTimeout));
        _timeProvider.Advance(LongDelay);
        await task;

        async Task<string> Execute(CancellationToken token)
        {
            if (Interlocked.Increment(ref executions) == _options.MaxHedgedAttempts)
            {
                allExecutionsReached.Set();
            }

            await _timeProvider.Delay(LongDelay, token);
            return Success;
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

        async Task<string> Execute(CancellationToken token)
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

        ConfigureHedging<string>(handler =>
        {
            handler.ShouldHandle.HandleException<InvalidCastException>();
            handler.ShouldHandle.HandleException<ArgumentException>();
            handler.ShouldHandle.HandleException<InvalidOperationException>();
            handler.HedgingActionGenerator = args =>
            {
                Exception exception = args.Attempt switch
                {
                    1 => new ArgumentException(),
                    2 => new InvalidOperationException(),
                    3 => new BadImageFormatException(),
                    _ => new NotSupportedException()
                };

                attempts++;
                return () => throw exception;
            };
        });

        var strategy = Create();
        await strategy.Invoking(s => s.ExecuteAsync<string>(_ => throw new InvalidCastException())).Should().ThrowAsync<BadImageFormatException>();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionsHandled_ShouldThrowLastException()
    {
        int attempts = 0;

        ConfigureHedging<string>(handler =>
        {
            handler.ShouldHandle.HandleException<InvalidCastException>();
            handler.ShouldHandle.HandleException<ArgumentException>();
            handler.ShouldHandle.HandleException<InvalidOperationException>();
            handler.ShouldHandle.HandleException<BadImageFormatException>();
            handler.HedgingActionGenerator = args =>
            {
                Exception exception = args.Attempt switch
                {
                    1 => new ArgumentException(),
                    2 => new InvalidOperationException(),
                    3 => new BadImageFormatException(),
                    _ => new NotSupportedException()
                };

                attempts++;
                return () => throw exception;
            };
        });

        var strategy = Create();
        await strategy.Invoking(s => s.ExecuteAsync<string>(_ => throw new InvalidCastException())).Should().ThrowAsync<InvalidCastException>();
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_PrimaryExceptionNotHandled_Rethrow()
    {
        int attempts = 0;

        ConfigureHedging<string>(handler =>
        {
            handler.ShouldHandle.HandleException<InvalidOperationException>();
            handler.HedgingActionGenerator = args =>
            {
                attempts++;
                return null;
            };
        });

        var strategy = Create();
        await strategy.Invoking(s => s.ExecuteAsync<string>(_ => throw new InvalidCastException())).Should().ThrowAsync<InvalidCastException>();
        attempts.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionsHandled_ShouldReturnLastResult()
    {
        int attempts = 0;

        ConfigureHedging<string>(handler =>
        {
            handler.ShouldHandle.HandleException<InvalidCastException>();
            handler.ShouldHandle.HandleException<ArgumentException>();
            handler.ShouldHandle.HandleException<InvalidOperationException>();
            handler.ShouldHandle.HandleException<BadImageFormatException>();
            handler.HedgingActionGenerator = args =>
            {
                Exception? exception = args.Attempt switch
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
                        throw exception;
                    }

                    return Task.FromResult(Success);
                };
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
        _options.HedgingDelayGenerator.SetGenerator(_ => TimeSpan.FromMilliseconds(12345));

        ConfigureHedging<string>(handler =>
        {
            handler.HedgingActionGenerator = args => () => Task.FromResult(Success);
        });

        var strategy = Create();
        var task = strategy.ExecuteAsync<string>(async token =>
        {
            await _timeProvider.Delay(TimeSpan.FromDays(1), token);
            throw new InvalidCastException();
        });

        _timeProvider.Advance(TimeSpan.FromHours(5));
        (await task).Should().Be(Success);
        _timeProvider.DelayEntries.Should().Contain(e => e.Delay == delay);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureExceptionStackTracePreserved()
    {
        ConfigureHedging<string>(handler =>
        {
            handler.HedgingActionGenerator = args => null;
        });

        var strategy = Create();

        var exception = await strategy.Invoking(s => s.ExecuteAsync(PrimaryTaskThatThrowsError)).Should().ThrowAsync<InvalidOperationException>();

        exception.WithMessage("Forced Error");
        exception.And.StackTrace.Should().Contain(nameof(PrimaryTaskThatThrowsError));

        static Task<string> PrimaryTaskThatThrowsError(CancellationToken cancellationToken) => throw new InvalidOperationException("Forced Error");
    }

    [Fact]
    public async Task ExecuteAsync_EnsureOnHedgingCalled()
    {
        var attempts = new List<int>();
        _options.OnHedging.Register((o, args) =>
        {
            o.Result.Should().Be(Failure);
            attempts.Add(args.Attempt);
        });
        ConfigureHedging<string>(handler =>
        {
            handler.ShouldHandle.HandleResult(Failure);
            handler.HedgingActionGenerator = args => () => Task.FromResult(Failure);
        });

        var strategy = Create();
        await strategy.ExecuteAsync(_ => Task.FromResult(Failure));

        attempts.Should().HaveCount(_options.MaxHedgedAttempts);
        attempts.Should().BeInAscendingOrder();
        attempts[0].Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureOnHedgingTelemetry()
    {
        var context = ResilienceContext.Get();

        ConfigureHedging<string>(handler =>
        {
            handler.ShouldHandle.HandleResult(Failure);
            handler.HedgingActionGenerator = args => () => Task.FromResult(Failure);
        });

        var strategy = Create();
        await strategy.ExecuteAsync((_, _) => Task.FromResult(Failure), context, "state");

        context.ResilienceEvents.Should().HaveCount(_options.MaxHedgedAttempts);
        context.ResilienceEvents.Select(v => v.EventName).Distinct().Should().ContainSingle("OnHedging");
    }

    private void ConfigureHedging()
    {
        _options.OnHedging.Register<string>((outcome, _) =>
        {
            lock (_results)
            {
                _results.Add(outcome.Result!);
            }
        });

        ConfigureHedging<string>(handler =>
        {
            handler.HedgingActionGenerator = _actions.Generator;
        });
    }

    private void ConfigureHedging(Func<ResilienceContext, Task<string>> background)
    {
        ConfigureHedging(args => () => background(args.Context));
    }

    private void ConfigureHedging(HedgingActionGenerator<string> generator)
    {
        ConfigureHedging<string>(handler =>
        {
            handler.HedgingActionGenerator = generator;
            handler.ShouldHandle.HandleResult(Failure);
        });
    }

    private void ConfigureHedging<T>(Action<HedgingHandler<T>> configure) => _options.Handler.SetHedging(configure);

    private void ConfigureHedging(TimeSpan delay) => ConfigureHedging(args => async () =>
    {
        await Task.Delay(delay);
        return "secondary";
    });

    private HedgingResilienceStrategy Create() => new(_options, _timeProvider, _telemetry);
}
