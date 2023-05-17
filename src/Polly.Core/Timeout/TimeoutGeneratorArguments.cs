using Polly.Strategy;

namespace Polly.Timeout;

/// <summary>
/// Arguments used by the timeout strategy to retrieve a timeout for current execution.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
public readonly record struct TimeoutGeneratorArguments(ResilienceContext Context) : IResilienceArguments;
