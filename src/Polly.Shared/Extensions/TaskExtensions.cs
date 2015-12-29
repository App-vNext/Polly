#if SUPPORTS_ASYNC

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Extensions
{
    internal static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable<T> NotOnCapturedContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable NotOnCapturedContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static async Task Continue(this Task task, bool onCapturedContext)
        {
            if (onCapturedContext)
            {
                await task;
                return;
            }

            await task.NotOnCapturedContext();
        }

        public static async Task<T> Continue<T>(this Task<T> task, bool onCapturedContext)
        {
            if (onCapturedContext)
            {
                return await task;
            }

            return await task.NotOnCapturedContext();
        }
    }
}

#endif