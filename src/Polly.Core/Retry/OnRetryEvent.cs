using Polly.Strategy;

namespace Polly.Retry;

/// <summary>
/// This class holds the user-callbacks that are invoked on retries.
/// </summary>
public sealed class OnRetryEvent : OutcomeEvent<OnRetryArguments, OnRetryEvent>
{
}
