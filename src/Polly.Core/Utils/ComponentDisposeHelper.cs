namespace Polly.Utils;

internal sealed class ComponentDisposeHelper : IDisposable, IAsyncDisposable
{
    private readonly PipelineComponent _component;
    private readonly DisposeBehavior _disposeBehavior;
    private bool _disposed;

    public ComponentDisposeHelper(PipelineComponent component, DisposeBehavior disposeBehavior)
    {
        _component = component;
        _disposeBehavior = disposeBehavior;
    }

    public void Dispose()
    {
        if (EnsureDisposable())
        {
            ForceDispose();
        }
    }

    public ValueTask DisposeAsync()
    {
        if (EnsureDisposable())
        {
            return ForceDisposeAsync();
        }

        return default;
    }

    public void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException("ResiliencePipeline", "This resilience pipeline has been disposed and cannot be used anymore.");
        }
    }

    public void ForceDispose()
    {
        _disposed = true;
#pragma warning disable S2952 // Classes should "Dispose" of members from the classes' own "Dispose" methods
        _component.Dispose();
#pragma warning restore S2952 // Classes should "Dispose" of members from the classes' own "Dispose" methods
    }

    public ValueTask ForceDisposeAsync()
    {
        _disposed = true;
        return _component.DisposeAsync();
    }

    private bool EnsureDisposable()
    {
        if (_disposeBehavior == DisposeBehavior.Ignore)
        {
            return false;
        }

        if (_disposeBehavior == DisposeBehavior.Reject)
        {
            throw new InvalidOperationException("Disposing this resilience pipeline is not allowed because it is owned by the pipeline registry.");
        }

        return !_disposed;
    }
}
