using Polly.Strategy;

namespace Polly.Retry;

/// <summary>
/// This class generates the customized retries used in retry strategy.
/// </summary>
/// <remarks>
/// If the generator returns a negative value, it's value is ignored.
/// </remarks>
public sealed class RetryDelayGenerator : OutcomeGenerator<TimeSpan, RetryDelayArguments, RetryDelayGenerator>
{
    /// <inheritdoc/>
    protected override TimeSpan DefaultValue => TimeSpan.MinValue;

    /// <inheritdoc/>
    protected override bool IsValid(TimeSpan value) => value >= TimeSpan.Zero;
}
