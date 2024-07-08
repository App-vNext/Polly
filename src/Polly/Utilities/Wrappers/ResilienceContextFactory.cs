namespace Polly.Utilities.Wrappers;

internal static class ResilienceContextFactory
{
    public static ResilienceContext Create(
        Context context,
        bool continueOnCapturedContext,
        out IDictionary<string, object> oldProperties,
        CancellationToken cancellationToken)
    {
        var resilienceContext = ResilienceContextPool.Shared.Get(context.OperationKey, continueOnCapturedContext, cancellationToken);
        resilienceContext.Properties.SetProperties(context, out oldProperties);

        return resilienceContext;
    }

    public static void Cleanup(ResilienceContext resilienceContext, IDictionary<string, object> oldProperties)
    {
        resilienceContext.Properties.SetProperties(oldProperties, out _);
        ResilienceContextPool.Shared.Return(resilienceContext);
    }
}
