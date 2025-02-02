#nullable enable
namespace Polly.Utilities;

/// <summary>
/// Task helpers.
/// </summary>
public static class TaskHelper
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable S2223 // Non-constant static fields should not be visible
    /// <summary>
    /// Defines a completed Task for use as a completed, empty asynchronous delegate.
    /// </summary>
    public static Task EmptyTask = Task.CompletedTask;
#pragma warning restore
}
