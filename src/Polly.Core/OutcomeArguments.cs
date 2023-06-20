#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Polly
{
    /// <summary>
    /// Encapsulates the outcome of a resilience operation or event and its associated arguments.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the resilience operation or event.</typeparam>
    /// <typeparam name="TArgs">The type of the additional arguments associated with the specific resilience operation or event.</typeparam>
    /// <remarks>
    /// Always use the constructor when creating this struct, otherwise we do not guarantee binary compatibility.
    /// </remarks>
    public readonly struct OutcomeArguments<TResult, TArgs>
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutcomeArguments{TResult, TArgs}"/> struct.
        /// </summary>
        /// <param name="context">The context in which the resilience operation or event is occurring.</param>
        /// <param name="outcome">The outcome of the resilience operation or event.</param>
        /// <param name="arguments">Additional arguments specific to the resilience operation or event.</param>
        public OutcomeArguments(ResilienceContext context, Outcome<TResult> outcome, TArgs arguments)
        {
            Guard.NotNull(context);

            Context = context;
            Outcome = outcome;
            Arguments = arguments;
        }

        /// <summary>
        /// Gets the outcome of the resilience operation or event.
        /// </summary>
        public Outcome<TResult> Outcome { get; }

        /// <summary>
        /// Gets the context in which the resilience operation or event occurred.
        /// </summary>
        public ResilienceContext Context { get; }

        /// <summary>
        /// Gets additional arguments specific to the resilience operation or event.
        /// </summary>
        public TArgs Arguments { get; }

        /// <summary>
        /// Gets the exception, if any, thrown during the resilience operation or event.
        /// </summary>
        public Exception? Exception => Outcome.Exception;

        /// <summary>
        /// Gets the result, if any, produced by the resilience operation or event.
        /// </summary>
        public TResult? Result => Outcome.Result;
    }
}
