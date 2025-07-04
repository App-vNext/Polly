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

    protected static Outcome<TTo> ConvertOutcome<TFrom, TTo>(Outcome<TFrom> outcome)
    {
        if (outcome.ExceptionDispatchInfo is not null)
        {
            return new(outcome.ExceptionDispatchInfo);
        }

        return outcome.Result is null ? default : new((TTo)(object)outcome.Result);
    }
}
