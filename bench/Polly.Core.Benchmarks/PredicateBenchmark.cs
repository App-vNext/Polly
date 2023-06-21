using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Polly.Core.Benchmarks;

public class PredicateBenchmark
{
    private readonly OutcomeArguments<HttpResponseMessage, RetryPredicateArguments> _args = new(
        ResilienceContext.Get(),
        Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.OK)),
        new RetryPredicateArguments(0));

    private readonly RetryStrategyOptions<HttpResponseMessage> _delegate = new()
    {
        ShouldHandle = args => args switch
        {
            { Result: { StatusCode: HttpStatusCode.InternalServerError } } => PredicateResult.True,
            { Exception: HttpRequestException } => PredicateResult.True,
            { Exception: IOException } => PredicateResult.True,
            { Exception: InvalidOperationException } => PredicateResult.False,
            _ => PredicateResult.False,
        }
    };

    private readonly RetryStrategyOptions<HttpResponseMessage> _builder = new()
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .HandleResult(r => r.StatusCode == HttpStatusCode.InternalServerError)
            .Handle<HttpRequestException>()
            .Handle<InvalidOperationException>(e => false)
    };

    [Benchmark(Baseline = true)]
    public ValueTask<bool> Predicate_SwitchExpression()
    {
        return _delegate.ShouldHandle(_args);
    }

    [Benchmark]
    public ValueTask<bool> Predicate_PredicateBuilder()
    {
        return _builder.ShouldHandle(_args);
    }
}
