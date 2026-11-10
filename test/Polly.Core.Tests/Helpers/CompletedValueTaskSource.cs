using System.Threading.Tasks.Sources;

namespace Polly.Core.Tests.Helpers;

/// <summary>
/// An already-completed <see cref="IValueTaskSource"/> that records how many times its result is consumed.
/// </summary>
internal sealed class CompletedValueTaskSource : IValueTaskSource, IValueTaskSource<int>
{
    public const int Value = 42;

    public int GetResultCalls { get; private set; }

    public ValueTask AsValueTask() => new(this, token: 0);

    public ValueTask<int> AsGenericValueTask() => new(this, token: 0);

    public ValueTaskSourceStatus GetStatus(short token) => ValueTaskSourceStatus.Succeeded;

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
        throw new NotSupportedException("The source is already completed.");

    void IValueTaskSource.GetResult(short token) => GetResultCalls++;

    int IValueTaskSource<int>.GetResult(short token)
    {
        GetResultCalls++;
        return Value;
    }
}
