namespace Polly.Benchmarks;

#pragma warning disable CA1822 // Mark members as static

[Config(typeof(PollyConfig))]
public class Retry
{
    private static readonly Policy SyncPolicy = Policy.Handle<InvalidOperationException>().Retry();
    private static readonly AsyncPolicy AsyncPolicy = Policy.Handle<InvalidOperationException>().RetryAsync();

    [Benchmark]
    public void Retry_Synchronous_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Action());

    [Benchmark]
    public Task Retry_Asynchronous_Succeeds() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.ActionAsync());

    [Benchmark]
    public Task Retry_Asynchronous_Succeeds_With_CancellationToken() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.ActionAsync(token), CancellationToken.None);

    [Benchmark]
    public int Retry_Synchronous_With_Result_Succeeds() =>
        SyncPolicy.Execute(() => Workloads.Func<int>());

    [Benchmark]
    public Task<int> Retry_Asynchronous_With_Result_Succeeds() =>
        AsyncPolicy.ExecuteAsync(() => Workloads.FuncAsync<int>());

    [Benchmark]
    public Task<int> Retry_Asynchronous_With_Result_Succeeds_With_CancellationToken() =>
        AsyncPolicy.ExecuteAsync(token => Workloads.FuncAsync<int>(token), CancellationToken.None);

    [Benchmark]
    public void Retry_Synchronous_Throws_Then_Succeeds()
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
    public Task Retry_Asynchronous_Throws_Then_Succeeds()
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
