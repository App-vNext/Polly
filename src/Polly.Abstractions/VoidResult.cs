namespace Polly;

internal class VoidResult
{
    private VoidResult()
    {
    }

    public static readonly VoidResult Instance = new();
}
