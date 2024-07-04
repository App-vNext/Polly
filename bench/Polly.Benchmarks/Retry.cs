namespace Polly.Benchmarks;

#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable

[Config(typeof(PollyConfig))]
public class Retry
{
    private static readonly Policy SyncPolicy = Policy.Handle<InvalidOperationException>().Retry();
    private static readonly AsyncPolicy AsyncPolicy = Policy.Handle<InvalidOperationException>().RetryAsync();

    [Benchmark]
    public static void Retry_Synchronous_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Action());

    [Benchmark]
    public static Task Retry_Asynchronous_Succeeds() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.ActionAsync());

    [Benchmark]
    public static Task Retry_Asynchronous_Succeeds_With_CancellationToken() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.ActionAsync(token), CancellationToken.None);

    [Benchmark]
    public static int Retry_Synchronous_With_Result_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Func<int>());

    [Benchmark]
    public static Task<int> Retry_Asynchronous_With_Result_Succeeds() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());

    [Benchmark]
    public static Task<int> Retry_Asynchronous_With_Result_Succeeds_With_CancellationToken() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.FuncAsync<int>(token), CancellationToken.None);

    [Benchmark]
    public static void Retry_Synchronous_Throws_Then_Succeeds()
    {
        int count = 0;

        SyncPolicy.Execute(() =>
        {
            if (count++ % 2 == 0)
            {
                throw new InvalidOperationException();
            }
        });
    }

    [Benchmark]
    public static Task Retry_Asynchronous_Throws_Then_Succeeds()
    {
        int count = 0;

        return AsyncPolicy.ExecuteAsync(() =>
        {
            if (count++ % 2 == 0)
            {
                throw new InvalidOperationException();
            }

            return Task.CompletedTask;
        });
    }
}
