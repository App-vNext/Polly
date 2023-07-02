namespace Polly.Simmy;

/// <summary>
/// Contains common functionality for chaos strategies which intentionally disrupt executions - which monkey around with calls.
/// </summary>
/// <typeparam name="TResult">The type of result this strategy supports.</typeparam>
public abstract class MonkeyStrategy<TResult> : MonkeyStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MonkeyStrategy{TResult}"/> class.
    /// </summary>
    /// <param name="options">The chaos strategy options.</param>
    protected MonkeyStrategy(MonkeyStrategyOptions options)
        : base(options)
    {
    }
}
