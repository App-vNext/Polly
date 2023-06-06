namespace Polly.Utilities.Wrappers;

internal static class ResilienceContextFactory
{
    public static readonly ResiliencePropertyKey<Context> ContextKey = new("Polly.V7.Context");

    public static ResilienceContext Create(Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        var resilienceContext = ResilienceContext.Get();
        resilienceContext.CancellationToken = cancellationToken;
        resilienceContext.ContinueOnCapturedContext = continueOnCapturedContext;

        foreach (var pair in context)
        {
            var props = (IDictionary<string, object>)resilienceContext.Properties;
            props.Add(pair.Key, pair.Value);
        }

        resilienceContext.Properties.Set(ContextKey, context);

        return resilienceContext;
    }

    public static void Restore(ResilienceContext context)
    {
        var originalContext = context.GetContext();

        foreach (var pair in context.Properties)
        {
            if (pair.Key == ContextKey.Key)
            {
                continue;
            }

            originalContext[pair.Key] = pair.Value;
        }

        ResilienceContext.Return(context);
    }

    public static Context GetContext(this ResilienceContext resilienceContext) => resilienceContext.Properties.GetValue(ContextKey, null!);
}
