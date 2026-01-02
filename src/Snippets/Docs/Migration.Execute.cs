using Polly.Timeout;

namespace Snippets.Docs;

internal static partial class Migration
{
    private static int Method() => 3;
    private static Task<int> MethodAsync(CancellationToken token) => Task.FromResult(3);
    public static async Task SafeExecute_V7()
    {
        #region migration-execute-v7
        // Synchronous execution
        ISyncPolicy<int> syncPolicy = Policy.Timeout<int>(TimeSpan.FromSeconds(1));
        PolicyResult<int> policyResult = syncPolicy.ExecuteAndCapture(Method);

        // Asynchronous execution
        IAsyncPolicy<int> asyncPolicy = Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(1));
        PolicyResult<int> asyncPolicyResult = await asyncPolicy.ExecuteAndCaptureAsync(MethodAsync, CancellationToken.None);

        // Assess policy result
        if (policyResult.Outcome == OutcomeType.Successful)
        {
            int result = policyResult.Result;

            // Process result
        }
        else
        {
            Exception exception = policyResult.FinalException;
            FaultType faultType = policyResult.FaultType!.Value;
            ExceptionType exceptionType = policyResult.ExceptionType!.Value;

            // Process failure
        }

        // Access context
        const string Key = "context_key";
        IAsyncPolicy<int> asyncPolicyWithContext = Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(10),
            onTimeoutAsync: (ctx, ts, task) =>
            {
                ctx[Key] = "context_value";
                return Task.CompletedTask;
            });

        asyncPolicyResult = await asyncPolicyWithContext.ExecuteAndCaptureAsync((ctx, token) => MethodAsync(token), new Context(), CancellationToken.None);
        string? ctxValue = asyncPolicyResult.Context.GetValueOrDefault(Key) as string;
        #endregion
    }

    public static async Task SafeExecute_V8()
    {
        #region migration-execute-v8
        ResiliencePipeline<int> pipeline = new ResiliencePipelineBuilder<int>()
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        // Synchronous execution
        // Polly v8 does not support

        // Asynchronous execution
        var context = ResilienceContextPool.Shared.Get();

        Outcome<int> pipelineResult =
            await pipeline.ExecuteOutcomeAsync(
                static async (ctx, state) =>
                {
                    try
                    {
                        return Outcome.FromResult(await MethodAsync(ctx.CancellationToken));
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException<int>(e);
                    }
                },
                context,
                "state");

        ResilienceContextPool.Shared.Return(context);

        // Assess policy result
        if (pipelineResult.Exception is null)
        {
            int result = pipelineResult.Result;

            // Process result
        }
        else
        {
            Exception exception = pipelineResult.Exception;
            // Process failure

            // If needed you can rethrow the exception
            pipelineResult.ThrowIfException();
        }

        // Access context
        ResiliencePropertyKey<string> contextKey = new("context_key");
        ResiliencePipeline<int> pipelineWithContext = new ResiliencePipelineBuilder<int>()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(1),
                OnTimeout = args =>
                {
                    args.Context.Properties.Set(contextKey, "context_value");
                    return default;
                }
            })
            .Build();

        context = ResilienceContextPool.Shared.Get();

        pipelineResult =
            await pipelineWithContext.ExecuteOutcomeAsync(
                static async (ctx, state) =>
                {
                    try
                    {
                        return Outcome.FromResult(await MethodAsync(ctx.CancellationToken));
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException<int>(e);
                    }
                },
                context,
                "state");

        context.Properties.TryGetValue(contextKey, out var ctxValue);
        ResilienceContextPool.Shared.Return(context);
        #endregion
    }
}
