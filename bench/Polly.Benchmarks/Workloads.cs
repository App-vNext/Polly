namespace Polly.Benchmarks;

internal static class Workloads
{
    internal static void Action()
    {
    }

    internal static Task ActionAsync() =>
        Task.CompletedTask;

    internal static Task ActionAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    internal static async Task ActionInfiniteAsync()
    {
        while (true)
        {
            await Task.Yield();
        }
    }

    internal static async Task ActionInfiniteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Yield();
        }
    }

    internal static T Func<T>()
        where T : struct =>
        default;

    internal static Task<T> FuncAsync<T>()
        where T : struct =>
        Task.FromResult<T>(default);

    internal static Task<T> FuncAsync<T>(CancellationToken cancellationToken)
        where T : struct =>
        Task.FromResult<T>(default);

    internal static TResult FuncThrows<TResult, TException>()
        where TException : Exception, new() =>
        throw new TException();

    internal static Task<TResult> FuncThrowsAsync<TResult, TException>()
        where TException : Exception, new() =>
        throw new TException();
}