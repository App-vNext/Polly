namespace Polly.Specs.Timeout;

[Collection(Constants.SystemClockDependentTestCollection)]
public class TimeoutOverloadSmokeSpecs : TimeoutSpecsBase
{
    [Fact]
    public void Sync_timeout_overloads_should_execute_and_timeout()
    {
        var executions = new Action[]
        {
            () => ShouldTimeout(Policy.Timeout(1)),
            () => ShouldTimeout(Policy.Timeout(1, TimeoutStrategy.Optimistic)),
            () => ShouldTimeout(Policy.Timeout(TimeSpan.FromSeconds(1))),
            () => ShouldTimeout(Policy.Timeout(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic)),
            () => ShouldTimeout(Policy.Timeout(() => TimeSpan.FromSeconds(1))),
            () => ShouldTimeout(Policy.Timeout(() => TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic)),
            () =>
            {
                bool onTimeoutCalled = false;
                ShouldTimeout(Policy.Timeout(() => TimeSpan.FromSeconds(1), (_, _, _) => onTimeoutCalled = true));
                onTimeoutCalled.ShouldBeTrue();
            },
            () =>
            {
                bool onTimeoutCalled = false;
                ShouldTimeout(Policy.Timeout(() => TimeSpan.FromSeconds(1), (_, _, _, _) => onTimeoutCalled = true));
                onTimeoutCalled.ShouldBeTrue();
            },
            () =>
            {
                bool onTimeoutCalled = false;
                ShouldTimeout(Policy.Timeout(_ => TimeSpan.FromSeconds(1), (_, _, _) => onTimeoutCalled = true), new Context("operation-key"));
                onTimeoutCalled.ShouldBeTrue();
            },
            () =>
            {
                bool onTimeoutCalled = false;
                ShouldTimeout(Policy.Timeout(_ => TimeSpan.FromSeconds(1), (_, _, _, _) => onTimeoutCalled = true), new Context("operation-key"));
                onTimeoutCalled.ShouldBeTrue();
            }
        };

        foreach (Action execution in executions)
        {
            execution();
        }
    }

