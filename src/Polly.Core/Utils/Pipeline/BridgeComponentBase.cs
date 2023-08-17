namespace Polly.Utils.Pipeline;

internal abstract class BridgeComponentBase : PipelineComponent
{
    private readonly object _strategy;

    protected BridgeComponentBase(object strategy) => _strategy = strategy;

    public override void Dispose()
    {
        if (_strategy is IDisposable disposable)
        {
            disposable.Dispose();
        }
        else if (_strategy is IAsyncDisposable asyncDisposable)
        {
            asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    public override ValueTask DisposeAsync()
    {
        if (_strategy is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }
        else
        {
            Dispose();
            return default;
        }
    }
}
