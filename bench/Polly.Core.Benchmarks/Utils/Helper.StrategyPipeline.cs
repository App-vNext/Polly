namespace Polly.Core.Benchmarks.Utils;

internal static partial class Helper
{
    public static object CreatePipeline(PollyVersion technology, int count) => technology switch
    {
        PollyVersion.V7 => count == 1 ? Policy.NoOpAsync<string>() : Policy.WrapAsync([.. Enumerable.Repeat(0, count).Select(_ => Policy.NoOpAsync<string>())]),

        PollyVersion.V8 => CreateStrategy(builder =>
        {
            for (var i = 0; i < count; i++)
            {
                builder.AddStrategy(static _ => new EmptyResilienceStrategy(), new EmptyResilienceOptions());
            }
        }),
        _ => throw new NotSupportedException()
    };
}
