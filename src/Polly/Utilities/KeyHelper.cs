#nullable enable
namespace Polly.Utilities;

internal static class KeyHelper
{
    private const int GuidPartLength = 8;

    public static string GuidPart() =>
        Guid.NewGuid().ToString().Substring(0, GuidPartLength);
}
