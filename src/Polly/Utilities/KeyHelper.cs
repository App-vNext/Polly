#nullable enable
namespace Polly.Utilities;

internal static class KeyHelper
{
    private const int GuidPartLength = 8;

    public static string GuidPart() =>
#if NET
        Guid.NewGuid().ToString()[..GuidPartLength];
#else
        Guid.NewGuid().ToString().Substring(0, GuidPartLength);
#endif
}
