namespace Polly.Core.Tests.Helpers;

internal class DisposableResult : IDisposable
{
    public readonly TaskCompletionSource<bool> OnDisposed = new();

    public DisposableResult() => Name = "";

    public DisposableResult(string name) => Name = name;

    public string Name { get; set; }

    public bool IsDisposed { get; private set; }

    public Task WaitForDisposalAsync() => OnDisposed.Task;

    public void Dispose()
    {
        IsDisposed = true;

        OnDisposed.TrySetResult(true);
    }

    public override string ToString() => Name;
}
