namespace Polly.Timeout;

public class TimeoutStrategyOptions : ResilienceStrategyOptions
{
    public TimeoutStrategyOptions() => StrategyType = "Retry";

    public TimeSpan TimeoutInterval { get; set; } = TimeSpan.FromSeconds(30);

    public Events<TimeoutTaskArguments> OnTimeout { get; set; } = new();
}
