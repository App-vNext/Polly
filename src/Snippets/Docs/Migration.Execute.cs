using Polly.Timeout;

namespace Snippets.Docs;

internal static partial class Migration
{
    private static int Method() => 3;
    private static Task<int> MethodAsync() => Task.FromResult(3);
    public static async Task SafeExecute_V7()
    {
        #region migration-execute-v7
        // Synchronous execution
        ISyncPolicy<int> syncPolicy = Policy.Timeout<int>(TimeSpan.FromSeconds(1));
        PolicyResult<int> policyResult = syncPolicy.ExecuteAndCapture(Method);

        // Asynchronous execution
        IAsyncPolicy<int> asyncPolicy = Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(1));
        PolicyResult<int> asyncPolicyResult = await asyncPolicy.ExecuteAndCaptureAsync(MethodAsync);

        // Assess policy result
        if (policyResult.Outcome == OutcomeType.Successful)
        {
            var result = policyResult.Result;
            // process result
        }
        else
        {
            var exception = policyResult.FinalException;
            FaultType failtType = policyResult.FaultType!.Value;
            ExceptionType exceptionType = policyResult.ExceptionType!.Value;
            // process failure
        }

        // Access context
        IAsyncPolicy<int> asyncPolicyWithContext = Policy.TimeoutAsync<int>(TimeSpan.FromSeconds(10),
            onTimeoutAsync: (ctx, ts, task) =>
            {
                ctx["context_key"] = "context_value";
                return Task.CompletedTask;
            });

        asyncPolicyResult = await asyncPolicyWithContext.ExecuteAndCaptureAsync((ctx) => MethodAsync(), new Context());
        var ctxValue = asyncPolicyResult.Context.GetValueOrDefault("context_key");
        #endregion
    }

    public static async Task SafeExecute_V8()
    {
        #region migration-execute-v8
        ResiliencePipeline<int> pipeline = new ResiliencePipelineBuilder<int>()
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        // Synchronous execution
        // Polly v8 does not provide an API to synchronously execute and capture the outcome of a pipeline

        // Asynchronous execution
        var context = ResilienceContextPool.Shared.Get();
        Outcome<int> pipelineResult = await pipeline.ExecuteOutcomeAsync(
            static async (ctx, state) => Outcome.FromResult(await MethodAsync()), context, "state");
        ResilienceContextPool.Shared.Return(context);

        // Assess policy result
        if (pipelineResult.Exception is null)
        {
            var result = pipelineResult.Result;
            // process result
        }
        else
        {
            var exception = pipelineResult.Exception;
            // process failure

            // if needed you can rethrow the exception
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
        pipelineResult = await pipelineWithContext.ExecuteOutcomeAsync(
            static async (ctx, state) => Outcome.FromResult(await MethodAsync()), context, "state");

        context.Properties.TryGetValue(contextKey, out var ctxValue);
        ResilienceContextPool.Shared.Return(context);
        #endregion
    }
}
