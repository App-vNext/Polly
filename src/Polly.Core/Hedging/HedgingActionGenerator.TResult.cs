namespace Polly.Hedging;

/// <summary>
/// Represents a generator that creates hedged actions.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
/// <param name="arguments">The arguments passed to the generator.</param>
/// <returns>A <see cref="Task{TResult}"/> that represents an asynchronous operation.</returns>
/// <remarks>
/// The generator can return a <see langword="null"/> function. In that case the hedging is not executed for that attempt.
/// Make sure that the returned action represents a real asynchronous work when invoked.
/// </remarks>
public delegate Func<Task<TResult>>? HedgingActionGenerator<TResult>(HedgingActionGeneratorArguments<TResult> arguments);