    [Fact]
    public async Task Async_timeout_overloads_should_execute_and_timeout()
    {
        var executions = new Func<Task>[]
        {
            () => ShouldTimeoutAsync(Policy.TimeoutAsync(1)),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync(1, TimeoutStrategy.Optimistic)),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync(TimeSpan.FromSeconds(1))),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic)),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync(() => TimeSpan.FromSeconds(1))),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync(() => TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic)),
            async () =>
            {
                bool onTimeoutCalled = false;
                await ShouldTimeoutAsync(Policy.TimeoutAsync(() => TimeSpan.FromSeconds(1), (_, _, _) =>
                {
                    onTimeoutCalled = true;
                    return TaskHelper.EmptyTask;
                }));
                onTimeoutCalled.ShouldBeTrue();
            },
            async () =>
            {
                bool onTimeoutCalled = false;
                await ShouldTimeoutAsync(Policy.TimeoutAsync(() => TimeSpan.FromSeconds(1), (_, _, _, _) =>
                {
                    onTimeoutCalled = true;
                    return TaskHelper.EmptyTask;
                }));
                onTimeoutCalled.ShouldBeTrue();
            },
            async () =>
            {
                bool onTimeoutCalled = false;
                await ShouldTimeoutAsync(Policy.TimeoutAsync(_ => TimeSpan.FromSeconds(1), (_, _, _) =>
                {
                    onTimeoutCalled = true;
                    return TaskHelper.EmptyTask;
                }), new Context("operation-key"));
                onTimeoutCalled.ShouldBeTrue();
            },
            async () =>
            {
                bool onTimeoutCalled = false;
                await ShouldTimeoutAsync(Policy.TimeoutAsync(_ => TimeSpan.FromSeconds(1), (_, _, _, _) =>
                {
                    onTimeoutCalled = true;
                    return TaskHelper.EmptyTask;
                }), new Context("operation-key"));
                onTimeoutCalled.ShouldBeTrue();
            }
        };

        foreach (Func<Task> execution in executions)
        {
            await execution();
        }
    }

    [Fact]
    public void Generic_timeout_overloads_should_execute_and_timeout()
    {
        var executions = new Action[]
        {
            () => ShouldTimeout(Policy.Timeout<ResultPrimitive>(1)),
            () => ShouldTimeout(Policy.Timeout<ResultPrimitive>(1, TimeoutStrategy.Optimistic)),
            () => ShouldTimeout(Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(1))),
            () => ShouldTimeout(Policy.Timeout<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic)),
            () => ShouldTimeout(Policy.Timeout<ResultPrimitive>(() => TimeSpan.FromSeconds(1))),
            () => ShouldTimeout(Policy.Timeout<ResultPrimitive>(() => TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic)),
            () =>
            {
                bool onTimeoutCalled = false;
                ShouldTimeout(Policy.Timeout<ResultPrimitive>(() => TimeSpan.FromSeconds(1), (_, _, _) => onTimeoutCalled = true));
                onTimeoutCalled.ShouldBeTrue();
            },
            () =>
            {
                bool onTimeoutCalled = false;
                ShouldTimeout(Policy.Timeout<ResultPrimitive>(() => TimeSpan.FromSeconds(1), (_, _, _, _) => onTimeoutCalled = true));
                onTimeoutCalled.ShouldBeTrue();
            },
            () =>
            {
                bool onTimeoutCalled = false;
                ShouldTimeout(Policy.Timeout<ResultPrimitive>(_ => TimeSpan.FromSeconds(1), (_, _, _) => onTimeoutCalled = true), new Context("operation-key"));
                onTimeoutCalled.ShouldBeTrue();
            },
            () =>
            {
                bool onTimeoutCalled = false;
                ShouldTimeout(Policy.Timeout<ResultPrimitive>(_ => TimeSpan.FromSeconds(1), (_, _, _, _) => onTimeoutCalled = true), new Context("operation-key"));
                onTimeoutCalled.ShouldBeTrue();
            }
        };

        foreach (Action execution in executions)
        {
            execution();
        }
    }

    [Fact]
    public async Task Generic_async_timeout_overloads_should_execute_and_timeout()
    {
        var executions = new Func<Task>[]
        {
            () => ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(1)),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(1, TimeoutStrategy.Optimistic)),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(1))),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic)),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(1))),
            () => ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(1), TimeoutStrategy.Optimistic)),
            async () =>
            {
                bool onTimeoutCalled = false;
                await ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(1), (_, _, _) =>
                {
                    onTimeoutCalled = true;
                    return TaskHelper.EmptyTask;
                }));
                onTimeoutCalled.ShouldBeTrue();
            },
            async () =>
            {
                bool onTimeoutCalled = false;
                await ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(() => TimeSpan.FromSeconds(1), (_, _, _, _) =>
                {
                    onTimeoutCalled = true;
                    return TaskHelper.EmptyTask;
                }));
                onTimeoutCalled.ShouldBeTrue();
            },
            async () =>
            {
                bool onTimeoutCalled = false;
                await ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(_ => TimeSpan.FromSeconds(1), (_, _, _) =>
                {
                    onTimeoutCalled = true;
                    return TaskHelper.EmptyTask;
                }), new Context("operation-key"));
                onTimeoutCalled.ShouldBeTrue();
            },
            async () =>
            {
                bool onTimeoutCalled = false;
                await ShouldTimeoutAsync(Policy.TimeoutAsync<ResultPrimitive>(_ => TimeSpan.FromSeconds(1), (_, _, _, _) =>
                {
                    onTimeoutCalled = true;
                    return TaskHelper.EmptyTask;
                }), new Context("operation-key"));
                onTimeoutCalled.ShouldBeTrue();
            }
        };

        foreach (Func<Task> execution in executions)
        {
            await execution();
        }
    }

    private static void ShouldTimeout(TimeoutPolicy policy, Context? context = null)
    {
        Action action = context is null
            ? () => policy.Execute((ct) => SystemClock.Sleep(TimeSpan.FromSeconds(2), ct), CancellationToken.None)
            : () => policy.Execute((_, ct) => SystemClock.Sleep(TimeSpan.FromSeconds(2), ct), context, CancellationToken.None);

        Should.Throw<TimeoutRejectedException>(() => action());
    }

    private static void ShouldTimeout<TResult>(TimeoutPolicy<TResult> policy, Context? context = null)
    {
        Func<TResult> action = context is null
            ? () => policy.Execute((ct) =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(2), ct);
                return default!;
            }, CancellationToken.None)
            : () => policy.Execute((_, ct) =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(2), ct);
                return default!;
            }, context, CancellationToken.None);

        Should.Throw<TimeoutRejectedException>(() => action());
    }

    private static async Task ShouldTimeoutAsync(AsyncTimeoutPolicy policy, Context? context = null)
    {
        Func<Task> action = context is null
            ? () => policy.ExecuteAsync((ct) =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(2), ct);
                return TaskHelper.EmptyTask;
            }, CancellationToken.None)
            : () => policy.ExecuteAsync((_, ct) =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(2), ct);
                return TaskHelper.EmptyTask;
            }, context, CancellationToken.None);

        await Should.ThrowAsync<TimeoutRejectedException>(action);
    }

    private static async Task ShouldTimeoutAsync<TResult>(AsyncTimeoutPolicy<TResult> policy, Context? context = null)
    {
        Func<Task<TResult>> action = context is null
            ? () => policy.ExecuteAsync((ct) =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(2), ct);
                return Task.FromResult(default(TResult)!);
            }, CancellationToken.None)
            : () => policy.ExecuteAsync((_, ct) =>
            {
                SystemClock.Sleep(TimeSpan.FromSeconds(2), ct);
                return Task.FromResult(default(TResult)!);
            }, context, CancellationToken.None);

        await Should.ThrowAsync<TimeoutRejectedException>(action);
    }
}
