﻿using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Registry;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static class Performance
{
    public static async Task Lambda()
    {
        var resiliencePipeline = ResiliencePipeline.Empty;
        var userId = string.Empty;
        var cancellationToken = CancellationToken.None;

        #region perf-lambdas

        // This call allocates for each invocation since the "userId" variable is captured from the outer scope.
        await resiliencePipeline.ExecuteAsync(
            cancellationToken => GetMemberAsync(userId, cancellationToken),
            cancellationToken);

        // This approach uses a static lambda, avoiding allocations.
        // The "userId" is stored as state, and the lambda reads it.
        await resiliencePipeline.ExecuteAsync(
            static (state, cancellationToken) => GetMemberAsync(state, cancellationToken),
            userId,
            cancellationToken);

        #endregion
    }

    public static async Task SwitchExpressions()
    {
        #region perf-switch-expressions

        // Here, PredicateBuilder is used to configure which exceptions the retry strategy should handle.
        new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<SomeExceptionType>()
                    .Handle<InvalidOperationException>()
                    .Handle<HttpRequestException>()
            })
            .Build();

        // For optimal performance, it's recommended to use switch expressions over PredicateBuilder.
        new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                ShouldHandle = args => args.Outcome.Exception switch
                {
                    SomeExceptionType => PredicateResult.True(),
                    InvalidOperationException => PredicateResult.True(),
                    HttpRequestException => PredicateResult.True(),
                    _ => PredicateResult.False()
                }
            })
            .Build();

        #endregion
    }

    public static async Task ExecuteOutcomeAsync()
    {
        var pipeline = ResiliencePipeline.Empty;
        var cancellationToken = CancellationToken.None;
        var logger = NullLogger.Instance;
        var id = "id";

        #region perf-execute-outcome

        // Execute GetMemberAsync and handle exceptions externally.
        try
        {
            await pipeline.ExecuteAsync(cancellationToken => GetMemberAsync(id, cancellationToken), cancellationToken);
        }
        catch (Exception e)
        {
            // Log the exception here.
            logger.LogWarning(e, "Failed to get member with id '{id}'.", id);
        }

        // The example above can be restructured as:

        // Acquire a context from the pool
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);

        // Instead of wrapping pipeline execution with try-catch, use ExecuteOutcomeAsync(...).
        // Certain strategies are optimized for this method, returning an exception instance without actually throwing it.
        Outcome<Member> outcome = await pipeline.ExecuteOutcomeAsync(
            static async (context, state) =>
            {
                // The callback for ExecuteOutcomeAsync must return an Outcome<T> instance. Hence, some wrapping is needed.
                try
                {
                    return Outcome.FromResult(await GetMemberAsync(state, context.CancellationToken));
                }
                catch (Exception e)
                {
                    return Outcome.FromException<Member>(e);
                }
            },
            context,
            id);

        // Handle exceptions using the Outcome<T> instance instead of try-catch.
        if (outcome.Exception is not null)
        {
            logger.LogWarning(outcome.Exception, "Failed to get member with id '{id}'.", id);
        }

        // Release the context back to the pool
        ResilienceContextPool.Shared.Return(context);

        #endregion
    }

    private static ValueTask<Member> GetMemberAsync(string id, CancellationToken token) => default;

    public class Member
    {
    }

    #region perf-reuse-pipelines

    public class MyApi
    {
        private readonly ResiliencePipelineRegistry<string> _registry;

        // Share a single instance of the registry throughout your application.
        public MyApi(ResiliencePipelineRegistry<string> registry)
        {
            _registry = registry;
        }

        public async Task UpdateData(CancellationToken cancellationToken)
        {
            // Get or create and cache the pipeline for subsequent use.
            // Choose a sufficiently unique key to prevent collisions.
            var pipeline = _registry.GetOrAddPipeline("my-app.my-api", builder =>
            {
                builder.AddRetry(new()
                {
                    ShouldHandle = new PredicateBuilder()
                        .Handle<InvalidOperationException>()
                        .Handle<HttpRequestException>()
                });
            });

            await pipeline.ExecuteAsync(async token =>
            {
                // Place your logic here
            },
            cancellationToken);
        }
    }

    #endregion
}
