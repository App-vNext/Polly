using System;

namespace Polly.Bulkhead.Settings;

/// <summary>
/// The <see cref="DelegateBulkheadRejectedCallback"/> keeps the ability to pass delegates as a <see cref="IBulkheadRejectedCallback"/>.
/// </summary>
public class DelegateBulkheadRejectedCallback : IBulkheadRejectedCallback
{
    private readonly Action<Context> _onBulkheadRejected;

    private static Action<Context> WrapCallback(Action onBulkheadRejected)
    {
        onBulkheadRejected = onBulkheadRejected ?? throw new ArgumentNullException(nameof(onBulkheadRejected));
        return _ => onBulkheadRejected.Invoke();
    }

    /// <summary>
    /// Creates a new <see cref="DelegateBulkheadRejectedCallback"/> without an argument.
    /// </summary>
    /// <param name="onBulkheadRejected">The delegate which gets called if the bulkhead was rejected.</param>
    public DelegateBulkheadRejectedCallback(Action onBulkheadRejected) : this(WrapCallback(onBulkheadRejected)) { }

    /// <summary>
    /// Creates a new <see cref="DelegateBulkheadRejectedCallback"/>.
    /// </summary>
    /// <param name="onBulkheadRejected">The delegate which gets called if the bulkhead was rejected</param>
    /// <exception cref="ArgumentNullException">If the delegate is null</exception>
    public DelegateBulkheadRejectedCallback(Action<Context> onBulkheadRejected)
    {
        _onBulkheadRejected = onBulkheadRejected ?? throw new ArgumentNullException(nameof(onBulkheadRejected));
    }

    /// <inheritdoc />
    public void OnBulkheadRejected(Context context) => _onBulkheadRejected.Invoke(context);
}