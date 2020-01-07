namespace Polly
{
    internal interface IResultPredicates<TResult>
    {
        ResultPredicates<TResult> PredicatesInternal { get; }
    }
}
