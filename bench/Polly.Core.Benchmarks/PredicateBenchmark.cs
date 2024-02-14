using System.Net;
using System.Net.Http;

namespace Polly.Core.Benchmarks;

public class PredicateBenchmark
{
    private readonly RetryPredicateArguments<HttpResponseMessage> _args = new(
        ResilienceContextPool.Shared.Get(),
        Outcome.FromResult(new HttpResponseMessage(HttpStatusCode.OK)),
        0);

    private readonly RetryStrategyOptions<HttpResponseMessage> _delegate = new()
    {
        ShouldHandle = args => args.Outcome switch
        {
            { Result: { StatusCode: HttpStatusCode.InternalServerError } } => PredicateResult.True(),
            { Exception: HttpRequestException } => PredicateResult.True(),
            { Exception: IOException } => PredicateResult.True(),
            { Exception: InvalidOperationException } => PredicateResult.False(),
            _ => PredicateResult.False(),
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
    public ValueTask<bool> Predicate_SwitchExpression() =>
        _delegate.ShouldHandle(_args);

    [Benchmark]
    public ValueTask<bool> Predicate_PredicateBuilder() =>
        _builder.ShouldHandle(_args);
}
