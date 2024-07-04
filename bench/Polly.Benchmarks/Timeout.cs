namespace Polly.Benchmarks;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable

[Config(typeof(PollyConfig))]
public class Timeout
{
    private static readonly Policy SyncPolicy = Policy.Timeout(TimeSpan.FromMilliseconds(1));
    private static readonly AsyncPolicy AsyncPolicy = Policy.TimeoutAsync(TimeSpan.FromMilliseconds(1));

    [Benchmark]
    public static void Timeout_Synchronous_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Action());

    [Benchmark]
    public static Task Timeout_Asynchronous_Succeeds() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.ActionAsync());

    [Benchmark]
    public static Task Timeout_Asynchronous_Succeeds_With_CancellationToken() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.ActionAsync(token), CancellationToken.None);

    [Benchmark]
    public static int Timeout_Synchronous_With_Result_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Func<int>());

    [Benchmark]
    public static Task<int> Timeout_Asynchronous_With_Result_Succeeds() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());

    [Benchmark]
    public static Task<int> Timeout_Asynchronous_With_Result_Succeeds_With_CancellationToken() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.FuncAsync<int>(token), CancellationToken.None);

    [Benchmark]
    public static Task Timeout_Asynchronous_Times_Out_Optimistic() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.ActionInfiniteAsync(token), CancellationToken.None);
}
