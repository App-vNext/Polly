using System.ComponentModel;
using Polly.Simmy.Utils;

namespace Polly.Simmy.Fault;

#pragma warning disable CA2225 // Operator overloads have named alternates
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters

/// <summary>
/// A generator for creating faults (exceptions) using registered delegate functions.
/// </summary>
/// <remarks>
/// An instance of this class can be assigned to the <see cref="ChaosFaultStrategyOptions.FaultGenerator"/> property.
/// </remarks>
public sealed class FaultGenerator
{
    private const int DefaultWeight = 100;

    private readonly GeneratorHelper<VoidResult> _helper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FaultGenerator"/> class.
    /// </summary>
    public FaultGenerator() => _helper = new(RandomUtil.Next);

    /// <summary>
    /// Registers an exception generator delegate.
    /// </summary>
    /// <param name="generator">The delegate that generates the exception.</param>
    /// <param name="weight">The weight assigned to this generator. Defaults to <c>100</c>.</param>
    /// <returns>The current instance of <see cref="FaultGenerator"/>.</returns>
    public FaultGenerator AddException(Func<Exception> generator, int weight = DefaultWeight)
    {
        Guard.NotNull(generator);

        _helper.AddOutcome(_ => Outcome.FromException<VoidResult>(generator()), weight);

        return this;
    }

    /// <summary>
    /// Registers an exception generator delegate that accepts a <see cref="ResilienceContext"/>.
    /// </summary>
    /// <param name="generator">The delegate that generates the exception, accepting a <see cref="ResilienceContext"/>.</param>
    /// <param name="weight">The weight assigned to this generator. Defaults to <c>100</c>.</param>
    /// <returns>The current instance of <see cref="FaultGenerator"/>.</returns>
    public FaultGenerator AddException(Func<ResilienceContext, Exception> generator, int weight = DefaultWeight)
    {
        Guard.NotNull(generator);

        _helper.AddOutcome(context => Outcome.FromException<VoidResult>(generator(context)), weight);

        return this;
    }

    /// <summary>
    /// Registers an exception generator for a specific exception type, using the default constructor of that exception.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to generate.</typeparam>
    /// <param name="weight">The weight assigned to this generator. Defaults to <c>100</c>.</param>
    /// <returns>The current instance of <see cref="FaultGenerator"/>.</returns>
    public FaultGenerator AddException<TException>(int weight = DefaultWeight)
        where TException : Exception, new()
    {
        _helper.AddOutcome(_ => Outcome.FromException<VoidResult>(new TException()), weight);

        return this;
    }

    /// <summary>
    /// Provides an implicit conversion from <see cref="FaultGenerator"/> to a delegate compatible with <see cref="ChaosFaultStrategyOptions.FaultGenerator"/>.
    /// </summary>
    /// <param name="generator">The instance of <see cref="FaultGenerator"/>.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<FaultGeneratorArguments, ValueTask<Exception?>>(FaultGenerator generator)
    {
        Guard.NotNull(generator);

        var generatorDelegate = generator._helper.CreateGenerator();

        return args => new ValueTask<Exception?>(generatorDelegate(args.Context)!.Value.Exception);
    }
}
