using System.ComponentModel;

namespace Polly;

/// <summary>
/// Legacy support for older versions of Polly.
/// </summary>
/// <remarks>
/// This class is used by the legacy Polly infrastructure and should not be used directly by user code.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class LegacySupport
{
    /// <summary>
    /// Changes the underlying properties of a <see cref="ResilienceProperties"/> instance.
    /// </summary>
    /// <param name="resilienceProperties">The resilience properties.</param>
    /// <param name="properties">The properties to use.</param>
    /// <param name="oldProperties">The old properties used by <paramref name="resilienceProperties"/>.</param>
    /// <remarks>
    /// This method is used by the legacy Polly infrastructure and should not be used directly by user code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetProperties(
        this ResilienceProperties resilienceProperties,
        IDictionary<string, object?> properties,
        out IDictionary<string, object?> oldProperties)
    {
        Guard.NotNull(resilienceProperties);
        Guard.NotNull(properties);

        oldProperties = resilienceProperties.Options;
        resilienceProperties.Options = properties;
    }
}
