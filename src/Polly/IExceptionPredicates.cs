namespace Polly
{
    internal interface IExceptionPredicates
    {
        ExceptionPredicates PredicatesInternal { get; }
    }
}
