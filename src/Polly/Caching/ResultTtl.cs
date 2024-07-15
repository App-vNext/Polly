#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines a ttl strategy which can calculate a duration to cache items dynamically based on the execution context and result of the execution.
/// </summary>
/// <typeparam name="TResult">The type of results that the ttl calculation function will take as an input parameter.</typeparam>
public class ResultTtl<TResult> : ITtlStrategy<TResult>
{
    private readonly Func<Context, TResult?, Ttl> _ttlFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultTtl{TResult}"/> class, with a func calculating <see cref="Ttl"/> based on the <typeparamref name="TResult"/> value to cache.
    /// </summary>
    /// <param name="ttlFunc">The function to calculate the TTL for which cache items should be considered valid.</param>
    public ResultTtl(Func<TResult?, Ttl> ttlFunc)
    {
        if (ttlFunc == null)
        {
            throw new ArgumentNullException(nameof(ttlFunc));
        }

        _ttlFunc = (_, result) => ttlFunc(result);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultTtl{TResult}"/> class, with a func calculating <see cref="Ttl"/> based on the execution <see cref="Context"/> and <typeparamref name="TResult"/> value to cache.
    /// </summary>
    /// <param name="ttlFunc">The function to calculate the TTL for which cache items should be considered valid.</param>
    public ResultTtl(Func<Context, TResult?, Ttl> ttlFunc) =>
        _ttlFunc = ttlFunc ?? throw new ArgumentNullException(nameof(ttlFunc));

    /// <summary>
    /// Gets a TTL for the cacheable item.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="result">The execution result.</param>
    /// <returns>A <see cref="Ttl"/> representing the remaining Ttl of the cached item.</returns>
    public Ttl GetTtl(Context context, TResult? result) =>
        _ttlFunc(context, result);
}
