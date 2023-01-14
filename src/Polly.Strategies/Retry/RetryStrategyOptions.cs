namespace Polly.Retry;

public class RetryStrategyOptions : ResilienceStrategyOptions
{
    public Predicates<ShouldRetryArguments> ShouldRetry { get; set; } = new();

    public Events<RetryActionArguments> OnRetry { get; set; } = new();

    public int RetryCount { get; set; } = 3;

    public Func<int, TimeSpan> RetryDelayGenerator { get; set; } = _ => TimeSpan.FromSeconds(3);
}
