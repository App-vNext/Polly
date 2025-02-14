namespace Polly;

/// <summary>
/// An interface defining resilience execution, it works together with Polly ResiliencePipeline as callback.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TInput">The type of the input.</typeparam>
public interface IInvoker
    <TResult, TInput>
{
    /// <summary>
    /// Execute the request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>The result.</returns>
    Task<TResult> ExecuteAsync(TInput request, CancellationToken cancellationToken);
}
