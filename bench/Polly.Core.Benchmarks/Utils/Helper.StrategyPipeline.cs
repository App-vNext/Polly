namespace Polly.Core.Benchmarks.Utils;

internal static partial class Helper
{
    public static object CreatePipeline(PollyVersion technology, int count) => technology switch
    {
        PollyVersion.V7 => count == 1 ? Policy.NoOpAsync<string>() : Policy.WrapAsync(Enumerable.Repeat(0, count).Select(_ => Policy.NoOpAsync<string>()).ToArray()),

        PollyVersion.V8 => CreateStrategy(builder =>
        {
            for (var i = 0; i < count; i++)
            {
                builder.AddStrategy(new EmptyResilienceStrategy<string>());
            }
        }),
        _ => throw new NotSupportedException()
    };
}
