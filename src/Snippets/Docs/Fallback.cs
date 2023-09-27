using System.Net;
using System.Net.Http;
using Polly.Fallback;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static class Fallback
{
    public static void Usage()
    {
        #region fallback

        // Add a fallback/substitute value if an operation fails.
        new ResiliencePipelineBuilder<UserAvatar>()
            .AddFallback(new FallbackStrategyOptions<UserAvatar>
            {
                ShouldHandle = new PredicateBuilder<UserAvatar>()
                    .Handle<SomeExceptionType>()
                    .HandleResult(r => r is null),
                FallbackAction = args => Outcome.FromResultAsValueTask(UserAvatar.Blank)
            });

        // Use a dynamically generated value if an operation fails.
        new ResiliencePipelineBuilder<UserAvatar>()
            .AddFallback(new FallbackStrategyOptions<UserAvatar>
            {
                ShouldHandle = new PredicateBuilder<UserAvatar>()
                    .Handle<SomeExceptionType>()
                    .HandleResult(r => r is null),
                FallbackAction = args =>
                {
                    var avatar = UserAvatar.GetRandomAvatar();
                    return Outcome.FromResultAsValueTask(avatar);
                }
            });

        // Use a default or dynamically generated value, and execute an additional action if the fallback is triggered.
        new ResiliencePipelineBuilder<UserAvatar>()
            .AddFallback(new FallbackStrategyOptions<UserAvatar>
            {
                ShouldHandle = new PredicateBuilder<UserAvatar>()
                    .Handle<SomeExceptionType>()
                    .HandleResult(r => r is null),
                FallbackAction = args =>
                {
                    var avatar = UserAvatar.GetRandomAvatar();
                    return Outcome.FromResultAsValueTask(UserAvatar.Blank);
                },
                OnFallback = args =>
                {
                    // Add extra logic to be executed when the fallback is triggered, such as logging.
                    return default; // Returns an empty ValueTask
                }
            });

        #endregion
    }

    public class UserAvatar
    {
        public static readonly UserAvatar Blank = new();

        public static UserAvatar GetRandomAvatar() => new();
    }

    private class CustomNetworkException : Exception
    {
        public CustomNetworkException()
        {
        }

        public CustomNetworkException(string message)
        : base(message)
        {
        }

        public CustomNetworkException(string message, Exception innerException)
        : base(message, innerException)
        {
        }
    }

    public static void AntiPattern_1()
    {
        #region fallback-anti-pattern-1

        var fallback = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddFallback(new()
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<HttpRequestException>(),
                FallbackAction = args => Outcome.FromResultAsValueTask(new HttpResponseMessage()),
                OnFallback = args => throw new CustomNetworkException("Replace thrown exception", args.Outcome.Exception!)
            })
            .Build();

        #endregion
    }

    private static readonly ResiliencePipeline<HttpResponseMessage> WhateverPipeline = ResiliencePipeline<HttpResponseMessage>.Empty;
    private static ValueTask<Outcome<HttpResponseMessage>> Action(ResilienceContext context, string state) => Outcome.FromResultAsValueTask(new HttpResponseMessage());
    public static async Task Pattern_1()
    {
        var context = ResilienceContextPool.Shared.Get();
        #region fallback-pattern-1

        var outcome = await WhateverPipeline.ExecuteOutcomeAsync(Action, context, "state");
        if (outcome.Exception is HttpRequestException httpEx)
        {
            throw new CustomNetworkException("Replace thrown exception", httpEx);
        }
        #endregion

        ResilienceContextPool.Shared.Return(context);
    }

    #region fallback-pattern-1-ext
    public static async ValueTask<HttpResponseMessage> Action()
    {
        var context = ResilienceContextPool.Shared.Get();
        var outcome = await WhateverPipeline.ExecuteOutcomeAsync<HttpResponseMessage, string>(
            async (ctx, state) =>
            {
                var result = await ActionCore();
                return Outcome.FromResult(result);
            }, context, "state");

        if (outcome.Exception is HttpRequestException hre)
        {
            throw new CustomNetworkException("Replace thrown exception", hre);
        }

        ResilienceContextPool.Shared.Return(context);
        return outcome.Result!;
    }

    private static ValueTask<HttpResponseMessage> ActionCore()
    {
        // The core logic
        return ValueTask.FromResult(new HttpResponseMessage());
    }
    #endregion

    private static ValueTask<HttpResponseMessage> CallPrimary(CancellationToken ct) => ValueTask.FromResult(new HttpResponseMessage());
    private static ValueTask<HttpResponseMessage> CallSecondary(CancellationToken ct) => ValueTask.FromResult(new HttpResponseMessage());
    public static async Task<HttpResponseMessage?> AntiPattern_2()
    {
        var fallbackKey = new ResiliencePropertyKey<HttpResponseMessage?>("fallback_result");

        #region fallback-anti-pattern-2

        var fallback = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(res => res.StatusCode == HttpStatusCode.RequestTimeout),
                MaxRetryAttempts = 1,
                OnRetry = async args =>
                {
                    args.Context.Properties.Set(fallbackKey, await CallSecondary(args.Context.CancellationToken));
                }
            })
            .Build();

        var context = ResilienceContextPool.Shared.Get();
        var outcome = await fallback.ExecuteOutcomeAsync<HttpResponseMessage, string>(
            async (ctx, state) =>
            {
                var result = await CallPrimary(ctx.CancellationToken);
                return Outcome.FromResult(result);
            }, context, "none");

        var result = outcome.Result is not null
            ? outcome.Result
            : context.Properties.GetValue(fallbackKey, default);

        ResilienceContextPool.Shared.Return(context);

        return result;

        #endregion
    }

    public static async ValueTask<HttpResponseMessage?> Pattern_2()
    {
        #region fallback-pattern-2

        var fallback = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddFallback(new()
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(res => res.StatusCode == HttpStatusCode.RequestTimeout),
                OnFallback = async args => await CallSecondary(args.Context.CancellationToken)
            })
            .Build();

        return await fallback.ExecuteAsync(CallPrimary, CancellationToken.None);

        #endregion
    }

    private static ValueTask<HttpResponseMessage> CallExternalSystem(CancellationToken ct) => ValueTask.FromResult(new HttpResponseMessage());
    public static async ValueTask<HttpResponseMessage?> AntiPattern_3()
    {
        var timeout = ResiliencePipeline<HttpResponseMessage>.Empty;
        var fallback = ResiliencePipeline<HttpResponseMessage>.Empty;

        #region fallback-anti-pattern-3
        var result = await fallback.ExecuteAsync(async (CancellationToken outerCT) =>
        {
            return await timeout.ExecuteAsync(static async (CancellationToken innerCT) =>
            {
                return await CallExternalSystem(innerCT);
            }, outerCT);
        }, CancellationToken.None);

        return result;
        #endregion
    }

    public static async ValueTask<HttpResponseMessage?> Pattern_3()
    {
        var timeout = ResiliencePipeline<HttpResponseMessage>.Empty;
        var fallback = ResiliencePipeline<HttpResponseMessage>.Empty;

        #region fallback-pattern-3
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddPipeline(timeout)
            .AddPipeline(fallback)
            .Build();

        return await pipeline.ExecuteAsync(CallExternalSystem, CancellationToken.None);
        #endregion
    }
}
