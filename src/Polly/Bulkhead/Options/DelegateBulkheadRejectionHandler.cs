#nullable enable
using System;

namespace Polly.Bulkhead.Options;

/// <summary>
/// The <see cref="DelegateBulkheadRejectionHandler"/> keeps the ability to pass delegates as a <see cref="BulkheadRejectionHandlerBase"/>.
/// </summary>
public class DelegateBulkheadRejectionHandler : BulkheadRejectionHandlerBase
{
    private readonly Action<Context> _onBulkheadRejected;

    /// <summary>
    /// Creates a new <see cref="DelegateBulkheadRejectionHandler"/> without an argument.
    /// </summary>
    /// <param name="onBulkheadRejected">The delegate which gets called if the bulkhead was rejected.</param>
    public DelegateBulkheadRejectionHandler(Action onBulkheadRejected)
    {
        if (onBulkheadRejected is null) throw new ArgumentNullException(nameof(onBulkheadRejected));
        _onBulkheadRejected = _ => onBulkheadRejected.Invoke();
    }

    /// <summary>
    /// Creates a new <see cref="DelegateBulkheadRejectionHandler"/>.
    /// </summary>
    /// <param name="onBulkheadRejected">The delegate which gets called if the bulkhead was rejected</param>
    /// <exception cref="ArgumentNullException">If the delegate is null</exception>
    public DelegateBulkheadRejectionHandler(Action<Context> onBulkheadRejected)
    {
        _onBulkheadRejected = onBulkheadRejected ?? throw new ArgumentNullException(nameof(onBulkheadRejected));
    }

    /// <inheritdoc />
    public override void OnBulkheadRejected(Context context) => _onBulkheadRejected.Invoke(context);
}