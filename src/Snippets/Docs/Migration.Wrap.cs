namespace Snippets.Docs;

internal static partial class Migration
{
    public static void PolicyWrap_V7()
    {
        #region migration-policy-wrap-v7

        IAsyncPolicy retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1));

        IAsyncPolicy timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(3));

        // Wrap the policies. The policies are executed in the following order (i.e. Last-In-First-Out):
        // 1. Retry << outer
        // 2. Timeout << inner
        IAsyncPolicy wrappedPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);

        #endregion
    }

    public static void PolicyWrap_V8()
    {
        #region migration-policy-wrap-v8

        // The "PolicyWrap" is integrated directly. Strategies are executed in the same order as they were added (i.e. First-In-First-Out):
        // 1. Retry << outer
        // 2. Timeout << outer
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Constant,
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .AddTimeout(TimeSpan.FromSeconds(3))
            .Build();

        #endregion
    }
}
