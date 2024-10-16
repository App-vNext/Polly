namespace Polly.Utils;

#pragma warning disable S3236 // Remove this argument from the method call; it hides the caller information.
#pragma warning disable S5034 // "ValueTask" should be consumed correctly

internal static class TaskHelper
{
    public static void GetResult(this ValueTask<VoidResult> task)
    {
        Debug.Assert(
            task.IsCompleted,
            "The value task should be already completed at this point. If not, it's an indication that the strategy does not respect the ResilienceContext.IsSynchronous value.");

        // Stryker disable once boolean : no means to test this
        if (task.IsCompleted)
        {
            _ = task.Result;
            return;
        }

        task.Preserve().GetAwaiter().GetResult();
    }

    public static TResult GetResult<TResult>(this ValueTask<TResult> task)
    {
        Debug.Assert(
            task.IsCompleted,
            "The value task should be already completed at this point. If not, it's an indication that the strategy does not respect the ResilienceContext.IsSynchronous value.");

        // Stryker disable once boolean : no means to test this
        if (task.IsCompleted)
        {
            return task.Result;
        }

        return task.Preserve().GetAwaiter().GetResult();
    }
}
