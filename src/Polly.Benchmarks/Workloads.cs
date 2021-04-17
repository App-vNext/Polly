using System.Threading;
using System.Threading.Tasks;

namespace Polly.Benchmarks
{
    internal static class Workloads
    {
        internal static void Action()
        {
        }

        internal static Task ActionAsync() => Task.CompletedTask;

        internal static Task ActionAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        internal static T Func<T>() => default;

        internal static Task<T> FuncAsync<T>() => Task.FromResult<T>(default);

        internal static Task<T> FuncAsync<T>(CancellationToken cancellationToken) => Task.FromResult<T>(default);
    }
}
