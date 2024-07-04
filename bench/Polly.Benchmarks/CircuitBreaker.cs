namespace Polly.Benchmarks;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable

[Config(typeof(PollyConfig))]
public class CircuitBreaker
{
    private static readonly Policy SyncPolicy = Policy.Handle<InvalidOperationException>().CircuitBreaker(2, TimeSpan.FromMinutes(1));
    private static readonly AsyncPolicy AsyncPolicy = Policy.Handle<InvalidOperationException>().CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));

    [Benchmark]
    public static void CircuitBreaker_Synchronous_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Action());

    [Benchmark]
    public static Task CircuitBreaker_Asynchronous_Succeeds() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.ActionAsync(token), CancellationToken.None);

    [Benchmark]
    public static int CircuitBreaker_Synchronous_With_Result_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Func<int>());

    [Benchmark]
    public static Task<int> CircuitBreaker_Asynchronous_With_Result_Succeeds() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.FuncAsync<int>(token), CancellationToken.None);
}
