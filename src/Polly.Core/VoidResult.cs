namespace Polly;

/// <summary>
/// Class that represents a void result.
/// </summary>
internal sealed class VoidResult
{
    private VoidResult()
    {
    }

    public static readonly VoidResult Instance = new();

    public override string ToString() => "void";
}
