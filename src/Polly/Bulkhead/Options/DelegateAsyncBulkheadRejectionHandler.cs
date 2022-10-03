#nullable enable
using System;
using System.Threading.Tasks;

namespace Polly.Bulkhead.Options;

/// <summary>
/// The <see cref="DelegateAsyncBulkheadRejectionHandler"/> keeps the ability to pass delegates as a <see cref="AsyncBulkheadRejectionHandlerBase"/>.
/// </summary>
public class DelegateAsyncBulkheadRejectionHandler : AsyncBulkheadRejectionHandlerBase
{
    private readonly Func<Context, Task> _onBulkheadRejected;

    /// <summary>
    /// Creates a new <see cref="DelegateBulkheadRejectionHandler"/> without an argument.
    /// </summary>
    /// <param name="onBulkheadRejected">The delegate which gets called if the bulkhead was rejected.</param>
    public DelegateAsyncBulkheadRejectionHandler(Func<Task> onBulkheadRejected)
    {
        if (onBulkheadRejected is null) throw new ArgumentNullException(nameof(onBulkheadRejected));
        _onBulkheadRejected = _ => onBulkheadRejected.Invoke();
    }

    /// <summary>
    /// Creates a new <see cref="DelegateBulkheadRejectionHandler"/>.
    /// </summary>
    /// <param name="onBulkheadRejected">The delegate which gets called if the bulkhead was rejected</param>
    /// <exception cref="ArgumentNullException">If the delegate is null</exception>
    public DelegateAsyncBulkheadRejectionHandler(Func<Context, Task> onBulkheadRejected)
    {
        _onBulkheadRejected = onBulkheadRejected ?? throw new ArgumentNullException(nameof(onBulkheadRejected));
    }

    /// <inheritdoc />
    public override Task OnBulkheadRejected(Context context) => _onBulkheadRejected.Invoke(context);
}