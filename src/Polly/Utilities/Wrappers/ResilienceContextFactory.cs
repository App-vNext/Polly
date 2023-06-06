using Polly.Utils;

namespace Polly.Utilities.Wrappers;

internal static class ResilienceContextFactory
{
    public static ResilienceContext Create(
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext,
        out IDictionary<string, object> oldProperties)
    {
        var resilienceContext = ResilienceContext.Get();
        resilienceContext.CancellationToken = cancellationToken;
        resilienceContext.ContinueOnCapturedContext = continueOnCapturedContext;
        resilienceContext.Properties.SetProperties(context, out oldProperties);

        return resilienceContext;
    }

    public static void Cleanup(ResilienceContext resilienceContext, IDictionary<string, object> oldProperties)
    {
        resilienceContext.Properties.SetProperties(oldProperties, out _);
        ResilienceContext.Return(resilienceContext);
    }
}
