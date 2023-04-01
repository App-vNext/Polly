using Polly.Strategy;

namespace Polly.Retry;

/// <summary>
/// This class configures the predicates used by the retry strategy to determine if a retry should be performed.
/// </summary>
public sealed class ShouldRetryPredicate : OutcomePredicate<ShouldRetryArguments, ShouldRetryPredicate>
{
}
