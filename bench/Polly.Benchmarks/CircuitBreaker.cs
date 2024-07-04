namespace Polly.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

[Config(typeof(PollyConfig))]
public class CircuitBreaker
{
    private static readonly Policy SyncPolicy = Policy.Handle<InvalidOperationException>().CircuitBreaker(2, TimeSpan.FromMinutes(1));
    private static readonly AsyncPolicy AsyncPolicy = Policy.Handle<InvalidOperationException>().CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

    [Benchmark]
    public void CircuitBreaker_Synchronous_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Action());

    [Benchmark]
    public Task CircuitBreaker_Asynchronous_Succeeds() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.ActionAsync(token), CancellationToken.None);

    [Benchmark]
    public int CircuitBreaker_Synchronous_With_Result_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Func<int>());

    [Benchmark]
    public Task<int> CircuitBreaker_Asynchronous_With_Result_Succeeds() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.FuncAsync<int>(token), CancellationToken.None);
}
