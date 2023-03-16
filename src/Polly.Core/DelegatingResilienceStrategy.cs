namespace Polly;

/// <summary>
/// A resilience strategy that delegates the execution to the next strategy in the chain.
/// </summary>
public class DelegatingResilienceStrategy : ResilienceStrategy
{
    private bool _executed;
    private ResilienceStrategy _next = NullResilienceStrategy.Instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegatingResilienceStrategy"/> class.
    /// </summary>
    protected DelegatingResilienceStrategy()
    {
    }

    /// <summary>
    /// Gets or sets the next resilience strategy in the chain.
    /// </summary>
    /// <remarks>This property cannot be changed once the strategy is executed.</remarks>
    public ResilienceStrategy Next
    {
        get => _next;
        set
        {
            Guard.NotNull(value);

            if (_executed)
            {
                throw new InvalidOperationException($"The delegating resilience strategy has already been executed and changing the value of '{nameof(Next)}' property is not allowed.");
            }

            _next = value;
        }
    }

    /// <inheritdoc/>
    protected internal override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> callback,
        ResilienceContext context,
        TState state)
    {
        _executed = true;
        return Next.ExecuteCoreAsync(callback, context, state);
    }
}

