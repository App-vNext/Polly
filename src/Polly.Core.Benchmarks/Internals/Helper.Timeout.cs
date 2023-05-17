#pragma warning disable S4225 // Extension methods should not extend "object"

using System;

namespace Polly.Core.Benchmarks;

internal static partial class Helper
{
    public static object CreateTimeout(PollyVersion technology)
    {
        var timeout = TimeSpan.FromSeconds(10);

        return technology switch
        {
            PollyVersion.V7 => Policy.TimeoutAsync<int>(timeout),
            PollyVersion.V8 => CreateStrategy(builder => builder.AddTimeout(timeout)),
            _ => throw new NotSupportedException()
        };
    }
}
