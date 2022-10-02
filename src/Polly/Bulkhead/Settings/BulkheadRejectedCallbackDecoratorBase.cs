using System;

namespace Polly.Bulkhead.Settings;

/// <summary>
/// The <see cref="BulkheadRejectedCallbackDecoratorBase"/> is a base type which should be the base of all decorator.
/// </summary>
public abstract class BulkheadRejectedCallbackDecoratorBase : IBulkheadRejectedCallback
{
    private readonly IBulkheadRejectedCallback _decorated;

    /// <summary>
    /// Creates a new <see cref="BulkheadRejectedCallbackDecoratorBase"/>
    /// </summary>
    /// <param name="decorated">The decorated instance</param>
    /// <exception cref="ArgumentNullException">if the decorated instance is null</exception>
    protected BulkheadRejectedCallbackDecoratorBase(IBulkheadRejectedCallback decorated)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
    }
    
    /// <inheritdoc />
    public virtual void OnBulkheadRejected(Context context) => _decorated.OnBulkheadRejected(context);
}