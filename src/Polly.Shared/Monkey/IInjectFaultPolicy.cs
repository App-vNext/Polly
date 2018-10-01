namespace Polly.Monkey
{
    /// <summary>
    /// Defines properties and methods common to all InjectFault policies.
    /// </summary>

    public interface IInjectFaultPolicy : IsPolicy
    {
    }

    /// <summary>
    /// Defines properties and methods common to all InjectFault policies generic-typed for executions returning results of type <typeparamref name="TResult"/>.
    /// </summary>
    public interface IInjectFaultPolicy<TResult> : IInjectFaultPolicy
    {

    }
}
