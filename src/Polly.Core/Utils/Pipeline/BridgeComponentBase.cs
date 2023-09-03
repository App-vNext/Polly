namespace Polly.Utils.Pipeline;

internal abstract class BridgeComponentBase : PipelineComponent
{
    private readonly object _strategy;

    protected BridgeComponentBase(object strategy) => _strategy = strategy;

    public override ValueTask DisposeAsync()
    {
        if (_strategy is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }
        else if (_strategy is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return default;
    }
}
