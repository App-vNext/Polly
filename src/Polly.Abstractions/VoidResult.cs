namespace Polly;

internal sealed class VoidResult
{
    private VoidResult()
    {
    }

    public static readonly VoidResult Instance = new();
}
