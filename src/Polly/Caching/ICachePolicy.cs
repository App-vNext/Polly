#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines properties and methods common to all Cache policies.
/// </summary>
public interface ICachePolicy : IsPolicy
{
}

/// <summary>
/// Defines properties and methods common to all Cache policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface ICachePolicy<TResult> : ICachePolicy
{
}
