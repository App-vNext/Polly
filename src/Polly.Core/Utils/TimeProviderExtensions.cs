namespace Polly.Utils;

/// <summary>
/// Extensions for <see cref="TimeProvider"/> used by resilience strategies.
/// </summary>
internal static class TimeProviderExtensions
{
    /// <summary>
    /// Delays the execution for the specified time span.
    /// </summary>
    /// <param name="timeProvider">The instance of <see cref="TimeProvider"/>.</param>
    /// <param name="delay">For how long we should delay.</param>
    /// <param name="context">The resilience context.</param>
    /// <returns>The task.</returns>
    /// <remarks>
    /// The delay is performed synchronously if the <see cref="ResilienceContext.IsSynchronous"/> property is true, otherwise the delay is performed asynchronously.
    /// This method will be public later.
    /// </remarks>
    public static Task DelayAsync(this TimeProvider timeProvider, TimeSpan delay, ResilienceContext context)
    {
        Guard.NotNull(timeProvider);
        Guard.NotNull(context);

        context.CancellationToken.ThrowIfCancellationRequested();

        if (context.IsSynchronous && timeProvider == TimeProvider.System)
        {
            if (context.CancellationToken.CanBeCanceled)
            {
                context.CancellationToken.WaitHandle.WaitOne(delay);
                context.CancellationToken.ThrowIfCancellationRequested();
            }
            else
            {
                Thread.Sleep(delay);
            }

            return Task.CompletedTask;
        }
        else
        {
            if (context.IsSynchronous)
            {
#pragma warning disable CA1849 // For synchronous scenarios we want to return completed task
                timeProvider.Delay(delay, context.CancellationToken).GetAwaiter().GetResult();
#pragma warning restore CA1849

                return Task.CompletedTask;
            }

            return timeProvider.Delay(delay, context.CancellationToken);
        }
    }
}
