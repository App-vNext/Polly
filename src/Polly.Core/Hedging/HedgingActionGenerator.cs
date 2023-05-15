namespace Polly.Hedging;

/// <summary>
/// Represents a generator that creates void-based hedged actions.
/// </summary>
/// <param name="arguments">The arguments passed to the generator.</param>
/// <returns>A <see cref="Task"/> that represents an asynchronous operation.</returns>
/// <remarks>
/// The generator can return a <see langword="null"/> function. In that case the hedging is not executed for that attempt.
/// Make sure that the returned action represents a real asynchronous work when invoked.
/// </remarks>
public delegate Func<Task>? HedgingActionGenerator(HedgingActionGeneratorArguments arguments);
