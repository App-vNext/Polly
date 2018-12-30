namespace Polly.Monkey
{
    /// <summary>
    /// Defines properties and methods common to all Monkey policies.
    /// </summary>
    public interface IMonkeyPolicy : IsPolicy
    {
    }

    /// <summary>
    /// Defines properties and methods common to all Monkey policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
    /// </summary>
    public interface IMonkeyPolicy<TResult> : IMonkeyPolicy
    {
    }
}
