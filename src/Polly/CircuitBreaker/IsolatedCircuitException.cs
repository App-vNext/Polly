namespace Polly.CircuitBreaker;

/// <summary>
/// Exception thrown when a circuit is isolated (held open) by manual override.
/// </summary>
public class IsolatedCircuitException : BrokenCircuitException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IsolatedCircuitException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public IsolatedCircuitException(string message) : base(message) { }
}
