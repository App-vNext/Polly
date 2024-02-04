namespace Polly.Core.Benchmarks.Utils;

internal static partial class Helper
{
    public static object CreateTimeout(PollyVersion technology)
    {
        var timeout = TimeSpan.FromSeconds(10);

        return technology switch
        {
            PollyVersion.V7 => Policy.TimeoutAsync<string>(timeout),
            PollyVersion.V8 => CreateStrategy(builder => builder.AddTimeout(timeout)),
            _ => throw new NotSupportedException()
        };
    }
}
