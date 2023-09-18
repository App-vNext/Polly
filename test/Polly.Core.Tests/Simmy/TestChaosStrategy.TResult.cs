﻿using Polly.Simmy;

namespace Polly.Core.Tests.Simmy;
public sealed class TestChaosStrategy<T> : MonkeyStrategy<T>
{
    public TestChaosStrategy(TestChaosStrategyOptions<T> options)
        : base(options)
    {
    }

    public Action<ResilienceContext, object?>? Before { get; set; }

    public Action<object?, Exception?>? After { get; set; }

    public Func<ResilienceContext, object?, Task>? OnExecute { get; set; }

    protected internal override async ValueTask<Outcome<T>> ExecuteCore<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback, ResilienceContext context, TState state)
    {
        Before?.Invoke(context, state);

        try
        {
            if (await ShouldInjectAsync(context).ConfigureAwait(context.ContinueOnCapturedContext))
            {
                if (OnExecute != null)
                {
                    await OnExecute(context, state).ConfigureAwait(false);
                }
            }

            var result = await callback(context, state).ConfigureAwait(false);

            After?.Invoke(result, null);

            return result;
        }
        catch (Exception e)
        {
            After?.Invoke(null, e);

            throw;
        }
    }
}
