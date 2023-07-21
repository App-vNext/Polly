namespace Polly;

#pragma warning disable CA1716 // Identifiers should not match keywords
#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters

/// <summary>
/// The pool of <see cref="ResilienceContext"/> instances.
/// </summary>
public abstract partial class ResilienceContextPool
{
    /// <summary>
    /// Gets the shared pool instance.
    /// </summary>
    public static ResilienceContextPool Shared { get; } = new SharedPool();

    /// <summary>
    /// Gets a <see cref="ResilienceContext"/> instance from the pool.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of <see cref="ResilienceContext"/>.</returns>
    /// <remarks>
    /// After the execution is finished you should return the <see cref="ResilienceContext"/> back to the pool
    /// by calling <see cref="Return(ResilienceContext)"/> method.
    /// </remarks>
    public ResilienceContext Get(CancellationToken cancellationToken = default) => Get(null, cancellationToken);

    /// <summary>
    /// Gets a <see cref="ResilienceContext"/> instance from the pool.
    /// </summary>
    /// <param name="operationKey">An operation key associated with the context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of <see cref="ResilienceContext"/>.</returns>
    /// <remarks>
    /// After the execution is finished you should return the <see cref="ResilienceContext"/> back to the pool
    /// by calling <see cref="Return(ResilienceContext)"/> method.
    /// </remarks>
    public abstract ResilienceContext Get(string? operationKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a <paramref name="context"/> back to the pool.
    /// </summary>
    /// <param name="context">The context instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    public abstract void Return(ResilienceContext context);
}
