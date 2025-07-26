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

        // A fallback/substitute value if an operation fails.
        var optionsSubstitute = new FallbackStrategyOptions<UserAvatar>
        {
            ShouldHandle = new PredicateBuilder<UserAvatar>()
                .Handle<SomeExceptionType>()
                .HandleResult(r => r is null),
            FallbackAction = static args => Outcome.FromResultAsValueTask(UserAvatar.Blank)
        };

        // Use a dynamically generated value if an operation fails.
        var optionsFallbackAction = new FallbackStrategyOptions<UserAvatar>
        {
            ShouldHandle = new PredicateBuilder<UserAvatar>()
                .Handle<SomeExceptionType>()
                .HandleResult(r => r is null),
            FallbackAction = static args =>
            {
                var avatar = UserAvatar.GetRandomAvatar();
                return Outcome.FromResultAsValueTask(avatar);
            }
        };

        // Use a default or dynamically generated value, and execute an additional action if the fallback is triggered.
        var optionsOnFallback = new FallbackStrategyOptions<UserAvatar>
        {
            ShouldHandle = new PredicateBuilder<UserAvatar>()
                .Handle<SomeExceptionType>()
                .HandleResult(r => r is null),
            FallbackAction = static args =>
            {
                var avatar = UserAvatar.GetRandomAvatar();
                return Outcome.FromResultAsValueTask(UserAvatar.Blank);
            },
            OnFallback = static args =>
            {
                // Add extra logic to be executed when the fallback is triggered, such as logging.
                return default; // Returns an empty ValueTask
            }
        };

        // Add a fallback strategy with a FallbackStrategyOptions<TResult> instance to the pipeline
        new ResiliencePipelineBuilder<UserAvatar>().AddFallback(optionsOnFallback);

        #endregion
    }

    public sealed class UserAvatar
    {
        public static readonly UserAvatar Blank = new();

        public static UserAvatar GetRandomAvatar() => new();
    }

    private sealed class CustomNetworkException : Exception
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

    public static void AntiPattern_ReplaceException()
    {
        #region fallback-anti-pattern-replace-exception

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
    public static async Task Pattern_ReplaceException()
    {
        var context = ResilienceContextPool.Shared.Get();
        #region fallback-pattern-replace-exception

        var outcome = await WhateverPipeline.TryExecuteAsync(Action, context, "state");
        if (outcome.Exception is HttpRequestException requestException)
        {
            throw new CustomNetworkException("Replace thrown exception", requestException);
        }
        #endregion

        ResilienceContextPool.Shared.Return(context);
    }

    #region fallback-pattern-replace-exception-ext
    public static async ValueTask<HttpResponseMessage> Action()
    {
        var context = ResilienceContextPool.Shared.Get();
        var outcome = await WhateverPipeline.TryExecuteAsync<HttpResponseMessage, string>(
            async (ctx, state) => await ActionCore(),
            context,
            "state");

        if (outcome.Exception is HttpRequestException requestException)
        {
            throw new CustomNetworkException("Replace thrown exception", requestException);
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

    private static ValueTask<HttpResponseMessage> CallPrimary(CancellationToken ct) => ValueTask.FromResult(new HttpResponseMessage(HttpStatusCode.RequestTimeout));
    private static ValueTask<HttpResponseMessage> CallSecondary(CancellationToken ct) => ValueTask.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    public static async Task<HttpResponseMessage?> AntiPattern_RetryForFallback()
    {
        var fallbackKey = new ResiliencePropertyKey<HttpResponseMessage?>("fallback_result");

        #region fallback-anti-pattern-retry-for-fallback

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
        var outcome = await fallback.TryExecuteAsync<HttpResponseMessage, string>(
            async (ctx, state) => await CallPrimary(ctx.CancellationToken),
            context,
            "none");

        var result = outcome.Result is not null
            ? outcome.Result
            : context.Properties.GetValue(fallbackKey, default);

        ResilienceContextPool.Shared.Return(context);

        return result;

        #endregion
    }

    public static async ValueTask<HttpResponseMessage?> Pattern_RetryForFallback()
    {
        #region fallback-pattern-retry-for-fallback

        var fallback = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddFallback(new()
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(res => res.StatusCode == HttpStatusCode.RequestTimeout),
                FallbackAction = async args => Outcome.FromResult(await CallSecondary(args.Context.CancellationToken))
            })
            .Build();

        return await fallback.ExecuteAsync(CallPrimary, CancellationToken.None);

        #endregion
    }

    private static ValueTask<HttpResponseMessage> CallExternalSystem(CancellationToken ct) => ValueTask.FromResult(new HttpResponseMessage());
    public static async ValueTask<HttpResponseMessage?> AntiPattern_NestingExecute()
    {
        var timeout = ResiliencePipeline<HttpResponseMessage>.Empty;
        var fallback = ResiliencePipeline<HttpResponseMessage>.Empty;

        #region fallback-anti-pattern-nesting-execute
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

    public static async ValueTask<HttpResponseMessage?> Pattern_NestingExecute()
    {
        var timeout = ResiliencePipeline<HttpResponseMessage>.Empty;
        var fallback = ResiliencePipeline<HttpResponseMessage>.Empty;

        #region fallback-pattern-nesting-execute
        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddPipeline(timeout)
            .AddPipeline(fallback)
            .Build();

        return await pipeline.ExecuteAsync(CallExternalSystem, CancellationToken.None);
        #endregion
    }

    public static void Pattern_FallbackAfterRetries()
    {
        #region fallback-pattern-after-retries

        // Define a common predicates re-used by both fallback and retries
        var predicateBuilder = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => r.StatusCode == HttpStatusCode.InternalServerError);

        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddFallback(new()
            {
                ShouldHandle = predicateBuilder,
                FallbackAction = args =>
                {
                    // Try to resolve the fallback response
                    HttpResponseMessage fallbackResponse = ResolveFallbackResponse(args.Outcome);

                    return Outcome.FromResultAsValueTask(fallbackResponse);
                }
            })
            .AddRetry(new()
            {
                ShouldHandle = predicateBuilder,
                MaxRetryAttempts = 3,
            })
            .Build();

        // Demonstrative execution that always produces invalid result
        pipeline.Execute(() => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        #endregion
    }

    private static HttpResponseMessage ResolveFallbackResponse(Outcome<HttpResponseMessage> outcome) => new();
}
