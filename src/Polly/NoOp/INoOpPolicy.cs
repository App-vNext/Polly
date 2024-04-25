#nullable enable

namespace Polly.NoOp;

/// <summary>
/// Defines properties and methods common to all NoOp policies.
/// </summary>
public interface INoOpPolicy : IsPolicy
{
}

/// <summary>
/// Defines properties and methods common to all NoOp policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface INoOpPolicy<TResult> : INoOpPolicy
{
}
