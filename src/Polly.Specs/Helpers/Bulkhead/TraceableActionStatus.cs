namespace Polly.Specs.Helpers.Bulkhead
{
    /// <summary>
    /// States of a <see cref="TraceableAction"/> that can be tracked during testing.
    /// </summary>
    public enum TraceableActionStatus
    {
        Unstarted,
        StartRequested,
        QueueingForSemaphore,
        Executing,
        Rejected,
        Canceled,
        Faulted,
        Completed,
    }
}