namespace Polly.Timeout;

public class TimeoutStrategyOptions : ResilienceStrategyOptions
{
    public TimeoutStrategyOptions() => StrategyType = "Timeout";

    public TimeSpan TimeoutInterval { get; set; } = TimeSpan.FromSeconds(30);

    public Events<OnTimeoutArguments> OnTimeout { get; set; } = new();
}
