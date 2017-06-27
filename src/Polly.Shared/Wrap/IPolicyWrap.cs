namespace Polly.Wrap
{
    /// <summary>
    /// Defines properties and methods common to all PolicyWrap policies.
    /// </summary>

    public interface IPolicyWrap : IsPolicy
    {
    }

    /// <summary>
    /// Defines properties and methods common to all PolicyWrap policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
    /// </summary>
    public interface IPolicyWrap<TResult> : IPolicyWrap
    {

    }
}
