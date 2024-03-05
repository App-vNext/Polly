namespace Polly.Wrap;

/// <summary>
/// Defines properties and methods common to all PolicyWrap policies.
/// </summary>
public interface IPolicyWrap : IsPolicy
{
    /// <summary>
    /// Returns the outer <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>.
    /// </summary>
    IsPolicy Outer { get; }

    /// <summary>
    /// Returns the next inner <see cref="IsPolicy"/> in this <see cref="IPolicyWrap"/>.
    /// </summary>
    IsPolicy Inner { get; }
}

/// <summary>
/// Defines properties and methods common to all PolicyWrap policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
/// </summary>
public interface IPolicyWrap<TResult> : IPolicyWrap
{
}
