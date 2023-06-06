namespace Polly;

/// <summary>
/// The pool of <see cref="ResilienceContext"/> instances.
/// </summary>
/// <remarks>
/// All members on this class are thread safe.
/// </remarks>
public abstract partial class ResilienceContextPool
{
    /// <summary>
    /// Gets the shared instance of <see cref="ResilienceContextPool"/>.
    /// </summary>
    public static readonly ResilienceContextPool Shared = new Pooled();

    /// <summary>
    /// Gets a <see cref="ResilienceContext"/> instance from the pool.
    /// </summary>
    /// <returns>An instance of <see cref="ResilienceContext"/>.</returns>
    /// <remarks>
    /// After the execution is finished you should return the <see cref="ResilienceContext"/> back to the pool
    /// by calling <see cref="Return(ResilienceContext)"/> method.
    /// </remarks>
    public abstract ResilienceContext Get();

    /// <summary>
    /// Returns a <paramref name="context"/> back to the pool.
    /// </summary>
    /// <param name="context">The context instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    public abstract void Return(ResilienceContext context);
}
