using System.ComponentModel;
using Polly.Simmy.Utils;

namespace Polly.Simmy.Outcomes;

#pragma warning disable CA2225 // Operator overloads have named alternates
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters

/// <summary>
/// Generator that produces faults such as exceptions or results.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <remarks>
/// An instance of this class is assignable to <see cref="OutcomeStrategyOptions{TResult}.OutcomeGenerator"/>.
/// </remarks>
public sealed class OutcomeGenerator<TResult>
{
    private const int DefaultWeight = 100;
    private readonly GeneratorHelper<TResult> _helper;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutcomeGenerator{TResult}"/> class.
    /// </summary>
    public OutcomeGenerator()
        : this(RandomUtil.Instance.Next)
    {
    }

    internal OutcomeGenerator(Func<int, int> weightGenerator)
        => _helper = new GeneratorHelper<TResult>(weightGenerator);

    /// <summary>
    /// Registers an exception generator delegate.
    /// </summary>
    /// <param name="generator">The delegate that generates the exception.</param>
    /// <param name="weight">The weight assigned to this generator. Defaults to <c>100</c>.</param>
    /// <returns>The current instance of <see cref="OutcomeGenerator{TResult}"/>.</returns>
    public OutcomeGenerator<TResult> AddException(Func<Exception> generator, int weight = DefaultWeight)
    {
        Guard.NotNull(generator);

        _helper.AddOutcome(_ => Outcome.FromException<TResult>(generator()), weight);

        return this;
    }

    /// <summary>
    /// Registers an exception generator delegate that accepts a <see cref="ResilienceContext"/>.
    /// </summary>
    /// <param name="generator">The delegate that generates the exception, accepting a <see cref="ResilienceContext"/>.</param>
    /// <param name="weight">The weight assigned to this generator. Defaults to <c>100</c>.</param>
    /// <returns>The current instance of <see cref="OutcomeGenerator{TResult}"/>.</returns>
    public OutcomeGenerator<TResult> AddException(Func<ResilienceContext, Exception> generator, int weight = DefaultWeight)
    {
        Guard.NotNull(generator);

        _helper.AddOutcome(context => Outcome.FromException<TResult>(generator(context)), weight);

        return this;
    }

    /// <summary>
    /// Registers an exception generator for a specific exception type, using the default constructor of that exception.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to generate.</typeparam>
    /// <param name="weight">The weight assigned to this generator. Defaults to <c>100</c>.</param>
    /// <returns>The current instance of <see cref="OutcomeGenerator{TResult}"/>.</returns>
    public OutcomeGenerator<TResult> AddException<TException>(int weight = DefaultWeight)
        where TException : Exception, new()
    {
        _helper.AddOutcome(_ => Outcome.FromException<TResult>(new TException()), weight);

        return this;
    }

    /// <summary>
    /// Registers a result generator.
    /// </summary>
    /// <param name="generator">The delegate that generates the result.</param>
    /// <param name="weight">The weight assigned to this generator. Defaults to <c>100</c>.</param>
    /// <returns>The current instance of <see cref="OutcomeGenerator{TResult}"/>.</returns>
    public OutcomeGenerator<TResult> AddResult(Func<TResult> generator, int weight = DefaultWeight)
    {
        Guard.NotNull(generator);

        _helper.AddOutcome(_ => Outcome.FromResult(generator()), weight);

        return this;
    }

    /// <summary>
    /// Registers a result generator.
    /// </summary>
    /// <param name="generator">The delegate that generates the result, accepting a <see cref="ResilienceContext"/>.</param>
    /// <param name="weight">The weight assigned to this generator. Defaults to <c>100</c>.</param>
    /// <returns>The current instance of <see cref="OutcomeGenerator{TResult}"/>.</returns>
    public OutcomeGenerator<TResult> AddResult(Func<ResilienceContext, TResult> generator, int weight = DefaultWeight)
    {
        Guard.NotNull(generator);

        _helper.AddOutcome(context => Outcome.FromResult(generator(context)), weight);

        return this;
    }

    /// <summary>
    /// Implicit conversion to <see cref="OutcomeStrategyOptions{TResult}.OutcomeGenerator"/>.
    /// </summary>
    /// <param name="generator">The generator instance.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static implicit operator Func<OutcomeGeneratorArguments, ValueTask<Outcome<TResult>?>>(OutcomeGenerator<TResult> generator)
    {
        Guard.NotNull(generator);

        var generatorDelegate = generator._helper.CreateGenerator();

        return args => new ValueTask<Outcome<TResult>?>(generatorDelegate(args.Context));
    }
}

