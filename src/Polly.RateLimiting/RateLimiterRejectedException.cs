#if !NETCOREAPP
using System.Runtime.Serialization;
#endif
using System.Threading.RateLimiting;

using Polly.Telemetry;

namespace Polly.RateLimiting;

/// <summary>
/// Exception thrown when a rate limiter rejects an execution.
/// </summary>
#if !NETCOREAPP
[Serializable]
#endif
public sealed class RateLimiterRejectedException : ExecutionRejectedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    public RateLimiterRejectedException()
        : base("The operation could not be executed because it was rejected by the rate limiter.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="telemetrySource">The source pipeline and strategy names.</param>
    public RateLimiterRejectedException(ResilienceTelemetrySource telemetrySource)
        : base("The operation could not be executed because it was rejected by the rate limiter.")
        => TelemetrySource = ComposeTelemetrySource(telemetrySource);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="retryAfter">The retry after value.</param>
    public RateLimiterRejectedException(TimeSpan retryAfter)
        : base($"The operation could not be executed because it was rejected by the rate limiter. It can be retried after '{retryAfter}'.")
        => RetryAfter = retryAfter;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="telemetrySource">The source pipeline and strategy names.</param>
    /// <param name="retryAfter">The retry after value.</param>
    public RateLimiterRejectedException(ResilienceTelemetrySource telemetrySource, TimeSpan retryAfter)
        : base($"The operation could not be executed because it was rejected by the rate limiter. It can be retried after '{retryAfter}'.")
        => (TelemetrySource, RetryAfter) = (ComposeTelemetrySource(telemetrySource), retryAfter);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RateLimiterRejectedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="telemetrySource">The source pipeline and strategy names.</param>
    public RateLimiterRejectedException(string message, ResilienceTelemetrySource telemetrySource)
        : base(message)
        => TelemetrySource = ComposeTelemetrySource(telemetrySource);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="retryAfter">The retry after value.</param>
    public RateLimiterRejectedException(string message, TimeSpan retryAfter)
        : base(message)
        => RetryAfter = retryAfter;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="telemetrySource">The source pipeline and strategy names.</param>
    /// <param name="retryAfter">The retry after value.</param>
    public RateLimiterRejectedException(string message, ResilienceTelemetrySource telemetrySource, TimeSpan retryAfter)
        : base(message)
        => (TelemetrySource, RetryAfter) = (ComposeTelemetrySource(telemetrySource), retryAfter);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">The inner exception.</param>
    public RateLimiterRejectedException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="telemetrySource">The source pipeline and strategy names.</param>
    /// <param name="inner">The inner exception.</param>
    public RateLimiterRejectedException(string message, ResilienceTelemetrySource telemetrySource, Exception inner)
        : base(message, inner)
        => TelemetrySource = ComposeTelemetrySource(telemetrySource);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="retryAfter">The retry after value.</param>
    /// <param name="inner">The inner exception.</param>
    public RateLimiterRejectedException(string message, TimeSpan retryAfter, Exception inner)
        : base(message, inner)
        => RetryAfter = retryAfter;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="telemetrySource">The source pipeline and strategy names.</param>
    /// <param name="retryAfter">The retry after value.</param>
    /// <param name="inner">The inner exception.</param>
    public RateLimiterRejectedException(string message, ResilienceTelemetrySource telemetrySource, TimeSpan retryAfter, Exception inner)
        : base(message, inner)
        => (TelemetrySource, RetryAfter) = (ComposeTelemetrySource(telemetrySource), retryAfter);

    /// <summary>
    /// Gets the amount of time to wait before retrying again.
    /// </summary>
    /// <remarks>
    /// This value was retrieved from the <see cref="RateLimitLease"/> by reading the <see cref="MetadataName.RetryAfter"/>.
    /// Defaults to <c>null</c>.
    /// </remarks>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Gets the name of the strategy which has thrown the exception.
    /// </summary>
    public string? TelemetrySource { get; }

    private static string ComposeTelemetrySource(ResilienceTelemetrySource telemetrySource)
    {
        var pipelineName = telemetrySource?.PipelineName ?? "(null)";
        var pipelineInstanceName = telemetrySource?.PipelineInstanceName ?? "(null)";
        var strategyName = telemetrySource?.StrategyName ?? "(null)";
        return $"{pipelineName}/{pipelineInstanceName}/{strategyName}";
    }

#pragma warning disable RS0016 // Add public types and members to the declared API
#if !NETCOREAPP
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimiterRejectedException"/> class.
    /// </summary>
    /// <param name="info">The information.</param>
    /// <param name="context">The context.</param>
    private RateLimiterRejectedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        var retryAfter = info.GetDouble(nameof(RetryAfter));
        if (retryAfter >= 0.0)
        {
            RetryAfter = TimeSpan.FromSeconds(retryAfter);
        }

        var telemetrySource = info.GetString(nameof(TelemetrySource));
        if (telemetrySource is not null)
        {
            TelemetrySource = telemetrySource;
        }
    }

    /// <inheritdoc/>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        Guard.NotNull(info);

        if (TelemetrySource is not null)
        {
            info.AddValue(nameof(TelemetrySource), TelemetrySource);
        }
        else
        {
            info.AddValue(nameof(TelemetrySource), "(null)/(null)/(null)");
        }

        if (RetryAfter.HasValue)
        {
            info.AddValue(nameof(RetryAfter), RetryAfter.Value.TotalSeconds);
        }
        else
        {
            info.AddValue(nameof(RetryAfter), -1.0);
        }

        base.GetObjectData(info, context);
    }
#endif
#pragma warning restore RS0016 // Add public types and members to the declared API
}
